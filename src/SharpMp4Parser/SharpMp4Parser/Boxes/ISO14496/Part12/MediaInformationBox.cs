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

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * This box contains all the objects that declare characteristic information of the media in the track.
     */
    public class MediaInformationBox : AbstractContainerBox
    {
        public const string TYPE = "minf";

        public MediaInformationBox() : base(TYPE)
        { }

        public SampleTableBox getSampleTableBox()
        {
            return Path.getPath(this, "stbl[0]");
        }

        public AbstractMediaHeaderBox getMediaHeaderBox()
        {
            foreach (Box box in getBoxes())
            {
                if (box is AbstractMediaHeaderBox)
                {
                    return (AbstractMediaHeaderBox)box;
                }
            }
            return null;
        }
    }
}
