using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SharpMp4Parser.Streaming.Input.MP4
{
    public class DiscardingByteArrayOutputStream : ByteStream
    {
        /**
         * The buffer where data is stored.
         */
        protected byte[] buf;

        /**
         * The number of valid bytes in the buffer.
         */
        protected int count;

        protected long startOffset = 0;

        /**
         * Creates a new byte array output stream. The buffer capacity is
         * initially 32 bytes, though its size increases if necessary.
         */
        public DiscardingByteArrayOutputStream() : this(32)
        {

        }

        /**
         * Creates a new byte array output stream, with a buffer capacity of
         * the specified size, in bytes.
         *
         * @param size the initial size.
         * @throws IllegalArgumentException if size is negative.
         */
        public DiscardingByteArrayOutputStream(int size) : base(new ByteStream())
        {
            if (size < 0)
            {
                throw new ArgumentException("Negative initial size: "
                        + size);
            }
            buf = new byte[size];
        }

        public byte[] get(long start, int count)
        {
            byte[] result = new byte[count];
            try
            {
                System.Array.Copy(buf, CastUtils.l2i(start - startOffset), result, 0, count);
            }
            catch (Exception)
            {
                Java.LOG.debug("start: " + start + " count: " + count + " startOffset:" + startOffset + " count:" + count + " len(buf):" + buf.Length + " (start - startOffset):" + (start - startOffset));
                throw;
            }
            return result;
        }

        /**
         * Increases the capacity if necessary to ensure that it can hold
         * at least the number of elements specified by the minimum
         * capacity argument.
         *
         * @param minCapacity the desired minimum capacity
         * @throws OutOfMemoryError if {@code minCapacity < 0}.  This is
         *                          interpreted as a request for the unsatisfiably large capacity
         *                          {@code (long) Integer.MAX_VALUE + (minCapacity - Integer.MAX_VALUE)}.
         */
        private void ensureCapacity(int minCapacity)
        {
            // overflow-conscious code
            if (minCapacity - buf.Length > 0)
                grow(minCapacity);
        }

        /**
         * Increases the capacity to ensure that it can hold at least the
         * number of elements specified by the minimum capacity argument.
         *
         * @param minCapacity the desired minimum capacity
         */
        private void grow(int minCapacity)
        {
            // overflow-conscious code
            int oldCapacity = buf.Length;
            int newCapacity = oldCapacity << 1;
            if (newCapacity - minCapacity < 0)
                newCapacity = minCapacity;
            if (newCapacity < 0)
            {
                if (minCapacity < 0) // overflow
                    throw new OutOfMemoryException();
                newCapacity = int.MaxValue;
            }
            byte[] b = new byte[newCapacity];
            System.Array.Copy(buf, b, buf.Length);
            buf = b;
        }

        /**
         * Writes the specified byte to this byte array output stream.
         *
         * @param b the byte to be written.
         */
        public override void write(int b)
        {
            ensureCapacity(count + 1);
            buf[count] = (byte)b;
            count += 1;
        }

        /**
         * Writes <code>len</code> bytes from the specified byte array
         * starting at offset <code>off</code> to this byte array output stream.
         *
         * @param b   the data.
         * @param off the start offset in the data.
         * @param len the number of bytes to write.
         */
        public override void write(byte[] b, int off, int len)
        {
            if ((off < 0) || (off > b.Length) || (len < 0) ||
                    ((off + len) - b.Length > 0))
            {
                throw new IndexOutOfRangeException();
            }
            ensureCapacity(count + len);
            System.Array.Copy(b, off, buf, count, len);
            count += len;
        }

        /**
         * Resets the <code>count</code> field of this byte array output
         * stream to zero, so that all currently accumulated output in the
         * output stream is discarded. The output stream can be used again,
         * reusing the already allocated buffer space.
         *
         * @see java.io.ByteArrayInputStream#count
         */
        public new void reset()
        {
            count = 0;
        }

        /**
         * Creates a newly allocated byte array. Its size is the current
         * size of this output stream and the valid contents of the buffer
         * have been copied into it.
         *
         * @return the current contents of this output stream, as a byte array.
         * @see java.io.ByteArrayOutputStream#size()
         */
        public override byte[] toByteArray()
        {
            return buf.ToArray();
        }

        /**
         * Returns the current size of the buffer.
         *
         * @return the value of the <code>count</code> field, which is the number
         * of valid bytes in this output stream.
         * @see java.io.ByteArrayOutputStream#count
         */
        public int size()
        {
            return count;
        }

        /**
         * Converts the buffer's contents into a string decoding bytes using the
         * platform's default character set. The length of the new String
         * is a function of the character set, and hence may not be equal to the
         * size of the buffer.
         * This method always replaces malformed-input and unmappable-character
         * sequences with the default replacement string for the platform's
         * default character set. The {@linkplain java.nio.charset.CharsetDecoder}
         * class should be used when more control over the decoding process is
         * required.
         *
         * @return String decoded from the buffer's contents.
         * @since JDK1.1
         */
        public override string ToString()
        {
            return Encoding.UTF8.GetString(buf, 0, count);
        }


        /**
         * Closing a ByteArrayOutputStream has no effect. The methods in
         * this class can be called after the stream has been closed without
         * generating an IOException.
         */
        public override void close()
        {
        }

        /**
         * Returns the last index that is available.
         *
         * @return the overall size (not taking discarded bytes into account)
         */
        public override long available()
        {
            return startOffset + count;
        }

        public void discardTo(long n)
        {
            //System.err.println("discard up to pos " + n);
            System.Array.Copy(buf, CastUtils.l2i(n - startOffset), buf, 0, CastUtils.l2i(buf.Length - (n - startOffset)));
            count -= (int)(n - startOffset);
            startOffset = n;
        }

    }
}