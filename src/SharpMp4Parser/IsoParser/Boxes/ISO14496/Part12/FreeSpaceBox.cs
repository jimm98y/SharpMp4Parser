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
     * The contents of a free-space box are irrelevant and may be ignored, or the object deleted, without affecting the
     * presentation. Care should be excercized when deleting the object, as this may invalidate the offsets used in the
     * sample table.
     */
    public class FreeSpaceBox : AbstractBox
    {
        public const string TYPE = "skip";

        byte[] data;

        public FreeSpaceBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return data.Length;
        }

        public byte[] getData()
        {
            return data;
        }

        public void setData(byte[] data)
        {
            this.data = data;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            data = new byte[content.remaining()];
            content.get(data);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            byteBuffer.put(data);
        }

        public override string ToString()
        {
            return "FreeSpaceBox[size=" + data.Length + ";type=" + getType() + "]";
        }
    }
}
