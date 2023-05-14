using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpMp4Parser.Java
{

    public class StreamBase : Closeable
    {
        protected int _capacity = -1;

        protected MemoryStream _ms = null;

        public StreamBase()
        {
            _ms = new MemoryStream();
            _capacity = _ms.Capacity;
        }

        public StreamBase(int capacity)
        {
            _ms = new MemoryStream(capacity);
            _capacity = _ms.Capacity;
        }

        public StreamBase(StreamBase input)
        {
            _ms = input._ms;
            _capacity = _ms.Capacity;
        }

        public StreamBase(MemoryStream input)
        {
            _ms = input;
            _capacity = _ms.Capacity;
        }

        public StreamBase(byte[] input)
        {
            _ms = new MemoryStream(input);
            _capacity = _ms.Capacity;
        }

        public virtual StreamBase duplicate()
        {
            return new StreamBase(_ms.ToArray());
        }

        public int capacity()
        {
            return _capacity;
        }

        public byte[] array()
        {
            return _ms.ToArray();
        }

        public int remaining()
        {
            return (int)(_ms.Capacity - _ms.Position);
        }

        public int position()
        {
            return (int)_ms.Position;
        }

        public StreamBase position(long position)
        {
            _ms.Position = Math.Min(position, _ms.Capacity);
            return this;
        }

        public void put(byte[] data)
        {
            int oldCapacity = _ms.Capacity;

            _ms.Write(data, 0, data.Length);

            if (_ms.Capacity > oldCapacity)
            {
                if (oldCapacity != 0)
                {
                    limit(oldCapacity);
                }
            }
        }

        public void put(StreamBase buffer)
        {
            byte[] data = new byte[buffer._ms.Capacity - buffer._ms.Position];
            buffer.read(data, 0, data.Length);
            put(data);
        }

        public virtual int write(ByteBuffer value)
        {
            put(value.array());
            return (int)(value.capacity());
        }

        public virtual int write(Buffer value)
        {
            return write((ByteBuffer)value);
        }

        public virtual int write(byte[] value)
        {
            put(value);
            return (int)value.Length;
        }

        public virtual void write(byte[] value, int offset, int length)
        {
            put(value, offset, length);
        }

        public virtual void write(int value)
        {
            putInt(value);
        }

        public virtual void write(byte value)
        {
            put(value);
        }

        public virtual int read()
        {
            return _ms.ReadByte();
        }

        public virtual byte[] toByteArray()
        {
            return _ms.ToArray();
        }

        public virtual long available()
        {
            // https://docs.oracle.com/javase/8/docs/api/java/io/InputStream.html
            // Returns an estimate of the number of bytes that can be read (or skipped over) from this input stream without blocking by the next invocation of a method for this input stream.
            return remaining();
        }
        public virtual void close()
        {
            _ms.Close();
        }

        public virtual bool isOpen()
        {
            return _ms.CanRead || _ms.CanWrite;
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

        public void writeShort(short value)
        {
            putShort(value);
        }

        public void put(byte value)
        {
            _ms.WriteByte(value);
        }

        private byte[] _original = null;

        public StreamBase limit(int limit)
        {
            if (this._original == null && this._ms.Length > 0)
            {
                this._original = this._ms.ToArray();
            }

            var oldpos = this._ms.Position;
            var oldMs = this._ms.ToArray();

            if (limit == this._capacity && this._original != null)
            {
                oldMs = this._original;
                this._original = null;
            }

            this._ms = new MemoryStream(limit);
            this._ms.Write(oldMs, 0, Math.Min(oldMs.Length, limit));

            this._ms.Position = Math.Min(oldpos, limit);
            return this;
        }

        public virtual int read(byte[] bb)
        {
            return get(bb, 0, bb.Length);
        }

        public virtual int read(byte[] bytes, int offset, int length)
        {
            return get(bytes, offset, length);
        }

        internal int get(byte[] bytes, int offset, int length)
        {
            return _ms.Read(bytes, offset, length);
        }

        public void put(byte[] bytes, int offset, int length)
        {
            _ms.Write(bytes, offset, length);
        }

        public void putInt(int value)
        {
            using (BinaryWriter br = new BinaryWriter(_ms, Encoding.UTF8, true))
            {
                br.Write(value);
            }
        }

        public void putShort(short value)
        {
            using (BinaryWriter br = new BinaryWriter(_ms, Encoding.UTF8, true))
            {
                br.Write(value);
            }
        }
    }

    public class ByteStreamBase : StreamBase
    {
        public ByteStreamBase(int capacity) : base(capacity)
        { }

        public ByteStreamBase(Java.StreamBase input) : base(input)
        { }

        public ByteStreamBase(MemoryStream input) : base(input)
        { }

        public ByteStreamBase(byte[] input) : base(input)
        { }

        public override StreamBase duplicate()
        {
            return new ByteStreamBase(new MemoryStream(_ms.ToArray()));
        }

        public static ByteStreamBase wrap(byte[] bytes, int offset, int length)
        {
            return new ByteStreamBase(new MemoryStream(bytes, offset, length));
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

        public ByteStreamBase reset()
        {
            position(0);
            return this;
        }

        public ByteStreamBase position(int nextBufferWritePosition)
        {
            base.position(nextBufferWritePosition);
            return this;
        }
    }

    public class BufferedReader
    {
        private InputStreamReader inputStreamReader;

        public BufferedReader(InputStreamReader inputStreamReader)
        {
            this.inputStreamReader = inputStreamReader;
        }

        internal string readLine()
        {
            return inputStreamReader.readLine();
        }
    }

    public class InputStreamReader
    {
        private ByteArrayInputStream input;
        private string encoding;

        public InputStreamReader(ByteArrayInputStream input, string encoding)
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
        public static WritableByteChannel newChannel(ByteArrayOutputStream outputStream)
        {
            return outputStream;
        }

        public static ReadableByteChannel newChannel(ByteArrayInputStream inputStream)
        {
            return inputStream;
        }

        public static ByteArrayInputStream newInputStream(ReadableByteChannel dataSource)
        {
            return new ByteArrayInputStream(dataSource);
        }
    }

    public class InputStream : ByteStreamBase
    {
        public InputStream(byte[] input) : base(input)
        {

        }

        public InputStream(Java.StreamBase input) : base(input)
        { }
    }

    public class OutputStream : ByteStreamBase
    {
        public OutputStream(byte[] input) : base(input)
        {

        }

        public OutputStream(Java.StreamBase input) : base(input)
        { }
    }

    public class MappedByteBuffer : ByteStreamBase
    {
        public MappedByteBuffer() : base(new StreamBase())
        {
            
        }
    }

    public class WritableByteChannel : OutputStream
    {
        public WritableByteChannel() : base(new StreamBase())
        {
            
        }
        public WritableByteChannel(Java.StreamBase output) : base(output)
        {

        }
    }

    public class ReadableByteChannel : InputStream
    {
        public ReadableByteChannel(byte[] input) : base(input)
        {

        }

        public ReadableByteChannel(Java.StreamBase input) : base(input)
        {

        }
    }

    public class ByteArrayOutputStream : WritableByteChannel
    {
        public ByteArrayOutputStream() : base(new StreamBase())
        {

        }

        public ByteArrayOutputStream(ByteArrayOutputStream output) : base(output)
        {

        }
    }

    public class DataOutputStream : ByteArrayOutputStream
    {
        public DataOutputStream(ByteArrayOutputStream output) : base(output)
        {

        }
    }

    public class ByteArrayInputStream : ReadableByteChannel
    {
        public ByteArrayInputStream(byte[] input) : base(input)
        {

        }

        public ByteArrayInputStream(Java.StreamBase input) : base(input)
        {
        }
    }


    public class FilterInputStream : ByteArrayInputStream
    {
        public FilterInputStream(Java.StreamBase input) : base(input)
        {
        }
    }
}
