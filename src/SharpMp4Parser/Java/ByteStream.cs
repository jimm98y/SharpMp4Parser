﻿using SharpMp4Parser.IsoParser.Tools;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpMp4Parser.Java
{
    public class ByteStream : Closeable, IDisposable
    {
        internal Stream _ms = null;
        private bool disposedValue;

        public void CopyTo(Stream stream)
        {
            _ms.CopyTo(stream);
        }

        public ByteStream()
        {
            _ms = new MemoryStream();
        }

        public ByteStream(Stream ms)
        {
            _ms = ms;
        }

        public ByteStream(ByteStream input)
        {
            _ms = input._ms;
        }

        public ByteStream(byte[] input)
        {
            _ms = new MemoryStream(input);
        }

        public ByteBuffer get(long offset, long size)
        {
            // random access
            var ret = ByteBuffer.allocate((int)size);
            long oldPos = _ms.Position;
            _ms.Position = CastUtils.l2i(offset);
            _ms.Read(ret.array(), 0, CastUtils.l2i(size));
            _ms.Position = oldPos;
            return ret;
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

        public short readShort()
        {
            using (BinaryReader br = new BinaryReader(_ms, Encoding.UTF8, true))
            {
                return br.ReadInt16();
            }
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

        public virtual bool isOpen()
        {
            return _ms.CanRead || _ms.CanWrite;
        }

        public virtual byte[] toByteArray()
        {
            if(_ms is MemoryStream)
            {
                return ((MemoryStream)_ms).ToArray().Take(position()).ToArray();
            }

            if(_ms is FileStream)
            {
                using(MemoryStream ms = new MemoryStream())
                {
                    ((FileStream)_ms).CopyTo(ms);
                    return ms.ToArray().Take(position()).ToArray();
                }
            }

            throw new NotSupportedException();
        }

        public virtual long available()
        {
            if (_ms is MemoryStream)
            {
                var ms = (MemoryStream)_ms;
                // https://docs.oracle.com/javase/8/docs/api/java/io/ByteStreamBase.html
                // Returns an estimate of the number of bytes that can be read (or skipped over) from this input stream without blocking by the next invocation of a method for this input stream.
                return (int)(ms.Capacity - ms.Position);
            }

            throw new NotSupportedException();
        }

        public virtual void close()
        {
            _ms.Close();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _ms.Close();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class BufferedReader
    {
        private ByteStreamBaseReader reader;

        public BufferedReader(ByteStreamBaseReader reader)
        {
            this.reader = reader;
        }

        internal string readLine()
        {
            return reader.readLine();
        }
    }

#warning TODO dispose
    public class ByteStreamBaseReader : IDisposable
    {
        private readonly StreamReader sr;

        public ByteStreamBaseReader(ByteStream input, string encoding)
        {
            if ("UTF-8".CompareTo(encoding) == 0)
            {
                this.sr = new StreamReader(input._ms, Encoding.UTF8, true, 1, true);
            }
            else
            {
                throw new NotSupportedException(encoding);
            }
        }

        public string readLine()
        {
            return sr.ReadLine();
        }

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    sr.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public static class Channels
    {
        public static ByteStream newChannel(ByteStream dataSource)
        {
            return dataSource;
        }

        public static ByteStream newInputStream(ByteStream dataSource)
        {
            return new ByteStream(dataSource);
        }
    }
}
