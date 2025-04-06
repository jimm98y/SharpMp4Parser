using SharpMp4Parser.Java;
using System;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors
{
    public class BitReaderBuffer
    {
        int initialPos;
        int position;
        private ByteBuffer buffer;

        public BitReaderBuffer(ByteBuffer buffer)
        {
            this.buffer = buffer;
            initialPos = buffer.position();
        }

        public bool readBool()
        {
            return readBits(1) == 1;
        }

        public int readBits(int i)
        {
            byte b = buffer.get(initialPos + position / 8);
            int v = b < 0 ? b + 256 : b;
            int left = 8 - position % 8;
            int rc;
            if (i <= left)
            {
                rc = (v << position % 8 & 0xFF) >> position % 8 + (left - i);
                position += i;
            }
            else
            {
                int now = left;
                int then = i - left;
                rc = readBits(now);
                rc = rc << then;
                rc += readBits(then);
            }
            ((Java.Buffer)buffer).position(initialPos + (int)Math.Ceiling((double)position / 8));
            return rc;
        }

        public int getPosition()
        {
            return position;
        }

        public int byteSync()
        {
            int left = 8 - position % 8;
            if (left == 8)
            {
                left = 0;
            }
            readBits(left);
            return left;
        }

        public int remainingBits()
        {
            return buffer.limit() * 8 - position;
        }
    }
}
