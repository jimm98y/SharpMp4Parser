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

using System.Collections.Generic;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{

    /**
     * <h1>4cc = {@value #TYPE}</h1>
     * The metadata for a presentation is stored in the single Movie Box which occurs at the top-level of a file.
     * Normally this box is close to the beginning or end of the file, though this is not required.
     */
    public class MovieBox : AbstractContainerBox
    {
        public const string TYPE = "moov";

        public MovieBox() : base(TYPE)
        { }

        public int getTrackCount()
        {
            return getBoxes(typeof(TrackBox)).size();
        }

        /**
         * Returns the track numbers associated with this <code>MovieBox</code>.
         *
         * @return the tracknumbers (IDs) of the tracks in their order of appearance in the file
         */
        public long[] getTrackNumbers()
        {
            List<TrackBox> trackBoxes = this.getBoxes(typeof(TrackBox));
            long[] trackNumbers = new long[trackBoxes.size()];
            for (int trackCounter = 0; trackCounter < trackBoxes.size(); trackCounter++)
            {
                trackNumbers[trackCounter] = trackBoxes.get(trackCounter).getTrackHeaderBox().getTrackId();
            }
            return trackNumbers;
        }

        public MovieHeaderBox getMovieHeaderBox()
        {
            return Path.getPath(this, "mvhd");
        }
    }
}
