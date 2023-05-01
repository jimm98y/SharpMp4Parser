/*
 * Copyright 2012 Sebastian Annies, Hamburg
 *
 * Licensed under the Apache License, Version 2.0 (the License);
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an AS IS BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Boxes.SampleGrouping;
using System.Collections.Generic;

namespace SharpMp4Parser.Muxer
{
    /**
     *
     */
    public abstract class AbstractTrack : Track
    {
        public string name;
        public List<Edit> edits = new List<Edit>();
        public Dictionary<GroupEntry, long[]> sampleGroups = new Dictionary<GroupEntry, long[]>();

        public AbstractTrack(string name)
        {
            this.name = name;
        }

        public virtual List<CompositionTimeToSample.Entry> getCompositionTimeEntries()
        {
            return null;
        }

        public virtual long[] getSyncSamples()
        {
            return null;
        }

        public virtual List<SampleDependencyTypeBox.Entry> getSampleDependencies()
        {
            return null;
        }

        public virtual SubSampleInformationBox getSubsampleInformationBox()
        {
            return null;
        }

        public virtual long getDuration()
        {
            long duration = 0;
            foreach (long delta in getSampleDurations())
            {
                duration += delta;
            }
            return duration;
        }

        public virtual string getName()
        {
            return this.name;
        }

        public virtual List<Edit> getEdits()
        {
            return edits;
        }

        public virtual Dictionary<GroupEntry, long[]> getSampleGroups()
        {
            return sampleGroups;
        }

        public abstract List<SampleEntry> getSampleEntries();
        public abstract long[] getSampleDurations();
        public abstract TrackMetaData getTrackMetaData();
        public abstract string getHandler();
        public abstract List<Sample> getSamples();
        public virtual void close() { }
    }
}
