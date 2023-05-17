using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpMp4Parser.Muxer.Tracks.Encryption;

namespace SharpMp4Parser.Muxer
{
    /**
     * This track implementation is to be used when MP4 track is CENC encrypted.
     */
    public class CencMp4TrackImplImpl : Mp4TrackImpl, CencEncryptedTrack
    {

        private List<CencSampleAuxiliaryDataFormat> sampleEncryptionEntries;

        /**
         * Creates a track from a TrackBox and potentially fragments. Use <b>fragements parameter
         * only</b> to supply additional fragments that are not located in the main file.
         *
         * @param trackId      ID of the track to extract
         * @param isofile      the parsed MP4 file
         * @param randomAccess the RandomAccessSource to read the samples from
         * @param name         an arbitrary naem to identify track later - e.g. filename
         * @throws java.io.IOException if reading from underlying <code>DataSource</code> fails
         */
        public CencMp4TrackImplImpl(long trackId, IsoParser.Container isofile, RandomAccessSource randomAccess, string name) : base(trackId, isofile, randomAccess, name)
        {
            TrackBox trackBox = null;
            foreach (TrackBox box in Path.getPaths<TrackBox>(isofile, "moov/trak"))
            {
                if (box.getTrackHeaderBox().getTrackId() == trackId)
                {
                    trackBox = box;
                    break;
                }
            }
            Debug.Assert(trackBox != null);
            List<SchemeTypeBox> schms = Path.getPaths<SchemeTypeBox>((IsoParser.Container)trackBox, "mdia[0]/minf[0]/stbl[0]/stsd[0]/enc./sinf[0]/schm[0]");
            foreach (SchemeTypeBox schm in schms)
            {
                Debug.Assert(schm != null && (schm.getSchemeType().Equals("cenc") || schm.getSchemeType().Equals("cbc1")), "Track must be CENC (cenc or cbc1) encrypted");
            }
            List<SampleEntry> sampleEntries = trackBox.getMediaBox().getMediaInformationBox().getSampleTableBox().getSampleDescriptionBox().getBoxes<SampleEntry>(typeof(SampleEntry));
            Debug.Assert(trackBox.getMediaBox().getMediaInformationBox().getSampleTableBox().getSampleDescriptionBox().getBoxes().Count == sampleEntries.Count);

            sampleEncryptionEntries = new List<CencSampleAuxiliaryDataFormat>();

            List<MovieExtendsBox> movieExtendsBoxes = Path.getPaths<MovieExtendsBox>(isofile, "moov/mvex");
            if (movieExtendsBoxes.Count != 0)
            {
                foreach (MovieFragmentBox movieFragmentBox in isofile.getBoxes< MovieFragmentBox>(typeof(MovieFragmentBox)))
                {
                    List<TrackFragmentBox> trafs = movieFragmentBox.getBoxes<TrackFragmentBox>(typeof(TrackFragmentBox));
                    foreach (TrackFragmentBox traf in trafs)
                    {
                        if (traf.getTrackFragmentHeaderBox().getTrackId() == trackId)
                        {
                            long baseOffset;
                            if (traf.getTrackFragmentHeaderBox().hasBaseDataOffset())
                            {
                                baseOffset = traf.getTrackFragmentHeaderBox().getBaseDataOffset();
                            }
                            else
                            {
                                List<Box>.Enumerator it = isofile.getBoxes().GetEnumerator();
                                baseOffset = 0;
                                it.MoveNext();
                                for (Box b = it.Current; b != movieFragmentBox; b = it.Current)
                                {
                                    baseOffset += b.getSize();
                                    it.MoveNext();
                                }
                            }
                            TrackEncryptionBox tenc = Path.getPath<TrackEncryptionBox>((IsoParser.Container)sampleEntries[CastUtils.l2i(traf.getTrackFragmentHeaderBox().getSampleDescriptionIndex() - 1)], "sinf[0]/schi[0]/tenc[0]");


                            FindSaioSaizPair saizSaioPair = new FindSaioSaizPair(traf).invoke();
                            SampleAuxiliaryInformationOffsetsBox saio = saizSaioPair.getSaio();
                            SampleAuxiliaryInformationSizesBox saiz = saizSaioPair.getSaiz();
                            // now we have the correct saio/saiz combo!
                            Debug.Assert(saio != null);
                            long[] saioOffsets = saio.getOffsets();
                            Debug.Assert(saioOffsets.Length == traf.getBoxes<TrackRunBox>(typeof(TrackRunBox)).Count);
                            Debug.Assert(saiz != null);

                            List<TrackRunBox> truns = traf.getBoxes<TrackRunBox>(typeof(TrackRunBox));
                            int sampleNo = 0;
                            for (int i = 0; i < saioOffsets.Length; i++)
                            {
                                int numSamples = truns[i].getEntries().Count;
                                long offset = saioOffsets[i];
                                long length = 0;

                                for (int j = sampleNo; j < sampleNo + numSamples; j++)
                                {
                                    length += saiz.getSize(j);
                                }
                                ByteBuffer trunsCencSampleAuxData = randomAccess.get(baseOffset + offset, length);
                                for (int j = sampleNo; j < sampleNo + numSamples; j++)
                                {
                                    int auxInfoSize = saiz.getSize(j);
                                    if (tenc != null)
                                    {
                                        sampleEncryptionEntries.Add(
                                                parseCencAuxDataFormat(tenc.getDefaultIvSize(), trunsCencSampleAuxData, auxInfoSize)
                                        );
                                    }
                                    else
                                    {
                                        sampleEncryptionEntries.Add(new CencSampleAuxiliaryDataFormat());
                                    }
                                }
                                sampleNo += numSamples;
                            }
                        }
                    }

                }
            }
            else
            {

                ChunkOffsetBox chunkOffsetBox = Path.getPath<ChunkOffsetBox>(trackBox, "mdia[0]/minf[0]/stbl[0]/stco[0]");

                if (chunkOffsetBox == null)
                {
                    chunkOffsetBox = Path.getPath<ChunkOffsetBox>(trackBox, "mdia[0]/minf[0]/stbl[0]/co64[0]");
                }

                Debug.Assert(chunkOffsetBox != null);
                long[] chunkSizes = trackBox.getSampleTableBox().getSampleToChunkBox().blowup(chunkOffsetBox.getChunkOffsets().Length);


                FindSaioSaizPair saizSaioPair = new FindSaioSaizPair((IsoParser.Container)Path.getPath<IsoParser.Container>(trackBox, "mdia[0]/minf[0]/stbl[0]")).invoke();
                SampleAuxiliaryInformationOffsetsBox saio = saizSaioPair.saio;
                SampleAuxiliaryInformationSizesBox saiz = saizSaioPair.saiz;
                SampleEntry se = null;
                TrackEncryptionBox tenc = null;
                IList<Sample> samples = this.getSamples();

                if (saio.getOffsets().Length == 1)
                {
                    long offset = saio.getOffsets()[0];
                    int sizeInTotal = 0;
                    if (saiz.getDefaultSampleInfoSize() > 0)
                    {
                        sizeInTotal += saiz.getSampleCount() * saiz.getDefaultSampleInfoSize();
                    }
                    else
                    {
                        for (int i = 0; i < saiz.getSampleCount(); i++)
                        {
                            sizeInTotal += saiz.getSampleInfoSizes()[i];
                        }
                    }

                    ByteBuffer chunksCencSampleAuxData = randomAccess.get(offset, sizeInTotal);

                    for (int i = 0; i < saiz.getSampleCount(); i++)
                    {
                        long auxInfoSize = saiz.getSize(i);
                        SampleEntry _se = samples[i].getSampleEntry();
                        if (se != _se)
                        {
                            tenc = Path.getPath<TrackEncryptionBox>((IsoParser.Container)_se, "sinf[0]/schi[0]/tenc[0]");
                        }
                        se = _se;
                        if (tenc != null)
                        {
                            sampleEncryptionEntries.Add(
                                    parseCencAuxDataFormat(tenc.getDefaultIvSize(), chunksCencSampleAuxData, auxInfoSize)
                            );
                        }
                        else
                        {
                            sampleEncryptionEntries.Add(new CencSampleAuxiliaryDataFormat());
                        }
                    }

                }
                else if (saio.getOffsets().Length == chunkSizes.Length)
                {
                    int currentSampleNo = 0;


                    for (int i = 0; i < chunkSizes.Length; i++)
                    {
                        long offset = saio.getOffsets()[i];
                        long size = 0;
                        if (saiz.getDefaultSampleInfoSize() > 0)
                        {
                            size += saiz.getSampleCount() * chunkSizes[i];
                        }
                        else
                        {
                            for (int j = 0; j < chunkSizes[i]; j++)
                            {
                                size += saiz.getSize(currentSampleNo + j);
                            }
                        }

                        ByteBuffer chunksCencSampleAuxData = randomAccess.get(offset, size);
                        for (int j = 0; j < chunkSizes[i]; j++)
                        {
                            long auxInfoSize = saiz.getSize(currentSampleNo + j);
                            SampleEntry _se = samples[currentSampleNo + j].getSampleEntry();
                            if (se != _se)
                            {
                                tenc = Path.getPath<TrackEncryptionBox>((IsoParser.Container)_se, "sinf[0]/schi[0]/tenc[0]");
                            }
                            se = _se;
                            if (tenc != null)
                            {
                                sampleEncryptionEntries.Add(
                                        parseCencAuxDataFormat(tenc.getDefaultIvSize(), chunksCencSampleAuxData, auxInfoSize)
                                );
                            }
                            else
                            {
                                sampleEncryptionEntries.Add(new CencSampleAuxiliaryDataFormat());
                            }
                        }
                        currentSampleNo += (int)chunkSizes[i];
                    }
                }
                else
                {
                    throw new Exception("Number of saio offsets must be either 1 or number of chunks");
                }
            }
        }

        private CencSampleAuxiliaryDataFormat parseCencAuxDataFormat(int ivSize, ByteBuffer chunksCencSampleAuxData, long auxInfoSize)
        {
            CencSampleAuxiliaryDataFormat cadf = new CencSampleAuxiliaryDataFormat();
            if (auxInfoSize > 0)
            {
                cadf.iv = new byte[ivSize];
                chunksCencSampleAuxData.get(cadf.iv);
                if (auxInfoSize > ivSize)
                {
                    int numOfPairs = IsoTypeReader.readUInt16(chunksCencSampleAuxData);
                    cadf.pairs = new CencSampleAuxiliaryDataFormat.Pair[numOfPairs];
                    for (int i = 0; i < cadf.pairs.Length; i++)
                    {
                        cadf.pairs[i] = cadf.createPair(
                                IsoTypeReader.readUInt16(chunksCencSampleAuxData),
                                IsoTypeReader.readUInt32(chunksCencSampleAuxData));
                    }
                }
            }
            return cadf;
        }


        public bool hasSubSampleEncryption()
        {
            return false;
        }

        public List<CencSampleAuxiliaryDataFormat> getSampleEncryptionEntries()
        {
            return sampleEncryptionEntries;
        }

        public override string ToString()
        {
            return "CencMp4TrackImpl{" +
                    "handler='" + getHandler() + '\'' +
                    '}';
        }

        public override string getName()
        {
            return "enc(" + base.getName() + ")";
        }

        public class FindSaioSaizPair
        {
            public IsoParser.Container container;
            public SampleAuxiliaryInformationSizesBox saiz;
            public SampleAuxiliaryInformationOffsetsBox saio;

            public FindSaioSaizPair(IsoParser.Container container)
            {
                this.container = container;
            }

            public SampleAuxiliaryInformationSizesBox getSaiz()
            {
                return saiz;
            }

            public SampleAuxiliaryInformationOffsetsBox getSaio()
            {
                return saio;
            }

            public FindSaioSaizPair invoke()
            {
                List<SampleAuxiliaryInformationSizesBox> saizs = container.getBoxes< SampleAuxiliaryInformationSizesBox>(typeof(SampleAuxiliaryInformationSizesBox));
                List<SampleAuxiliaryInformationOffsetsBox> saios = container.getBoxes< SampleAuxiliaryInformationOffsetsBox>(typeof(SampleAuxiliaryInformationOffsetsBox));
                Debug.Assert(saizs.Count == saios.Count);
                saiz = null;
                saio = null;

                for (int i = 0; i < saizs.Count; i++)
                {
                    if (saiz == null && (saizs[i].getAuxInfoType() == null) || "cenc".Equals(saizs[i].getAuxInfoType()))
                    {
                        saiz = saizs[i];
                    }
                    else if (saiz != null && saiz.getAuxInfoType() == null && "cenc".Equals(saizs[i].getAuxInfoType()))
                    {
                        saiz = saizs[i];
                    }
                    else
                    {
                        throw new Exception("Are there two cenc labeled saiz?");
                    }
                    if (saio == null && (saios[i].getAuxInfoType() == null) || "cenc".Equals(saios[i].getAuxInfoType()))
                    {
                        saio = saios[i];
                    }
                    else if (saio != null && saio.getAuxInfoType() == null && "cenc".Equals(saios[i].getAuxInfoType()))
                    {
                        saio = saios[i];
                    }
                    else
                    {
                        throw new Exception("Are there two cenc labeled saio?");
                    }
                }
                return this;
            }
        }
    }
}