using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;

namespace SharpMp4Parser.Muxer
{
    /**
     * Created by sannies on 10/15/13.
     */
    public class MemoryDataSourceImpl : DataSource
    {
        ByteBuffer data;

        public MemoryDataSourceImpl(byte[] data)
        {
            this.data = ByteBuffer.wrap(data);
        }

        public MemoryDataSourceImpl(ByteBuffer buffer)
        {
            this.data = buffer;
        }

        public int read(ByteBuffer byteBuffer)
        {
            if (0 == data.remaining() && 0 != byteBuffer.remaining())
            {
                return -1;
            }
            int size = Math.Min(byteBuffer.remaining(), data.remaining());
            if (byteBuffer.hasArray())
            {
                byteBuffer.put(data.array(), data.position(), size);
                ((Java.Buffer)data).position(data.position() + size);
            }
            else
            {
                byte[] buf = new byte[size];
                data.get(buf);
                byteBuffer.put(buf);
            }
            return size;
        }

        public long size()
        {
            return data.capacity();
        }

        public long position()
        {
            return data.position();
        }

        public void position(long nuPos)
        {
            ((Java.Buffer)data).position(CastUtils.l2i(nuPos));
        }

        public long transferTo(long position, long count, ByteStream target)
        {
            return target.write((ByteBuffer)((Java.Buffer)((ByteBuffer)((Java.Buffer)data).position(CastUtils.l2i(position))).slice()).limit(CastUtils.l2i(count)));
        }

        public ByteBuffer map(long startPosition, long size)
        {
            int oldPosition = data.position();
            ((Java.Buffer)data).position(CastUtils.l2i(startPosition));
            ByteBuffer result = data.slice();
            ((Java.Buffer)result).limit(CastUtils.l2i(size));
            ((Java.Buffer)data).position(oldPosition);
            return result;
        }

        public void close()
        {
            //nop
        }
    }
}
