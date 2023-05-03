using SharpMp4Parser.Java;
using System;

namespace SharpMp4Parser.Streaming.Input.H264.SpsPps
{
    public class ByteBufferBitreader
    {
        ByteBuffer buffer;

        int nBit;

        private int currentByte;
        private int nextByte;


        public ByteBufferBitreader(ByteBuffer buffer)
        {
            this.buffer = buffer;
            currentByte = get();
            nextByte = get();
        }

        public int get()
        {
            try
            {
                int i = buffer.get();
                i = i < 0 ? i + 256 : i;
                return i;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public int read1Bit()
        {
            if (nBit == 8)
            {
                advance();
                if (currentByte == -1)
                {
                    return -1;
                }
            }
            int res = (currentByte >> (7 - nBit)) & 1;
            nBit++;
            return res;
        }

        private void advance()
        {
            currentByte = nextByte;
            nextByte = get();
            nBit = 0;
        }

        public int readUE()
        {
            int cnt = 0;
            while (read1Bit() == 0)
            {
                cnt++;
            }

            int res = 0;
            if (cnt > 0)
            {
                res = (int)((1 << cnt) - 1 + readNBit(cnt));
            }

            return res;
        }

        public long readNBit(int n)
        {
            if (n > 64)
                throw new ArgumentOutOfRangeException("Can not readByte more then 64 bit");

            long val = 0;

            for (int i = 0; i < n; i++)
            {
                val <<= 1;
                val |= (long)read1Bit();
            }

            return val;
        }

        public bool readBool()
        {
            return read1Bit() != 0;
        }

        public int readSE()
        {
            int val = readUE();
            int sign = ((val & 0x1) << 1) - 1;
            val = ((val >> 1) + (val & 0x1)) * sign;
            return val;
        }

        public bool moreRBSPData()
        {
            if (nBit == 8)
            {
                advance();
            }
            int tail = 1 << (8 - nBit - 1);
            int mask = ((tail << 1) - 1);
            bool hasTail = (currentByte & mask) == tail;

            return !(currentByte == -1 || (nextByte == -1 && hasTail));
        }
    }
}
