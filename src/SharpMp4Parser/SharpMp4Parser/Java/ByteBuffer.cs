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

        public static ByteArrayInputStream newInputStream(ReadableByteChannel dataSource)
        {
            throw new NotImplementedException();
        }
    }

    public class InputStream : ByteBuffer
    {
        public InputStream()
        { }

        public InputStream(Java.Buffer input) : base(input)
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

        public void write(int value)
        {
            putInt(value);
        }

        public virtual int read()
        {
            return get();
        }

        public byte[] toByteArray()
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

        internal int available()
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
            if (encoding == "UTF-8")
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






        public char getChar()
        {
            throw new NotImplementedException();
        }

        public void putChar(char value)
        {
            throw new NotImplementedException();
        }


        public int getInt()
        {
            throw new System.NotImplementedException();
        }

        public void putInt(int value)
        {
            throw new System.NotImplementedException();
        }



        public short getShort()
        {
            throw new System.NotImplementedException();
        }

        public void putShort(short value)
        {
            throw new System.NotImplementedException();
        }



        public long getLong()
        {
            throw new System.NotImplementedException();
        }

        public void putLong(long value)
        {
            throw new System.NotImplementedException();
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
            throw new System.NotImplementedException();
        }



        public void put(byte[] bytes, int v1, int v2)
        {
            throw new System.NotImplementedException();
        }


        public void get(byte[] bytes)
        {
            throw new System.NotImplementedException();
        }


        public void put(int v1, byte v2)
        {
            throw new System.NotImplementedException();
        }


        internal void order(ByteOrder endian)
        {
            throw new System.NotImplementedException();
        }

        public virtual int read(byte[] bb)
        {
            throw new NotImplementedException();
        }

        internal bool hasArray()
        {
            throw new NotImplementedException();
        }

        internal void get(byte[] b, int v, int length)
        {
            throw new NotImplementedException();
        }

    }

    public class ByteBuffer : Buffer
    {
        public ByteBuffer()
        { }

        public ByteBuffer(int capacity) : base(capacity)
        {  }

        public ByteBuffer(Java.Buffer input): base(input)
        {  }

        public ByteBuffer(MemoryStream input) : base(input)
        { }

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
            throw new System.NotImplementedException();
        }

        public ByteBuffer position(int nextBufferWritePosition)
        {
            throw new System.NotImplementedException();
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
        public ReadableByteChannel()
        {
            
        }

        public ReadableByteChannel(Java.Buffer input) : base(input)
        {
            
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
    }

    public class ByteArrayInputStream : ReadableByteChannel
    {
        public ByteArrayInputStream(byte[] input)
        {
            throw new NotImplementedException();
        }

        public ByteArrayInputStream(Java.Buffer input) : base(input)
        {
        }
    }
}
