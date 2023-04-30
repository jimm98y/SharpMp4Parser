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
 * This box provides a compact marking of the random access points withinthe stream. The table is arranged in
 * strictly decreasinf order of sample number. Defined in ISO/IEC 14496-12.
 */
    public class SyncSampleBox : AbstractFullBox
    {
        public const string TYPE = "stss";

        private long[] sampleNumber;

        public SyncSampleBox() : base(TYPE)
        { }

        /**
         * Gives the numbers of the samples that are random access points in the stream.
         *
         * @return random access sample numbers.
         */
        public long[] getSampleNumber()
        {
            return sampleNumber;
        }

        public void setSampleNumber(long[] sampleNumber)
        {
            this.sampleNumber = sampleNumber;
        }

        protected long getContentSize()
        {
            return sampleNumber.Length * 4L + 8;
        }

        public override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            int entryCount = CastUtils.l2i(IsoTypeReader.readUInt32(content));

            sampleNumber = new long[entryCount];
            for (int i = 0; i < entryCount; i++)
            {
                sampleNumber[i] = IsoTypeReader.readUInt32(content);
            }
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt32(byteBuffer, sampleNumber.Length);

            foreach (long aSampleNumber in sampleNumber)
            {
                IsoTypeWriter.writeUInt32(byteBuffer, aSampleNumber);
            }
        }

        public string toString()
        {
            return "SyncSampleBox[entryCount=" + sampleNumber.Length + "]";
        }
    }
}
