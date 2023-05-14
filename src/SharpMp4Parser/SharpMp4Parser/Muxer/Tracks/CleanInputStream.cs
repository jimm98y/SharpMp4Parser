using SharpMp4Parser.Java;
using System;

namespace SharpMp4Parser.Muxer.Tracks
{
    /**
     * Removes NAL Unit emulation_prevention_three_byte.
     */
    public class CleanByteStreamBase : ByteStream
    {

        int prevprev = -1;
        int prev = -1;

        public CleanByteStreamBase(ByteStream input) : base(input)
        { }

        public bool markSupported()
        {
            return false;
        }

        public override int read()
        {
            int c = base.read();
            if (c == 3 && prevprev == 0 && prev == 0)
            {
                // discard this character
                prevprev = -1;
                prev = -1;
                c = base.read();
            }
            prevprev = prev;
            prev = c;
            return c;
        }

        /**
         * Copy of InputStream.read(b, off, len)
         *
         * @see java.io.InputStream#read()
         */
        public override int read(byte[] b, int off, int len)
        {
            if (b == null)
            {
                throw new ArgumentNullException();
            }
            else if (off < 0 || len < 0 || len > b.Length - off)
            {
                throw new ArgumentOutOfRangeException();
            }
            else if (len == 0)
            {
                return 0;
            }

            int c = read();
            if (c == -1)
            {
                return -1;
            }
            b[off] = (byte)c;

            int i = 1;
            try
            {
                for (; i < len; i++)
                {
                    c = read();
                    if (c == -1)
                    {
                        break;
                    }
                    b[off + i] = (byte)c;
                }
            }
            catch (Exception)
            {
            }
            return i;
        }
    }
}
