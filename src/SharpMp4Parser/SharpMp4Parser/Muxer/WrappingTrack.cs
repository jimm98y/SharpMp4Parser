using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Boxes.SampleGrouping;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Muxer
{
    /**
 * A simple track wrapper that delegates all calls to parent track. Override certain methods inline to change result.
 */
    public class WrappingTrack : Track
    {
        private Track parent;

        public WrappingTrack(Track parent)
        {
            this.parent = parent;
        }

        public List<SampleEntry> getSampleEntries()
        {
            return parent.getSampleEntries();
        }

        public long[] getSampleDurations()
        {
            return parent.getSampleDurations();
        }

        public long getDuration()
        {
            return parent.getDuration();
        }

        public List<CompositionTimeToSample.Entry> getCompositionTimeEntries()
        {
            return parent.getCompositionTimeEntries();
        }

        public long[] getSyncSamples()
        {
            return parent.getSyncSamples();
        }

        public List<SampleDependencyTypeBox.Entry> getSampleDependencies()
        {
            return parent.getSampleDependencies();
        }

        public TrackMetaData getTrackMetaData()
        {
            return parent.getTrackMetaData();
        }

        public string getHandler()
        {
            return parent.getHandler();
        }

        public List<Sample> getSamples()
        {
            return parent.getSamples();
        }

        public SubSampleInformationBox getSubsampleInformationBox()
        {
            return parent.getSubsampleInformationBox();
        }

        public string getName()
        {
            return parent.getName() + "'";
        }

        public List<Edit> getEdits()
        {
            return parent.getEdits();
        }

        public void close()
        {
            parent.close();
        }

        public Dictionary<GroupEntry, long[]> getSampleGroups()
        {
            return parent.getSampleGroups();
        }
    }
}
