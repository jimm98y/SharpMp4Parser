/*
 * Copyright 2011 castLabs, Berlin
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

using System.Text;

namespace SharpMp4Parser.Boxes.ISO14496.Part1.ObjectDescriptors
{
    /**
     * class SLConfigDescriptor extends BaseDescriptor : bit(8) tag=SLConfigDescrTag {
     * bit(8) predefined;
     * if (predefined==0) {
     * bit(1) useAccessUnitStartFlag;
     * bit(1) useAccessUnitEndFlag;
     * bit(1) useRandomAccessPointFlag;
     * bit(1) hasRandomAccessUnitsOnlyFlag;
     * bit(1) usePaddingFlag;
     * bit(1) useTimeStampsFlag;
     * bit(1) useIdleFlag;
     * bit(1) durationFlag;
     * bit(32) timeStampResolution;
     * bit(32) OCRResolution;
     * bit(8) timeStampLength; // must be ≤ 64
     * bit(8) OCRLength; // must be ≤ 64
     * bit(8) AU_Length; // must be ≤ 32
     * bit(8) instantBitrateLength;
     * bit(4) degradationPriorityLength;
     * bit(5) AU_seqNumLength; // must be ≤ 16
     * bit(5) packetSeqNumLength; // must be ≤ 16
     * bit(2) reserved=0b11;
     * }
     * if (durationFlag) {
     * bit(32) timeScale;
     * bit(16) accessUnitDuration;
     * bit(16) compositionUnitDuration;
     * }
     * if (!useTimeStampsFlag) {
     * bit(timeStampLength) startDecodingTimeStamp;
     * bit(timeStampLength) startCompositionTimeStamp;
     * }
     * }
     */
    [Descriptor(Tags = new int[] { 0x06 })]
    public class SLConfigDescriptor : BaseDescriptor
    {
        int predefined;

        public SLConfigDescriptor()
        {
            tag = 0x06;
        }

        public int getPredefined()
        {
            return predefined;
        }

        public void setPredefined(int predefined)
        {
            this.predefined = predefined;
        }

        public override void parseDetail(ByteBuffer bb)
        {
            predefined = IsoTypeReader.readUInt8(bb);
        }

        public int getContentSize()
        {
            return 1;
        }

        public ByteBuffer serialize()
        {
            ByteBuffer output = ByteBuffer.allocate(getSize());
            IsoTypeWriter.writeUInt8(output, 6);
            writeSize(output, getContentSize());
            IsoTypeWriter.writeUInt8(output, predefined);
            return output;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SLConfigDescriptor");
            sb.Append("{predefined=").Append(predefined);
            sb.Append('}');
            return sb.ToString();
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || getClass() != o.getClass())
            {
                return false;
            }

            SLConfigDescriptor that = (SLConfigDescriptor)o;

            if (predefined != that.predefined)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return predefined;
        }
    }
}
