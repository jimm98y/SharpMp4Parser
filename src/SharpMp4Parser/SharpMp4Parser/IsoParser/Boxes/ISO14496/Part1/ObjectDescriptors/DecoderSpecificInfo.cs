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

using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System.Linq;
using System.Text;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors
{
    /**
     * abstract class DecoderSpecificInfo extends BaseDescriptor : bit(8)
     * tag=DecSpecificInfoTag
     * {
     * // empty. To be filled by classes extending this class.
     * }
     */
    [Descriptor(Tags = new int[] { 0x05 })]
    public class DecoderSpecificInfo : BaseDescriptor
    {
        byte[] bytes;

        public DecoderSpecificInfo()
        {
            tag = 0x5;
        }

        public override void parseDetail(ByteBuffer bb)
        {
            bytes = new byte[bb.remaining()];
            bb.get(bytes);
        }

        public void setData(byte[] bytes)
        {
            this.bytes = bytes;
        }

        public override int getContentSize()
        {
            return bytes.Length;
        }

        public override ByteBuffer serialize()
        {
            ByteBuffer output = ByteBuffer.allocate(getSize());
            IsoTypeWriter.writeUInt8(output, tag);
            writeSize(output, getContentSize());
            output.put(bytes);
            return (ByteBuffer)output.rewind();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("DecoderSpecificInfo");
            sb.Append("{bytes=").Append(bytes == null ? "null" : Hex.encodeHex(bytes));
            sb.Append('}');
            return sb.ToString();
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }

            DecoderSpecificInfo that = (DecoderSpecificInfo)o;

            if (!bytes.SequenceEqual(that.bytes))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return bytes != null ? Arrays.hashCode(bytes) : 0;
        }
    }
}
