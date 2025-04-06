﻿/*
 * Copyright 2009 castLabs GmbH, Berlin
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

using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using System.Collections.Generic;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * aligned(8) class MovieFragmentBox extends Box(moof){
     * }
     */

    public class MovieFragmentBox : AbstractContainerBox
    {
        public const string TYPE = "moof";

        public MovieFragmentBox() : base(TYPE)
        { }

        public List<long> getSyncSamples(SampleDependencyTypeBox sdtp)
        {
            List<long> result = new List<long>();

            List<SampleDependencyTypeBox.Entry> sampleEntries = sdtp.getEntries();
            long i = 1;
            foreach (SampleDependencyTypeBox.Entry sampleEntry in sampleEntries)
            {
                if (sampleEntry.getSampleDependsOn() == 2)
                {
                    result.Add(i);
                }
                i++;
            }

            return result;
        }


        public int getTrackCount()
        {
            return getBoxes<TrackFragmentBox>(typeof(TrackFragmentBox), false).Count;
        }

        /**
         * Returns the track numbers associated with this <code>MovieBox</code>.
         *
         * @return the tracknumbers (IDs) of the tracks in their order of appearance in the file
         */

        public long[] getTrackNumbers()
        {
            List<TrackFragmentBox> trackBoxes = getBoxes<TrackFragmentBox>(typeof(TrackFragmentBox), false);
            long[] trackNumbers = new long[trackBoxes.Count];
            for (int trackCounter = 0; trackCounter < trackBoxes.Count; trackCounter++)
            {
                TrackFragmentBox trackBoxe = trackBoxes[trackCounter];
                trackNumbers[trackCounter] = trackBoxe.getTrackFragmentHeaderBox().getTrackId();
            }
            return trackNumbers;
        }

        public List<TrackFragmentHeaderBox> getTrackFragmentHeaderBoxes()
        {
            return Path.getPaths<TrackFragmentHeaderBox>((Container)this, "tfhd");
        }

        public List<TrackRunBox> getTrackRunBoxes()
        {
            return getBoxes<TrackRunBox>(typeof(TrackRunBox), true);
        }
    }
}
