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
using System.Diagnostics;

namespace SharpMp4Parser.Muxer.Tracks.H264.Parsing.Write
{
    /**
     * A class responsible for outputting exp-Golumb values into binary stream
     *
     * @author Stanislav Vitvitskiy
     */
    public class CAVLCWriter : BitstreamWriter
    {

        public CAVLCWriter(ByteStream output) : base(output)
        { }

        public void writeU(int value, int n, string str)
        {
            Debug.WriteLine(str + "\t");
            writeNBit(value, n);
            Debug.WriteLine("\t" + value);
        }

        public void writeUE(int value)
        {
            int bits = 0;
            int cumul = 0;
            for (int i = 0; i < 15; i++)
            {
                if (value < cumul + (1 << i))
                {
                    bits = i;
                    break;
                }
                cumul += (1 << i);
            }
            writeNBit(0, bits);
            write1Bit(1);
            writeNBit(value - cumul, bits);
        }

        public void writeUE(int value, string str)
        {
            Debug.Write(str + "\t");
            writeUE(value);
            Debug.WriteLine("\t" + value);
        }

        public void writeSE(int value, string str)
        {
            Debug.Write(str + "\t");
            writeUE((value << 1) * (value < 0 ? -1 : 1) + (value > 0 ? 1 : 0));
            Debug.WriteLine("\t" + value);
        }

        public void writeBool(bool value, string str)
        {
            Debug.Write(str + "\t");
            write1Bit(value ? 1 : 0);
            Debug.WriteLine("\t" + value);
        }

        public void writeU(int i, int n)
        {
            writeNBit(i, n);
        }

        public void writeNBit(long value, int n, string str)
        {
            Debug.Write(str + "\t");
            for (int i = 0; i < n; i++)
            {
                write1Bit((int)(value >> (n - i - 1)) & 0x1);
            }
            Debug.WriteLine("\t" + value);
        }

        public void writeTrailingBits()
        {
            write1Bit(1);
            writeRemainingZero();
            flush();
        }

        public void writeSliceTrailingBits()
        {
            throw new NotImplementedException("todo");
        }
    }
}