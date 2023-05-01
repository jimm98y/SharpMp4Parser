using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Boxes.SampleGrouping;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharpMp4Parser.Muxer.Tracks
{
    /**
     * This is just a basic idea how things could work but they don't.
     */
    public class SilenceTrackImpl : Track
    {
        private Track source;

        private List<Sample> samples = new List<Sample>();
        private long[] decodingTimes;
        private string name;

        public SilenceTrackImpl(Track ofType, long ms)
        {
            source = ofType;
            name = "" + ms + "ms silence";
            Debug.Assert(ofType.getSampleEntries().Count == 1, "");
            if ("mp4a".Equals(ofType.getSampleEntries()[0].getType()))
            {
                int numFrames = CastUtils.l2i(getTrackMetaData().getTimescale() * ms / 1000 / 1024);
                decodingTimes = new long[numFrames];
                Arrays.fill(decodingTimes, getTrackMetaData().getTimescale() * ms / numFrames / 1000);

                while (numFrames-- > 0)
                {
                    samples.Add(new SampleImpl((ByteBuffer)((Java.Buffer)ByteBuffer.wrap(new byte[] {
                            0x21, 0x10, 0x04, 0x60, (byte) 0x8c, 0x1c,
                    })).rewind(), ofType.getSampleEntries()[0]));
                }
            }
            else
            {
                throw new Exception("Tracks of type " + ofType.GetType().Name + " are not supported");
            }
        }

        public void close()
        {
            // nothing to close
        }

        public List<SampleEntry> getSampleEntries()
        {
            return source.getSampleEntries();
        }

        public long[] getSampleDurations()
        {
            return decodingTimes;
        }

        public long getDuration()
        {
            long duration = 0;
            foreach (long delta in decodingTimes)
            {
                duration += delta;
            }
            return duration;
        }

        public TrackMetaData getTrackMetaData()
        {
            return source.getTrackMetaData();
        }

        public string getHandler()
        {
            return source.getHandler();
        }


        public List<Sample> getSamples()
        {
            return samples;
        }

        public SubSampleInformationBox getSubsampleInformationBox()
        {
            return null;
        }

        public List<CompositionTimeToSample.Entry> getCompositionTimeEntries()
        {
            return null;
        }

        public long[] getSyncSamples()
        {
            return null;
        }

        public List<SampleDependencyTypeBox.Entry> getSampleDependencies()
        {
            return null;
        }

        public string getName()
        {
            return name;
        }

        public List<Edit> getEdits()
        {
            return null;
        }

        public Dictionary<GroupEntry, long[]> getSampleGroups()
        {
            return source.getSampleGroups();
        }
    }
}
