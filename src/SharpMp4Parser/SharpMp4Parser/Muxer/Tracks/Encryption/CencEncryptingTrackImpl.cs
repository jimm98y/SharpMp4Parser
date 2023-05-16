using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15;
using SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Boxes.SampleGrouping;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;
using System.Numerics;
using SharpMp4Parser.Muxer.Tracks.H264;
using SharpMp4Parser.Muxer.Tracks.H265;
using System.Linq;

namespace SharpMp4Parser.Muxer.Tracks.Encryption
{
    /**
      * Encrypts a given track with common encryption.
      */
    public class CencEncryptingTrackImpl : CencEncryptedTrack
    {

        private Track source;
        private CencEncryptingSampleList samples;
        private List<CencSampleAuxiliaryDataFormat> cencSampleAuxiliaryData;
        private HashSet<SampleEntry> sampleEntries = new HashSet<SampleEntry>();
        private bool subSampleEncryption;
        private object configurationBox;
        private Dictionary<GroupEntry, long[]> groupEntries = new Dictionary<GroupEntry, long[]>();

        public CencEncryptingTrackImpl(Track source, Uuid defaultKeyId, SecretKey key, bool dummyIvs) :
                this(source, new RangeStartMap<int, Uuid>() { { 0, defaultKeyId } }, new Dictionary<Uuid, SecretKey>() { { defaultKeyId, key } }, "cenc", dummyIvs, false)
        { }


        /**
         * Encrypts a given source track.
         *
         * @param source             unencrypted source file
         * @param indexToKeyId      dunno
         * @param keys               key ID to key map
         * @param encryptionAlgo     cenc or cbc1 (don't use cbc1)
         * @param dummyIvs           disables RNG for IVs and use IVs starting with 0x00...000
         * @param encryptButAllClear will cause sub sample encryption format to keep full sample in clear (clear/encrypted pair will be len(sample)/0
         */
        public CencEncryptingTrackImpl(Track source, RangeStartMap<int, Uuid> indexToKeyId, Dictionary<Uuid, SecretKey> keys,
                                       string encryptionAlgo, bool dummyIvs, bool encryptButAllClear)
        {
            this.source = source;
            this.cencSampleAuxiliaryData = new List<CencSampleAuxiliaryDataFormat>();

            BigInteger one = new BigInteger(1);
            byte[] init = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };

            if (!dummyIvs)
            {
#warning use secure random
                Random random = new Random();
                random.NextBytes(init);
            }
            BigInteger ivInt = new BigInteger(init);

            CencEncryptingSampleEntryTransformer tx = new CencEncryptingSampleEntryTransformer();

            Dictionary<SampleEntry, int> nalLengthSizes = new Dictionary<SampleEntry, int>();
            foreach (SampleEntry sampleEntry in source.getSampleEntries())
            {

                List<Box> boxes = sampleEntry.getBoxes();

                foreach (Box box in boxes)
                {
                    if (box is AvcConfigurationBox) {
                        AvcConfigurationBox avcC = (AvcConfigurationBox)(configurationBox = box);
                        nalLengthSizes[sampleEntry] = avcC.getLengthSizeMinusOne() + 1;
                        subSampleEncryption = true;
                    } 
                    else if (box is HevcConfigurationBox) {
                        HevcConfigurationBox hvcC = (HevcConfigurationBox)(configurationBox = box);
                        nalLengthSizes[sampleEntry] = hvcC.getLengthSizeMinusOne() + 1;
                        subSampleEncryption = true;
                    } 
                    else
                    {
                        if (!nalLengthSizes.ContainsKey(sampleEntry))
                        {
                            nalLengthSizes.Add(sampleEntry, -1);
                        }
                    }
                }
            }

            IList<Sample> sourceSamples = source.getSamples();
            RangeStartMap<int, SampleEntry> indexToSampleEntry = new RangeStartMap<int, SampleEntry>();
            RangeStartMap<int, KeyIdKeyPair> indexToKey = new RangeStartMap<int, KeyIdKeyPair>();
            SampleEntry previousSampleEntry = null;
            for (int i = 0; i < sourceSamples.Count; i++)
            {
                Sample origSample = sourceSamples[i];
                int nalLengthSize = nalLengthSizes[origSample.getSampleEntry()];
                CencSampleAuxiliaryDataFormat e = new CencSampleAuxiliaryDataFormat();
                this.cencSampleAuxiliaryData.Add(e);
                Uuid keyId = indexToKeyId[i];
                if (keyId != null)
                {
                    SampleEntry correct = tx.transform(origSample.getSampleEntry(), encryptionAlgo, indexToKeyId[i]);
                    sampleEntries.Add(correct);
                    if (previousSampleEntry != correct)
                    {
                        indexToSampleEntry.Add(i, correct);
                        indexToKey.Add(i, new KeyIdKeyPair(keyId, keys[indexToKeyId[i]]));
                    }
                    previousSampleEntry = correct;

                    byte[] iv = ivInt.ToByteArray();
                    byte[] eightByteIv = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                    System.Array.Copy(
                            iv,
                            iv.Length - 8 > 0 ? iv.Length - 8 : 0,
                            eightByteIv,
                            (8 - iv.Length) < 0 ? 0 : (8 - iv.Length),
                            iv.Length > 8 ? 8 : iv.Length);

                    e.iv = eightByteIv;

                    ByteBuffer sample = (ByteBuffer)((Java.Buffer)origSample.asByteBuffer()).rewind();

                    if (nalLengthSize > 0)
                    {
                        if (encryptButAllClear)
                        {
                            e.pairs = new CencSampleAuxiliaryDataFormat.Pair[] { e.createPair(sample.remaining(), 0) };
                        }
                        else
                        {
                            List<CencSampleAuxiliaryDataFormat.Pair> pairs = new List<CencSampleAuxiliaryDataFormat.Pair>(5);
                            while (sample.remaining() > 0)
                            {
                                int nalLength = CastUtils.l2i(IsoTypeReaderVariable.read(sample, nalLengthSize));
                                int clearBytes;
                                int nalGrossSize = nalLength + nalLengthSize;
                                if (nalGrossSize < 112 || isClearNal((ByteBuffer)sample.duplicate()))
                                {
                                    clearBytes = nalGrossSize;
                                }
                                else
                                {
                                    clearBytes = 96 + nalGrossSize % 16;
                                }
                                pairs.Add(e.createPair(clearBytes, nalGrossSize - clearBytes));
                                ((Java.Buffer)sample).position(sample.position() + nalLength);
                            }
                            e.pairs = pairs.ToArray();
                        }
                    }

                    ivInt = ivInt + one;
                }
                else
                {

                    SampleEntry correct = origSample.getSampleEntry();
                    sampleEntries.Add(correct);
                    if (previousSampleEntry != correct)
                    {
                        indexToSampleEntry.Add(i, correct);
                        indexToKey.Add(i, null);
                    }
                    previousSampleEntry = correct;
                }
            }

            this.samples = new CencEncryptingSampleList(indexToKey, indexToSampleEntry, source.getSamples(), cencSampleAuxiliaryData);
        }

        public bool hasSubSampleEncryption()
        {
            return subSampleEncryption;
        }

        public List<CencSampleAuxiliaryDataFormat> getSampleEncryptionEntries()
        {
            return cencSampleAuxiliaryData;
        }

        private readonly object _syncRoot = new object();

        public List<SampleEntry> getSampleEntries()
        {
            lock (_syncRoot)
            {
                return new List<SampleEntry>(sampleEntries);
            }
        }

        public long[] getSampleDurations()
        {
            return source.getSampleDurations();
        }

        public long getDuration()
        {
            return source.getDuration();
        }

        public List<CompositionTimeToSample.Entry> getCompositionTimeEntries()
        {
            return source.getCompositionTimeEntries();
        }

        public long[] getSyncSamples()
        {
            return source.getSyncSamples();
        }

        public List<SampleDependencyTypeBox.Entry> getSampleDependencies()
        {
            return source.getSampleDependencies();
        }

        public TrackMetaData getTrackMetaData()
        {
            return source.getTrackMetaData();
        }

        public string getHandler()
        {
            return source.getHandler();
        }

        public IList<Sample> getSamples()
        {
            return samples;
        }

        public SubSampleInformationBox getSubsampleInformationBox()
        {
            return source.getSubsampleInformationBox();
        }

        public void close()
        {
            source.close();
        }

        public string getName()
        {
            return "enc(" + source.getName() + ")";
        }

        public List<Edit> getEdits()
        {
            return source.getEdits();
        }

        public virtual Dictionary<GroupEntry, long[]> getSampleGroups()
        {
            return groupEntries;
        }

        private bool isClearNal(ByteBuffer s)
        {
            if (configurationBox is HevcConfigurationBox)
            {
                H265NalUnitHeader nuh = H265TrackImpl.getNalUnitHeader(s.slice());
                return !( // These ranges are all slices --> NOT CLEAR
                        (nuh.nalUnitType >= H265NalUnitTypes.NAL_TYPE_TRAIL_N && (nuh.nalUnitType <= H265NalUnitTypes.NAL_TYPE_RASL_R)) ||
                                (nuh.nalUnitType >= H265NalUnitTypes.NAL_TYPE_BLA_W_LP && (nuh.nalUnitType <= H265NalUnitTypes.NAL_TYPE_CRA_NUT)) ||
                                (nuh.nalUnitType >= H265NalUnitTypes.NAL_TYPE_BLA_W_LP && (nuh.nalUnitType <= H265NalUnitTypes.NAL_TYPE_CRA_NUT))
                );
            } 
            else if (configurationBox is AvcConfigurationBox)
            {
                // only encrypt
                H264NalUnitHeader nuh = H264TrackImpl.getNalUnitHeader(s.slice());
                return !(nuh.nal_unit_type == H264NalUnitTypes.CODED_SLICE_AUX_PIC ||
                        nuh.nal_unit_type == H264NalUnitTypes.CODED_SLICE_DATA_PART_A ||
                        nuh.nal_unit_type == H264NalUnitTypes.CODED_SLICE_DATA_PART_B ||
                        nuh.nal_unit_type == H264NalUnitTypes.CODED_SLICE_DATA_PART_C ||
                        nuh.nal_unit_type == H264NalUnitTypes.CODED_SLICE_EXT ||
                        nuh.nal_unit_type == H264NalUnitTypes.CODED_SLICE_IDR ||
                        nuh.nal_unit_type == H264NalUnitTypes.CODED_SLICE_NON_IDR
                );

            }
            else
            {
                throw new Exception("Subsample encryption is activated but the CencEncryptingTrackImpl can't say if this sample is to be encrypted or not!");
            }
        }
    }
}
