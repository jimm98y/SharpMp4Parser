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
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12
{
    /**
      * <h1>4cc = "{@value #TYPE}"</h1>
      * Only used within the DataReferenceBox. Find more information there.
      *
      * @see DataReferenceBox
      */
    public class DataEntryUrnBox : AbstractFullBox
    {
        public const string TYPE = "urn ";
        private string name;
        private string location;

        public DataEntryUrnBox() : base(TYPE)
        { }

        public string getName()
        {
            return name;
        }

        public string getLocation()
        {
            return location;
        }

        protected override long getContentSize()
        {
            return Utf8.utf8StringLengthInBytes(name) + 1 + Utf8.utf8StringLengthInBytes(location) + 1;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            name = IsoTypeReader.readString(content);
            location = IsoTypeReader.readString(content);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            byteBuffer.put(Utf8.convert(name));
            byteBuffer.put(0);
            byteBuffer.put(Utf8.convert(location));
            byteBuffer.put(0);
        }

        public override string ToString()
        {
            return "DataEntryUrlBox[name=" + getName() + ";location=" + getLocation() + "]";
        }
    }
}