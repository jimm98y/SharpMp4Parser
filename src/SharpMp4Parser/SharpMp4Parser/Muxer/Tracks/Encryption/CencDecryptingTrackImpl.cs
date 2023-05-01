using SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Boxes.SampleGrouping;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharpMp4Parser.Muxer.Tracks.Encryption
{
    public class CencDecryptingTrackImpl : AbstractTrack
    {
        private CencDecryptingSampleList samples;
        private CencEncryptedTrack original;
        private HashSet<SampleEntry> sampleEntries = new HashSet<SampleEntry>();

        public CencDecryptingTrackImpl(CencEncryptedTrack original, SecretKey sk) : base("dec(" + original.getName() + ")")
        {

            this.original = original;
            Dictionary<Uuid, SecretKey> keys = new Dictionary<Uuid, SecretKey>();
            foreach (SampleEntry sampleEntry in original.getSampleEntries())
            {
                TrackEncryptionBox tenc = Path.getPath((IsoParser.Container)sampleEntry, "sinf[0]/schi[0]/tenc[0]");
                Debug.Assert(tenc != null);
                keys.Add(tenc.getDefault_KID(), sk);
            }
            init(keys);
        }

        public CencDecryptingTrackImpl(CencEncryptedTrack original, Dictionary<Uuid, SecretKey> keys) : base("dec(" + original.getName() + ")")
        {
            this.original = original;
            init(keys);
        }

        private void init(Dictionary<Uuid, SecretKey> keys)
        {
            CencDecryptingSampleEntryTransformer tx = new CencDecryptingSampleEntryTransformer();
            List<Sample> encSamples = original.getSamples();

            RangeStartMap<int, SecretKey> indexToKey = new RangeStartMap<int, SecretKey>();
            RangeStartMap<int, SampleEntry> indexToSampleEntry = new RangeStartMap<int, SampleEntry>();
            SampleEntry previousSampleEntry = null;

            for (int i = 0; i < encSamples.Count; i++)
            {
                Sample encSample = encSamples[i];
                SampleEntry current = encSample.getSampleEntry();
                sampleEntries.Add(tx.transform(encSample.getSampleEntry()));
                if (previousSampleEntry != current)
                {
                    indexToSampleEntry.Add(i, current);
                    TrackEncryptionBox tenc = Path.getPath((IsoParser.Container)encSample.getSampleEntry(), "sinf[0]/schi[0]/tenc[0]");
                    if (tenc != null)
                    {
                        indexToKey.Add(i, keys[tenc.getDefault_KID()]);
                    }
                    else
                    {
                        indexToKey.Add(i, null);
                    }
                }
                previousSampleEntry = current;
            }

            samples = new CencDecryptingSampleList(indexToKey, indexToSampleEntry, encSamples, original.getSampleEncryptionEntries());
        }

        public override void close()
        {
            original.close();
        }

        public override long[] getSyncSamples()
        {
            return original.getSyncSamples();
        }

        public override List<SampleEntry> getSampleEntries()
        {
            return new List<SampleEntry>(sampleEntries);
        }

        public override long[] getSampleDurations()
        {
            return original.getSampleDurations();
        }

        public override TrackMetaData getTrackMetaData()
        {
            return original.getTrackMetaData();
        }

        public override string getHandler()
        {
            return original.getHandler();
        }

        public override List<Sample> getSamples()
        {
            return samples;
        }

        public override Dictionary<GroupEntry, long[]> getSampleGroups()
        {
            return original.getSampleGroups();
        }
    }
}
