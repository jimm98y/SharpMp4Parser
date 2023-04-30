/*  
 * Copyright 2008 CoreMedia AG, Hamburg
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
     * Tracks are used for two purposes: (a) to contain media data (media tracks) and (b) to contain packetization
     * information for streaming protocols (hint tracks).  <br>
     * There shall be at least one media track within an ISO file, and all the media tracks that contributed to the hint
     * tracks shall remain in the file, even if the media data within them is not referenced by the hint tracks; after
     * deleting all hint tracks, the entire un-hinted presentation shall remain.
     */
    public class TrackBox : AbstractContainerBox
    {
        public const string TYPE = "trak";
        private SampleTableBox sampleTableBox;

        public TrackBox() : base(TYPE)
        { }

        public TrackHeaderBox getTrackHeaderBox()
        {
            return Path.getPath<TrackHeaderBox>(this, "tkhd[0]");
        }

        /**
         * Gets the SampleTableBox at mdia/minf/stbl if existing.
         *
         * @return the SampleTableBox or <code>null</code>
         */
        public SampleTableBox getSampleTableBox()
        {
            if (sampleTableBox != null)
            {
                return sampleTableBox;
            }
            MediaBox mdia = getMediaBox();
            if (mdia != null)
            {
                MediaInformationBox minf = mdia.getMediaInformationBox();
                if (minf != null)
                {
                    sampleTableBox = minf.getSampleTableBox();
                    return sampleTableBox;
                }
            }
            return null;
        }


        public MediaBox getMediaBox()
        {
            return Path.getPath<MediaBox>(this, "mdia[0]");
        }

        public override void setBoxes(List<Box> boxes)
        {
            base.setBoxes(boxes);
            sampleTableBox = null;
        }
    }
}
