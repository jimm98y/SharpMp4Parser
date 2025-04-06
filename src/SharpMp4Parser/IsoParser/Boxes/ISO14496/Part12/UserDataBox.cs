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
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12
{
    /**
      * <h1>4cc = "{@value #TYPE}"</h1>
      * This box contains objects that declare user information about the containing box and its data (presentation or
      * track).<br>
      * The User Data Box is a container box for informative user-data. This user data is formatted as a set of boxes
      * with more specific box types, which declare more precisely their content
      */
    public class UserDataBox : AbstractContainerBox
    {
        public const string TYPE = "udta";

        public UserDataBox() : base(TYPE)
        { }

        public override void parse(ByteStream dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            base.parse(dataSource, header, contentSize, boxParser);
        }

        public override void getBox(ByteStream writableByteChannel)
        {
            base.getBox(writableByteChannel);
        }
    }
}
