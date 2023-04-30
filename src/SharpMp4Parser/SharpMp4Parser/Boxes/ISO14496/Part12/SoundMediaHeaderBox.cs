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
     */
    public class SoundMediaHeaderBox : AbstractMediaHeaderBox
    {
        public const string TYPE = "smhd";
        private float balance;

        public SoundMediaHeaderBox() : base(TYPE)
        { }

        public float getBalance()
        {
            return balance;
        }

        protected long getContentSize()
        {
            return 8;
        }

        public override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            balance = IsoTypeReader.readFixedPoint88(content);
            IsoTypeReader.readUInt16(content);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeFixedPoint88(byteBuffer, balance);
            IsoTypeWriter.writeUInt16(byteBuffer, 0);
        }

        public override string ToString()
        {
            return "SoundMediaHeaderBox[balance=" + getBalance() + "]";
        }
    }
}
