using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpMp4Parser.Java
{
    public class ByteStream : Closeable
    {
        protected MemoryStream _ms = null;

        public ByteStream()
        {
            _ms = new MemoryStream();
        }

        public ByteStream(ByteStream input)
        {
            _ms = new MemoryStream(input._ms.ToArray());
        }

        public ByteStream(byte[] input)
        {
            _ms = new MemoryStream(input);
        }

        

        public int position()
        {
            return (int)_ms.Position;
        }

        public ByteStream position(long position)
        {
            _ms.Position = position;
            return this;
        }

        public ByteStream reset()
        {
            position(0);
            return this;
        }

        public virtual int read(ByteBuffer bb)
        {
            if (bb.remaining() == 0)
                return 0;

            int ret = read(bb.array(), bb.arrayOffset() + bb.position(), bb.remaining());
            if (ret == 0)
            {
                return -1;
            }
            else
            {
                bb.position(bb.position() + ret);
                return ret;
            }
        }

        public virtual int write(ByteBuffer value)
        {
            var data = value.array().Skip(value.arrayOffset() + value.position()).Take(value.limit() - value.position()).ToArray();
            return write(data);
        }

        public virtual int write(byte[] data)
        {
            write(data, 0, data.Length);
            return (int)data.Length;
        }

        public virtual void write(byte[] value, int offset, int length)
        {
            _ms.Write(value, offset, length);
        }

        public virtual int read(byte[] bb)
        {
            return read(bb, 0, bb.Length);
        }

        public virtual int read(byte[] bytes, int offset, int length)
        {
            return _ms.Read(bytes, offset, length);
        }





        public virtual int read()
        {
            return _ms.ReadByte();
        }

        public virtual void write(int value)
        {
            _ms.WriteByte((byte)value);
        }

        public void writeShort(short value)
        {
            using (BinaryWriter br = new BinaryWriter(_ms, Encoding.UTF8, true))
            {
                br.Write(value);
            }
        }

        public string readLine(string encoding)
        {
            if ("UTF-8".CompareTo(encoding) == 0)
            {
                // utf8
                using (var sr = new StreamReader(_ms, Encoding.UTF8, false, 1, true))
                {
                    return sr.ReadLine();
                }
            }
            else
            {
                throw new NotSupportedException(encoding);
            }
        }

        public virtual bool isOpen()
        {
            return _ms.CanRead || _ms.CanWrite;
        }

        public virtual byte[] toByteArray()
        {
            return _ms.ToArray().Take(position()).ToArray();
        }

        public virtual long available()
        {
            // https://docs.oracle.com/javase/8/docs/api/java/io/ByteStreamBase.html
            // Returns an estimate of the number of bytes that can be read (or skipped over) from this input stream without blocking by the next invocation of a method for this input stream.
            return (int)(_ms.Capacity - _ms.Position);
        }

        public virtual void close()
        {
            _ms.Close();
        }
    }

    public class BufferedReader
    {
        private ByteStreamBaseReader ByteStreamBaseReader;

        public BufferedReader(ByteStreamBaseReader ByteStreamBaseReader)
        {
            this.ByteStreamBaseReader = ByteStreamBaseReader;
        }

        internal string readLine()
        {
            return ByteStreamBaseReader.readLine();
        }
    }

    public class ByteStreamBaseReader
    {
        private ByteStream input;
        private string encoding;

        public ByteStreamBaseReader(ByteStream input, string encoding)
        {
            this.input = input;
            this.encoding = encoding;
        }

        internal string readLine()
        {
            return input.readLine(encoding);
        }
    }

    public static class Channels
    {
        public static ByteStream newChannel(ByteStream ByteStreamBase)
        {
            return ByteStreamBase;
        }

        public static ByteStream newInputStream(ByteStream dataSource)
        {
            return new ByteStream(dataSource);
        }
    }
}
