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
using System.Text;

namespace SharpMp4Parser.IsoParser.Boxes.SampleEntry
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * AMR audio format specific subbox of an audio sample entry.
     *
     * @see AudioSampleEntry
     */
    public class AmrSpecificBox : AbstractBox
    {
        public const string TYPE = "damr";

        private string vendor;
        private int decoderVersion;
        private int modeSet;
        private int modeChangePeriod;
        private int framesPerSample;

        public AmrSpecificBox() : base(TYPE)
        { }

        public string getVendor()
        {
            return vendor;
        }

        public int getDecoderVersion()
        {
            return decoderVersion;
        }

        public int getModeSet()
        {
            return modeSet;
        }

        public int getModeChangePeriod()
        {
            return modeChangePeriod;
        }

        public int getFramesPerSample()
        {
            return framesPerSample;
        }

        protected override long getContentSize()
        {
            return 9;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            byte[] v = new byte[4];
            content.get(v);
            vendor = IsoFile.bytesToFourCC(v);

            decoderVersion = IsoTypeReader.readUInt8(content);
            modeSet = IsoTypeReader.readUInt16(content);
            modeChangePeriod = IsoTypeReader.readUInt8(content);
            framesPerSample = IsoTypeReader.readUInt8(content);

        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            byteBuffer.put(IsoFile.fourCCtoBytes(vendor));
            IsoTypeWriter.writeUInt8(byteBuffer, decoderVersion);
            IsoTypeWriter.writeUInt16(byteBuffer, modeSet);
            IsoTypeWriter.writeUInt8(byteBuffer, modeChangePeriod);
            IsoTypeWriter.writeUInt8(byteBuffer, framesPerSample);
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("AmrSpecificBox[vendor=").Append(getVendor());
            buffer.Append(";decoderVersion=").Append(getDecoderVersion());
            buffer.Append(";modeSet=").Append(getModeSet());
            buffer.Append(";modeChangePeriod=").Append(getModeChangePeriod());
            buffer.Append(";framesPerSample=").Append(getFramesPerSample());
            buffer.Append("]");
            return buffer.ToString();
        }
    }
}
