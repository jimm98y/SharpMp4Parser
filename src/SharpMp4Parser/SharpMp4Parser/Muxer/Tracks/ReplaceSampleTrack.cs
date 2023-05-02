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
using SharpMp4Parser.Java;
using System.Collections.Generic;
using SharpMp4Parser.IsoParser.Tools;

namespace SharpMp4Parser.Muxer.Tracks
{
    /**
 * Generates a Track where a single sample has been replaced by a given <code>ByteBuffer</code>.
 */

    public class ReplaceSampleTrack : AbstractTrack
    {
        Track origTrack;
        private long sampleNumber;
        private Sample sampleContent;
        private List<Sample> samples;

        public ReplaceSampleTrack(Track origTrack, long sampleNumber, ByteBuffer content) : base("replace(" + origTrack.getName() + ")")
        {
            this.origTrack = origTrack;
            this.sampleNumber = sampleNumber;
            this.sampleContent = new SampleImpl(content, this.origTrack.getSamples()[CastUtils.l2i(sampleNumber)].getSampleEntry());
            this.samples = new ReplaceASingleEntryList(sampleNumber, sampleContent, origTrack);
        }

        public override void close()
        {
            origTrack.close();
        }

        public override IList<Sample> getSamples()
        {
            return samples;
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
                return origTrack.getSampleDurations();
            }
        }

        public override List<CompositionTimeToSample.Entry> getCompositionTimeEntries()
        {
            return origTrack.getCompositionTimeEntries();

        }

        public override long[] getSyncSamples()
        {
            lock (_syncRoot)
            {
                return origTrack.getSyncSamples();
            }
        }

        public override List<SampleDependencyTypeBox.Entry> getSampleDependencies()
        {
            return origTrack.getSampleDependencies();
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

        private class ReplaceASingleEntryList : AbstractList<Sample>
        {
            private long sampleNumber;
            private Sample sampleContent;
            private Track origTrack;

            public ReplaceASingleEntryList(long sampleNumber, Sample sampleContent, Track origTrack)
            {
                this.sampleNumber = sampleNumber;
                this.sampleContent = sampleContent;
                this.origTrack = origTrack;
            }

            public override Sample get(int index)
            {
                if (sampleNumber == index)
                {
                    return sampleContent;
                }
                else
                {
                    return origTrack.getSamples()[index];
                }
            }

            public override int size()
            {
                return origTrack.getSamples().Count;
            }
        }
    }
}
