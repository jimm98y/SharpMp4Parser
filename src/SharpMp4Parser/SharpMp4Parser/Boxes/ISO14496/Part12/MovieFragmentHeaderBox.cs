/*
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

using SharpMp4Parser.Java;
using SharpMp4Parser.Support;
using SharpMp4Parser.Tools;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * aligned(8) class MovieFragmentHeaderBox
     * extends FullBox('mfhd', 0, 0){
     * unsigned int(32) sequence_number;
     * }
     */
    public class MovieFragmentHeaderBox : AbstractFullBox
    {
        public const string TYPE = "mfhd";
        private long sequenceNumber;

        public MovieFragmentHeaderBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return 8;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt32(byteBuffer, sequenceNumber);
        }


        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            sequenceNumber = IsoTypeReader.readUInt32(content);
        }

        public long getSequenceNumber()
        {
            return sequenceNumber;
        }

        public void setSequenceNumber(long sequenceNumber)
        {
            this.sequenceNumber = sequenceNumber;
        }

        public override string ToString()
        {
            return "MovieFragmentHeaderBox{" +
                    "sequenceNumber=" + sequenceNumber +
                    '}';
        }
    }
}
