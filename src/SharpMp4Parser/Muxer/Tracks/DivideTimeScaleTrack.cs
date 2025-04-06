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

namespace SharpMp4Parser.Muxer.Tracks
{
    /**
     * Changes the timescale of a track by wrapping the track.
     */
    public class DivideTimeScaleTrack : Track
    {
        Track source;
        private int timeScaleDivisor;

        public DivideTimeScaleTrack(Track source, int timeScaleDivisor)
        {
            this.source = source;
            this.timeScaleDivisor = timeScaleDivisor;
        }

        public void close()
        {
            source.close();
        }

        public List<SampleEntry> getSampleEntries()
        {
            return source.getSampleEntries();
        }

        public long[] getSampleDurations()
        {
            long[] scaled = new long[source.getSampleDurations().Length];


            for (int i = 0; i < source.getSampleDurations().Length; i++)
            {
                scaled[i] = source.getSampleDurations()[i] / timeScaleDivisor;
            }
            return scaled;
        }

        public List<CompositionTimeToSample.Entry> getCompositionTimeEntries()
        {
            return adjustCtts();
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
            TrackMetaData trackMetaData = (TrackMetaData)source.getTrackMetaData().Clone();
            trackMetaData.setTimescale(source.getTrackMetaData().getTimescale() / this.timeScaleDivisor);
            return trackMetaData;
        }

        public string getHandler()
        {
            return source.getHandler();
        }


        public IList<Sample> getSamples()
        {
            return source.getSamples();
        }


        List<CompositionTimeToSample.Entry> adjustCtts()
        {
            List<CompositionTimeToSample.Entry> origCtts = this.source.getCompositionTimeEntries();
            if (origCtts != null)
            {
                List<CompositionTimeToSample.Entry> entries2 = new List<CompositionTimeToSample.Entry>(origCtts.Count);
                foreach (CompositionTimeToSample.Entry entry in origCtts)
                {
                    entries2.Add(new CompositionTimeToSample.Entry(entry.getCount(), entry.getOffset() / timeScaleDivisor));
                }
                return entries2;
            }
            else
            {
                return null;
            }
        }

        public SubSampleInformationBox getSubsampleInformationBox()
        {
            return source.getSubsampleInformationBox();
        }

        public long getDuration()
        {
            long duration = 0;
            foreach (long delta in getSampleDurations())
            {
                duration += delta;
            }
            return duration;
        }

        public override string ToString()
        {
            return "MultiplyTimeScaleTrack{" +
                    "source=" + source +
                    '}';
        }

        public string getName()
        {
            return "timscale(" + source.getName() + ")";
        }

        public List<Edit> getEdits()
        {
            return source.getEdits();
        }

        public Dictionary<GroupEntry, long[]> getSampleGroups()
        {
            return source.getSampleGroups();
        }
    }
}
