﻿/*  
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

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12
{
    /**
      * <h1>4cc = "{@value #TYPE}"</h1>
      * The media declaration container contains all the objects that declare information about the media data within a
      * track.
      */
    public class MediaBox : AbstractContainerBox
    {
        public const string TYPE = "mdia";

        public MediaBox() : base(TYPE)
        { }

        public MediaInformationBox getMediaInformationBox()
        {
            return Path.getPath<MediaInformationBox>(this, "minf[0]");
        }

        public MediaHeaderBox getMediaHeaderBox()
        {
            return Path.getPath<MediaHeaderBox>(this, "mdhd[0]");
        }

        public HandlerBox getHandlerBox()
        {
            return Path.getPath<HandlerBox>(this, "hdlr[0]");
        }
    }
}
