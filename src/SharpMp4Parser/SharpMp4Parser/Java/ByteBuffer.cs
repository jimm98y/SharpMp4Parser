using System;
using System.IO;
using System.Text;

namespace SharpMp4Parser.Java
{
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

    public class InputStream : ByteBuffer
    {
        public InputStream()
        { }

        public InputStream(byte[] input) : base(input)
        {

        }

        public InputStream(Java.Buffer input) : base(input)
        { }
    }

    public class OutputStream : ByteBuffer
    {
        public OutputStream()
        { }

        public OutputStream(byte[] input) : base(input)
        {

        }

        public OutputStream(Java.Buffer input) : base(input)
        { }
    }

    public class Buffer : Closeable
    {
        protected MemoryStream _ms = new MemoryStream();

        public Buffer()
        {
            _ms = new MemoryStream();
        }

        public Buffer(int capacity)
        {
            _ms = new MemoryStream(capacity);
        }

        public Buffer(Buffer input)
        {
            _ms = input._ms;
        }

        public Buffer(MemoryStream input)
        {
            _ms = input;
        }

        public Buffer(byte[] input)
        {
            _ms = new MemoryStream(input);
        }

        public virtual Buffer duplicate()
        {
            return new Buffer(_ms.ToArray());
        }

        public int capacity()
        {
            return _ms.Capacity;
        }

        public byte[] array()
        {
            return _ms.ToArray();
        }

        public int remaining()
        {
            return (int)(_ms.Length - _ms.Position);
        }

        public int position()
        {
            return (int)_ms.Position;
        }

        public Buffer position(long position)
        {
            _ms.Seek(position, SeekOrigin.Begin);
            return this;
        }

        public void put(byte[] data)
        {
            _ms.Write(data, 0, data.Length);
        }

        public void put(Buffer buffer)
        {
            byte[] data = buffer.toByteArray();
            put(data);
        }

        public virtual int write(Buffer value)
        {
            put(value);
            return (int)(value.toByteArray()).Length;
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

        public virtual int read()
        {
            return get();
        }

        public virtual byte[] toByteArray()
        {
            return _ms.ToArray();
        }

        public int hashCode()
        {
            return GetHashCode();
        }

        public Buffer rewind()
        {
            _ms.Seek(0, SeekOrigin.Begin);
            return this;
        }

        public virtual long available()
        {
            // https://docs.oracle.com/javase/8/docs/api/java/io/InputStream.html
            // Returns an estimate of the number of bytes that can be read (or skipped over) from this input stream without blocking by the next invocation of a method for this input stream.
            return remaining();
        }

        internal bool hasRemaining()
        {
            return remaining() > 0;
        }

        public virtual void close()
        {
            _ms.Close();
        }

        public virtual bool isOpen()
        {
            return _ms.CanRead || _ms.CanWrite;
        }

        public int arrayOffset()
        {
            return position();
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

        public byte get()
        {
            return (byte)_ms.ReadByte();
        }

        public void put(byte value)
        {
            _ms.WriteByte(value);
        }

        public int limit()
        {
            return this.capacity();
        }

        public Buffer limit(int limit)
        {
            this._ms.Capacity = limit;
            return this;
        }

        public byte get(int i)
        {
            return _ms.ToArray()[i];
        }

        public virtual int read(byte[] bb)
        {
            return get(bb, 0, bb.Length);
        }

        public virtual int read(byte[] bytes, int offset, int length)
        {
            return get(bytes, offset, length);
        }

        public void get(byte[] bytes)
        {
            get(bytes, 0, bytes.Length);
        }

        internal int get(byte[] bytes, int offset, int length)
        {
            return _ms.Read(bytes, offset, length);
        }

        internal bool hasArray()
        {
            return true;
        }

        public void put(byte[] bytes, int offset, int length)
        {
            _ms.Write(bytes, offset, length);
        }

        public void put(int index, byte value)
        {
            _ms.Seek(index, SeekOrigin.Begin);
            _ms.WriteByte(value);
        }

        public char getChar()
        {
            using (BinaryReader br = new BinaryReader(_ms))
            {
                return br.ReadChar();
            }
        }

        public void putChar(char value)
        {
            using (BinaryWriter br = new BinaryWriter(_ms))
            {
                br.Write(value);
            }
        }

        public int getInt()
        {
            using (BinaryReader br = new BinaryReader(_ms))
            {
                return br.ReadInt32();
            }
        }

        public void putInt(int value)
        {
            using (BinaryWriter br = new BinaryWriter(_ms))
            {
                br.Write(value);
            }
        }

        public short getShort()
        {
            using (BinaryReader br = new BinaryReader(_ms))
            {
                return br.ReadInt16();
            }
        }

        public void putShort(short value)
        {
            using (BinaryWriter br = new BinaryWriter(_ms))
            {
                br.Write(value);
            }
        }

        public long getLong()
        {
            using (BinaryReader br = new BinaryReader(_ms))
            {
                return br.ReadInt64();
            }
        }

        public void putLong(long value)
        {
            using (BinaryWriter br = new BinaryWriter(_ms))
            {
                br.Write(value);
            }
        }

        internal void order(ByteOrder endian)
        {
            if(endian == ByteOrder.BIG_ENDIAN)
            {
                throw new NotImplementedException();
            }
        }
    }

    public class ByteBuffer : Buffer
    {
        public ByteBuffer() : base()
        { }

        public ByteBuffer(int capacity) : base(capacity)
        {  }

        public ByteBuffer(Java.Buffer input): base(input)
        {  }

        public ByteBuffer(MemoryStream input) : base(input)
        { }

        public ByteBuffer(byte[] input) : base(input)
        {  }

        public override Buffer duplicate()
        {
            return new ByteBuffer(new MemoryStream(_ms.ToArray()));
        }

        public ByteBuffer slice()
        {
            return new ByteBuffer(this._ms);
        }

        public static ByteBuffer wrap(byte[] bytes)
        {
            return wrap(bytes, 0, bytes.Length);
        }

        public static ByteBuffer wrap(byte[] bytes, int offset, int length)
        {
            return new ByteBuffer(new MemoryStream(bytes, offset, length));
        }

        public static ByteBuffer allocate(int bufferCapacity)
        {
            return new ByteBuffer(bufferCapacity);
        }

        public virtual int read(ByteBuffer bb)
        {
            throw new System.NotImplementedException();
        }

        public ByteBuffer reset()
        {
            position(0);
            return this;
        }

        public ByteBuffer position(int nextBufferWritePosition)
        {
            base.position(nextBufferWritePosition);
            return this;
        }
    }

    public class MappedByteBuffer : ByteBuffer
    {

    }

    public class WritableByteChannel : OutputStream
    {

        public WritableByteChannel()
        {
            
        }

        public WritableByteChannel(ByteBuffer output) : base(output)
        {
            
        }
    }

    public class ReadableByteChannel : InputStream
    {
        public ReadableByteChannel()
        {
            
        }

        public ReadableByteChannel(byte[] input) : base(input)
        {

        }

        public ReadableByteChannel(Java.Buffer input) : base(input)
        {
            
        }
    }

    public class ByteArrayOutputStream : WritableByteChannel
    {
        public ByteArrayOutputStream()
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

        public ByteArrayInputStream(Java.Buffer input) : base(input)
        {
        }
    }


    public class FilterInputStream : ByteArrayInputStream
    {
        public FilterInputStream(byte[] input) : base(input)
        {
        }

        public FilterInputStream(Java.Buffer input) : base(input)
        {
        }
    }

}
