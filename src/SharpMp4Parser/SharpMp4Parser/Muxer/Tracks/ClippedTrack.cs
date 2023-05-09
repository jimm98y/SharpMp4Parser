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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SharpMp4Parser.Muxer.Tracks
{
    /**
     * Generates a Track that starts at fromSample and ends at toSample (exclusive). The user of this class
     * has to make sure that the fromSample is a random access sample.
     * <ul>
     * <li>In AAC and most other audio formats this is every single sample</li>
     * <li>In H264 this is every sample that is marked in the SyncSampleBox</li>
     * </ul>
     */
    public class ClippedTrack : AbstractTrack
    {
        private Track origTrack;
        private int fromSample;
        private int toSample;

        /**
         * Wraps an existing track and masks out a number of samples.
         * Works like {@link java.util.List#subList(int, int)}.
         *
         * @param origTrack  the original <code>Track</code>
         * @param fromSample first sample in the new <code>Track</code> - beginning with 0
         * @param toSample   first sample not in the new <code>Track</code> - beginning with 0
         */
        public ClippedTrack(Track origTrack, long fromSample, long toSample) : base("crop(" + origTrack.getName() + ")")
        {
            this.origTrack = origTrack;
            Debug.Assert(fromSample <= int.MaxValue);
            Debug.Assert(toSample <= int.MaxValue);
            this.fromSample = (int)fromSample;
            this.toSample = (int)toSample;
        }

        static List<TimeToSampleBox.Entry> getDecodingTimeEntries(List<TimeToSampleBox.Entry> origSamples, long fromSample, long toSample)
        {
            if (origSamples != null && origSamples.Count != 0)
            {
                long current = 0;
                List<TimeToSampleBox.Entry>.Enumerator e = origSamples.GetEnumerator();
                e.MoveNext();
                List<TimeToSampleBox.Entry> nuList = new List<TimeToSampleBox.Entry>();

                // Skip while not yet reached:
                TimeToSampleBox.Entry currentEntry;
                while ((currentEntry = e.Current).getCount() + current <= fromSample)
                {
                    current += currentEntry.getCount();
                    e.MoveNext();
                }
                // Take just a bit from the next
                if (currentEntry.getCount() + current >= toSample)
                {
                    nuList.Add(new TimeToSampleBox.Entry(toSample - fromSample, currentEntry.getDelta()));
                    return nuList; // done in one step
                }
                else
                {
                    nuList.Add(new TimeToSampleBox.Entry(currentEntry.getCount() + current - fromSample, currentEntry.getDelta()));
                }
                current += currentEntry.getCount();

                while (e.MoveNext() && (currentEntry = e.Current).getCount() + current < toSample)
                {
                    nuList.Add(currentEntry);
                    current += currentEntry.getCount();
                }

                nuList.Add(new TimeToSampleBox.Entry(toSample - current, currentEntry.getDelta()));

                return nuList;
            }
            else
            {
                return null;
            }
        }

        static List<CompositionTimeToSample.Entry> getCompositionTimeEntries(List<CompositionTimeToSample.Entry> origSamples, long fromSample, long toSample)
        {
            if (origSamples != null && origSamples.Count != 0)
            {
                long current = 0;
                List<CompositionTimeToSample.Entry>.Enumerator e = origSamples.GetEnumerator();
                e.MoveNext();
                List<CompositionTimeToSample.Entry> nuList = new List<CompositionTimeToSample.Entry>();

                // Skip while not yet reached:
                CompositionTimeToSample.Entry currentEntry;
                while ((currentEntry = e.Current).getCount() + current <= fromSample)
                {
                    current += currentEntry.getCount();
                    e.MoveNext();
                }
                // Take just a bit from the next
                if (currentEntry.getCount() + current >= toSample)
                {
                    nuList.Add(new CompositionTimeToSample.Entry((int)(toSample - fromSample), currentEntry.getOffset()));
                    return nuList; // done in one step
                }
                else
                {
                    nuList.Add(new CompositionTimeToSample.Entry((int)(currentEntry.getCount() + current - fromSample), currentEntry.getOffset()));
                }
                current += currentEntry.getCount();

                while (e.MoveNext() && (currentEntry = e.Current).getCount() + current < toSample)
                {
                    nuList.Add(currentEntry);
                    current += currentEntry.getCount();
                }

                nuList.Add(new CompositionTimeToSample.Entry((int)(toSample - current), currentEntry.getOffset()));

                return nuList;
            }
            else
            {
                return null;
            }
        }

        public override void close()
        {
            origTrack.close();
        }

        public override IList<Sample> getSamples()
        {
            return origTrack.getSamples().ToList().GetRange(fromSample, toSample);
        }

        public override List<SampleEntry> getSampleEntries()
        {
            return origTrack.getSampleEntries();
        }

        private readonly object _syncRoot = new object();

        public override long[] getSampleDurations()
        {
            lock (_syncRoot)
            {
                long[] decodingTimes = new long[toSample - fromSample];
                System.Array.Copy(origTrack.getSampleDurations(), fromSample, decodingTimes, 0, decodingTimes.Length);
                return decodingTimes;
            }
        }

        public override List<CompositionTimeToSample.Entry> getCompositionTimeEntries()
        {
            return getCompositionTimeEntries(origTrack.getCompositionTimeEntries(), fromSample, toSample);
        }

        public override long[] getSyncSamples()
        {
            lock (_syncRoot)
            {
                if (origTrack.getSyncSamples() != null)
                {
                    long[] origSyncSamples = origTrack.getSyncSamples();
                    int i = 0, j = origSyncSamples.Length;
                    while (i < origSyncSamples.Length && origSyncSamples[i] < fromSample)
                    {
                        i++;
                    }
                    while (j > 0 && toSample < origSyncSamples[j - 1])
                    {
                        j--;
                    }
                    long[] syncSampleArray = new long[j - i];
                    System.Array.Copy(origTrack.getSyncSamples(), i, syncSampleArray, 0, j - i);
                    for (int k = 0; k < syncSampleArray.Length; k++)
                    {
                        syncSampleArray[k] -= fromSample;
                    }
                    return syncSampleArray;
                }
                return null;
            }
        }

        public override List<SampleDependencyTypeBox.Entry> getSampleDependencies()
        {
            if (origTrack.getSampleDependencies() != null && origTrack.getSampleDependencies().Count != 0)
            {
                return origTrack.getSampleDependencies().GetRange(fromSample, toSample);
            }
            else
            {
                return null;
            }
        }

        public override TrackMetaData getTrackMetaData()
        {
            return origTrack.getTrackMetaData();
        }

        public override string getHandler()
        {
            return origTrack.getHandler();
        }

        public override SubSampleInformationBox getSubsampleInformationBox()
        {
            return origTrack.getSubsampleInformationBox();
        }
    }
}
