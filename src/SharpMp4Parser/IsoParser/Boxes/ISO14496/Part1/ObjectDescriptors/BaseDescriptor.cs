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
using System.Diagnostics;
using System.Text;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors
{
    /*
     abstract aligned(8) expandable(228-1) class BaseDescriptor : bit(8) tag=0 {
     // empty. To be filled by classes extending this class.
     }

     int sizeOfInstance = 0;
     bit(1) nextByte;
     bit(7) sizeOfInstance;
     while(nextByte) {
     bit(1) nextByte;
     bit(7) sizeByte;
     sizeOfInstance = sizeOfInstance<<7 | sizeByte;
     }
      */
    [Descriptor(Tags = new int[] { 0x00 })]
    public abstract class BaseDescriptor
    {
        protected int tag;
        protected int sizeOfInstance;
        protected int sizeBytes;

        public BaseDescriptor()
        {
        }

        public int getTag()
        {
            return tag;
        }

        public void writeSize(ByteBuffer bb, int size)
        {
            int pos = bb.position();

            int i = 0;
            while (size > 0 || i < sizeBytes)
            {
                i++;
                if (size > 0)
                {
                    bb.put(pos + getSizeSize() - i, (byte)(size & 0x7f));
                }
                else
                {
                    bb.put(pos + getSizeSize() - i, 0x80);
                }
                size = (int)((uint)size >> 7);

            }

            ((Buffer)bb).position(pos + getSizeSize());

        }

        public int getSizeSize()
        {
            int size = getContentSize();
            int i = 0;
            while (size > 0 || i < sizeBytes)
            {
                size = (int)((uint)size >> 7);
                i++;
            }
            return i;
        }


        public int getSize()
        {
            return getContentSize() + getSizeSize() + 1;
        }

        public void parse(int tag, ByteBuffer bb)
        {
            this.tag = tag;

            int i = 0;
            int tmp = IsoTypeReader.readUInt8(bb);
            i++;
            sizeOfInstance = tmp & 0x7f;
            while ((int)((uint)tmp >> 7) == 1)
            {
                //nextbyte indicator bit
                tmp = IsoTypeReader.readUInt8(bb);
                i++;
                //sizeOfInstance = sizeOfInstance<<7 | sizeByte;
                sizeOfInstance = sizeOfInstance << 7 | tmp & 0x7f;
            }
            sizeBytes = i;
            ByteBuffer detailSource = bb.slice();
            detailSource.limit(sizeOfInstance);
            parseDetail(detailSource);
            Debug.Assert(detailSource.remaining() == 0, GetType().Name + " has not been fully parsed");
            ((Buffer)bb).position(bb.position() + sizeOfInstance);
        }

        public abstract void parseDetail(ByteBuffer bb);

        public abstract ByteBuffer serialize();

        public abstract int getContentSize();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("BaseDescriptor");
            sb.Append("{tag=").Append(tag);
            sb.Append(", sizeOfInstance=").Append(sizeOfInstance);
            sb.Append('}');
            return sb.ToString();
        }
    }
}
