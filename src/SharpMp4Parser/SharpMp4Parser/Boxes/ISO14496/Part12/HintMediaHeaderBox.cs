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

using SharpMp4Parser.Java;
using SharpMp4Parser.Tools;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * The hint media header contains general information, independent of the protocaol, for hint tracks. Resides
     * in Media Information Box.
     *
     * @see MediaInformationBox
     */
    public class HintMediaHeaderBox : AbstractMediaHeaderBox
    {
        public const string TYPE = "hmhd";
        private int maxPduSize;
        private int avgPduSize;
        private long maxBitrate;
        private long avgBitrate;

        public HintMediaHeaderBox() : base(TYPE)
        { }

        public int getMaxPduSize()
        {
            return maxPduSize;
        }

        public int getAvgPduSize()
        {
            return avgPduSize;
        }

        public long getMaxBitrate()
        {
            return maxBitrate;
        }

        public long getAvgBitrate()
        {
            return avgBitrate;
        }

        protected override long getContentSize()
        {
            return 20;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            maxPduSize = IsoTypeReader.readUInt16(content);
            avgPduSize = IsoTypeReader.readUInt16(content);
            maxBitrate = IsoTypeReader.readUInt32(content);
            avgBitrate = IsoTypeReader.readUInt32(content);
            IsoTypeReader.readUInt32(content);    // reserved!
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt16(byteBuffer, maxPduSize);
            IsoTypeWriter.writeUInt16(byteBuffer, avgPduSize);
            IsoTypeWriter.writeUInt32(byteBuffer, maxBitrate);
            IsoTypeWriter.writeUInt32(byteBuffer, avgBitrate);
            IsoTypeWriter.writeUInt32(byteBuffer, 0);
        }

        public override string ToString()
        {
            return "HintMediaHeaderBox{" +
                    "maxPduSize=" + maxPduSize +
                    ", avgPduSize=" + avgPduSize +
                    ", maxBitrate=" + maxBitrate +
                    ", avgBitrate=" + avgBitrate +
                    '}';
        }
    }
}