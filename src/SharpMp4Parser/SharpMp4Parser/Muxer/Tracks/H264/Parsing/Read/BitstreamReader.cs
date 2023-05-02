/*
Copyright (c) 2011 Stanislav Vitvitskiy

Permission is hereby granted, free of charge, to any person obtaining a copy of this
software and associated documentation files (the "Software"), to deal in the Software
without restriction, including without limitation the rights to use, copy, modify,
merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE
OR OTHER DEALINGS IN THE SOFTWARE.
*/

using SharpMp4Parser.Java;
using System;

namespace SharpMp4Parser.Muxer.Tracks.H264.Parsing.Read
{
    /**
     * A dummy implementation of H264 RBSP reading
     *
     * @author Stanislav Vitvitskiy
     */
    public class BitstreamReader
    {
        protected static int bitsRead;
        protected CharCache debugBits = new CharCache(50);
        int nBit;
        private ByteArrayInputStream input;
        private int curByte;
        private int nextByte;

        public BitstreamReader(ByteArrayInputStream input)
        {
            this.input = input;
            curByte = input.read();
            nextByte = input.read();
        }

        public bool readBool()
        {
            return read1Bit() == 1;
        }

        /*
          * (non-Javadoc)
          *
          * @see ua.org.jplayer.javcodec.h264.RBSPInputStream#read1Bit()
          */
        public int read1Bit()
        {
            if (nBit == 8)
            {
                advance();
                if (curByte == -1)
                {
                    return -1;
                }
            }
            int res = (curByte >> (7 - nBit)) & 1;
            nBit++;

            debugBits.append(res == 0 ? '0' : '1');
            ++bitsRead;

            return res;
        }

        /*
          * (non-Javadoc)
          *
          * @see ua.org.jplayer.javcodec.h264.RBSPInputStream#readNBit(int)
          */
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

        private void advance()
        {
            curByte = nextByte;
            nextByte = input.read();
            nBit = 0;
        }

        /*
          * (non-Javadoc)
          *
          * @see ua.org.jplayer.javcodec.h264.RBSPInputStream#readByte()
          */
        public int readByte()
        {
            if (nBit > 0)
            {
                advance();
            }

            int res = curByte;

            advance();

            return res;
        }

        /*
          * (non-Javadoc)
          *
          * @see ua.org.jplayer.javcodec.h264.RBSPInputStream#moreRBSPData()
          */
        public bool moreRBSPData()
        {
            if (nBit == 8)
            {
                advance();
            }
            int tail = 1 << (8 - nBit - 1);
            int mask = ((tail << 1) - 1);
            bool hasTail = (curByte & mask) == tail;

            return !(curByte == -1 || (nextByte == -1 && hasTail));
        }

        public long getBitPosition()
        {
            return (bitsRead * 8 + (nBit % 8));
        }

        /*
          * (non-Javadoc)
          *
          * @see ua.org.jplayer.javcodec.h264.RBSPInputStream#readRemainingByte()
          */
        public long readRemainingByte()
        {
            return readNBit(8 - nBit);
        }

        /*
          * (non-Javadoc)
          *
          * @see ua.org.jplayer.javcodec.h264.RBSPInputStream#next_bits(int)
          */
        public int peakNextBits(int n)
        {
            if (n > 8)
                throw new ArgumentOutOfRangeException("N should be less then 8");
            if (nBit == 8)
            {
                advance();
                if (curByte == -1)
                {
                    return -1;
                }
            }
            int[] bits = new int[16 - nBit];

            int cnt = 0;
            for (int i = nBit; i < 8; i++)
            {
                bits[cnt++] = (curByte >> (7 - i)) & 0x1;
            }

            for (int i = 0; i < 8; i++)
            {
                bits[cnt++] = (nextByte >> (7 - i)) & 0x1;
            }

            int result = 0;
            for (int i = 0; i < n; i++)
            {
                result <<= 1;
                result |= bits[i];
            }

            return result;
        }

        /*
          * (non-Javadoc)
          *
          * @see ua.org.jplayer.javcodec.h264.RBSPInputStream#byte_aligned()
          */
        public bool isByteAligned()
        {
            return (nBit % 8) == 0;
        }

        /*
          * (non-Javadoc)
          *
          * @see ua.org.jplayer.javcodec.h264.RBSPInputStream#close()
          */
        public void close()
        {
        }

        public int getCurBit()
        {
            return nBit;
        }
    }
}