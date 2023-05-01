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

namespace SharpMp4Parser.Muxer.Tracks.H264.Parsing
{
    public static class DebugHelper
    {
        public static bool debug = false;

        public static void print8x8(int[] output)
        {
            int i = 0;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    System.Diagnostics.Debug.Write(string.Format("{0}, ", output[i]));
                    i++;
                }
                System.Diagnostics.Debug.WriteLine("");
            }
        }

        public static void print8x8(short[] output)
        {
            int i = 0;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    System.Diagnostics.Debug.Write(string.Format("{0}, ", output[i]));
                    i++;
                }
                System.Diagnostics.Debug.WriteLine("");
            }
        }

        public static void print8x8(ShortBuffer output)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    System.Diagnostics.Debug.Write(string.Format("{0}, ", output.get()));
                }
                System.Diagnostics.Debug.WriteLine("");
            }
        }

        public static void print(short[] table)
        {
            int i = 0;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    System.Diagnostics.Debug.Write(string.Format("{0}, ", table[i]));
                    i++;
                }
                System.Diagnostics.Debug.WriteLine("");
            }
        }

        public static void trace(string format, params object[] args)
        {
            // System.out.printf("> " + format + "\n", args);
        }

        public static void print(int i)
        {
            if (debug)
                System.Diagnostics.Debug.WriteLine(i);
        }

        public static void print(string str)
        {
            if (debug)
                System.Diagnostics.Debug.Write(str);
        }

        public static void println(string str)
        {
            if (debug)
                System.Diagnostics.Debug.WriteLine(str);
        }
    }
}
