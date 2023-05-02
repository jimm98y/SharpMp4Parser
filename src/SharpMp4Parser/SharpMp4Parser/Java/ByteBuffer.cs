using System;
using System.IO;

namespace SharpMp4Parser.Java
{
    internal class BufferedReader
    {
        private InputStreamReader inputStreamReader;

        public BufferedReader(InputStreamReader inputStreamReader)
        {
            this.inputStreamReader = inputStreamReader;
        }

        internal string readLine()
        {
            throw new NotImplementedException();
        }
    }

    internal class InputStreamReader
    {
        private ByteArrayInputStream input;
        private string v;

        public InputStreamReader(ByteArrayInputStream input, string v)
        {
            this.input = input;
            this.v = v;
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
            throw new NotImplementedException();
        }
    }

    public class InputStream : ByteBuffer
    {
        internal int available()
        {
            throw new NotImplementedException();
        }

    }

    public class Buffer : MemoryStream
    {
        public Buffer()
        { }

        public Buffer(int capacity) : base(capacity)
        { }

        public ByteBuffer limit(int limit)
        {
            throw new System.NotImplementedException();
        }

        public int limit()
        {
            throw new System.NotImplementedException();
        }

        public ByteBuffer position(int nextBufferWritePosition)
        {
            throw new System.NotImplementedException();
        }

        public ByteBuffer rewind()
        {
            throw new System.NotImplementedException();
        }
    }

    public class ByteBuffer : Buffer
    {
        public ByteBuffer()
        { }

        public ByteBuffer(int capacity) : base(capacity)
        {  }

        public static ByteBuffer allocate(int bufferCapacity)
        {
            return new ByteBuffer(bufferCapacity);
        }

        public static ByteBuffer wrap(byte[] bytes)
        {
            throw new System.NotImplementedException();
        }

        public int capacity()
        {
            return this.Capacity;
        }

        public ByteBuffer duplicate()
        {
            throw new System.NotImplementedException();
        }

        public byte get()
        {
            return (byte)this.ReadByte();
        }

        public byte get(int i)
        {
            throw new System.NotImplementedException();
        }

        public int getInt()
        {
            throw new System.NotImplementedException();
        }

        public short getShort()
        {
            throw new System.NotImplementedException();
        }

        public int position()
        {
            return (int)this.Position;
        }

        public ByteBuffer position(long position)
        {
            this.Seek(position, SeekOrigin.Begin);
            return this;
        }

        public void put(byte[] data)
        {
            this.Write(data, 0, data.Length);
        }

        public void put(ByteBuffer buffer)
        {
            byte[] data = buffer.ToArray();
            put(data);
        }

        public void put(byte[] bytes, int v1, int v2)
        {
            throw new System.NotImplementedException();
        }

        public void put(byte v)
        {
            throw new System.NotImplementedException();
        }

        public void putInt(int dataType)
        {
            throw new System.NotImplementedException();
        }

        public int remaining()
        {
            return (int)(this.Length - this.Position);
        }

        public ByteBuffer reset()
        {
            throw new System.NotImplementedException();
        }

        public ByteBuffer slice()
        {
            throw new System.NotImplementedException();
        }

        public void get(byte[] bytes)
        {
            throw new System.NotImplementedException();
        }

        public byte[] array()
        {
            return this.ToArray();
        }

        public void putShort(short b)
        {
            throw new System.NotImplementedException();
        }

        public void put(int v1, byte v2)
        {
            throw new System.NotImplementedException();
        }

        public void putLong(long value)
        {
            throw new System.NotImplementedException();
        }

        public long getLong()
        {
            throw new System.NotImplementedException();
        }

        internal void order(object bIG_ENDIAN)
        {
            throw new System.NotImplementedException();
        }

        internal char getChar()
        {
            throw new NotImplementedException();
        }

        public void putChar(char v)
        {
            throw new NotImplementedException();
        }

        public virtual int write(ByteBuffer value)
        {
            put(value);
            return (int)value.Length;
        }

        public virtual int write(byte[] value)
        {
            put(value);
            return (int)value.Length;
        }

        public void write(int value)
        {
            putInt(value);
        }

        public virtual int read(ByteBuffer bb)
        {
            throw new System.NotImplementedException();
        }

        public virtual int read(byte[] bb)
        {
            throw new System.NotImplementedException();
        }


        public virtual int read()
        {
            return get();
        }

        public byte[] toByteArray()
        {
            return this.ToArray();
        }

        public int hashCode()
        {
            return GetHashCode();
        }

        internal int arrayOffset()
        {
            throw new NotImplementedException();
        }

        internal bool hasArray()
        {
            throw new NotImplementedException();
        }

        internal static ByteBuffer wrap(byte[] bytes, int v1, int v2)
        {
            throw new NotImplementedException();
        }

        internal bool hasRemaining()
        {
            throw new NotImplementedException();
        }

        internal void get(byte[] b, int v, int length)
        {
            throw new NotImplementedException();
        }
    }

    public class MappedByteBuffer : ByteBuffer
    {

    }

    public class WritableByteChannel : ByteBuffer
    {

        public WritableByteChannel()
        {
            
        }

        public WritableByteChannel(ByteBuffer vtte)
        {
            throw new NotImplementedException();
        }
    }

    public class ReadableByteChannel : InputStream
    {
        public virtual void close()
        {
            throw new NotImplementedException();
        }

        public virtual bool isOpen()
        {
            throw new NotImplementedException();
        }
    }

    public class ByteArrayOutputStream : WritableByteChannel
    {
    }

    public class DataOutputStream : ByteArrayOutputStream
    {
        private ByteArrayOutputStream baos;

        public DataOutputStream(ByteArrayOutputStream baos)
        {
            this.baos = baos;
        }

        internal void close()
        {
            throw new NotImplementedException();
        }

        internal void writeShort(short length)
        {
            throw new NotImplementedException();
        }
    }

    public class ByteArrayInputStream : ReadableByteChannel
    {
        private Stream input;

        public ByteArrayInputStream(byte[] input)
        {
            throw new NotImplementedException();
        }

        public ByteArrayInputStream(Stream input)
        {
            this.input = input;
        }
    }
}
