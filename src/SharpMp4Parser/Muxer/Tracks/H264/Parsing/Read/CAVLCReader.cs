﻿/*
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

using System.Text;
using System;
using System.Diagnostics;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.Muxer.Tracks.H264.Parsing.Read
{
    public class CAVLCReader : BitstreamReader, IByteBufferReader
    {

        public CAVLCReader(ByteStream input) : base(input)
        { }

        public long readNBit(int n, string message)
        {
            long val = readNBit(n);

            trace(message, val.ToString());

            return val;
        }

        /**
         * Read unsigned exp-golomb code
         *
         * @return
         * @throws java.io.IOException
         * @throws java.io.IOException
         */
        public int readUE()
        {
            int cnt = 0;
            while (read1Bit() == 0)
                cnt++;

            int res = 0;
            if (cnt > 0)
            {
                long val = readNBit(cnt);

                res = (int)((1 << cnt) - 1 + val);
            }

            return res;
        }

        /*
          * (non-Javadoc)
          *
          * @see
          * ua.org.jplayer.javcodec.h264.H264BitByteStreamBase#readUE(java.lang.String)
          */
        public int readUE(string message)
        {
            int res = readUE();

            trace(message, res.ToString());

            return res;
        }

        public int readSE()
        {
            return readSE(null);
        }

        public int readSE(string message)
        {
            int val = readUE();

            int sign = ((val & 0x1) << 1) - 1;
            val = ((val >> 1) + (val & 0x1)) * sign;

            trace(message, val.ToString());

            return val;
        }

        public bool readBool(string message)
        {

            bool res = read1Bit() == 0 ? false : true;

            trace(message, res ? "1" : "0");

            return res;
        }

        public int readU(int i, string str)
        {
            return (int)readNBit(i, str);
        }

        public byte[] read(int payloadSize)
        {
            byte[]
            result = new byte[payloadSize];
            for (int i = 0; i < payloadSize; i++)
            {
                result[i] = (byte)readByte();
            }
            return result;
        }

        public bool readAE()
        {
            // TODO: do it!!
            throw new NotSupportedException("Stan");
        }

        public int readTE(int max)
        {
            if (max > 1)
                return readUE();
            return ~read1Bit() & 0x1;
        }

        public int readAEI()
        {
            // TODO: do it!!
            throw new NotSupportedException("Stan");
        }

        public int readME(string str)
        {
            return readUE(str);
        }

        public object readCE(BTree bt, string message)
        {
            while (true)
            {
                int bit = read1Bit();
                bt = bt.down(bit);
                if (bt == null)
                {
                    throw new Exception("Illegal code");
                }
                object i = bt.getValue();
                if (i != null)
                {
                    trace(message, i.ToString());
                    return i;
                }
            }
        }

        public int readZeroBitCount(string message)
        {
            int count = 0;
            while (read1Bit() == 0)
                count++;

            trace(message, count.ToString());

            return count;
        }

        public void readTrailingBits()
        {
            read1Bit();
            readRemainingByte();
        }

        private void trace(string message, string val)
        {
            if (string.IsNullOrEmpty(message))
                return;

            StringBuilder traceBuilder = new StringBuilder();
            int spaces;
            string pos = (bitsRead - debugBits.length()).ToString();
            spaces = 8 - pos.Length;

            traceBuilder.Append("@" + pos);

            for (int i = 0; i < spaces; i++)
                traceBuilder.Append(' ');

            traceBuilder.Append(message);
            spaces = 100 - traceBuilder.Length - debugBits.length();
            for (int i = 0; i < spaces; i++)
                traceBuilder.Append(' ');
            traceBuilder.Append(debugBits);
            traceBuilder.Append(" (" + val + ")");
            debugBits.clear();

            Java.LOG.debug(traceBuilder.ToString());
        }
    }
}
