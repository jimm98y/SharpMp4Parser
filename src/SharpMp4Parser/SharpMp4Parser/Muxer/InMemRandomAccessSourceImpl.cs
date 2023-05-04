using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.Muxer
{
    /**
     * Typically used for tests.
     */
    public class InMemRandomAccessSourceImpl : RandomAccessSource
    {
        ByteBuffer buffer;

        public InMemRandomAccessSourceImpl(ByteBuffer buffer)
        {
            this.buffer = (ByteBuffer)buffer.duplicate();
        }

        public InMemRandomAccessSourceImpl(byte[] b)
        {
            buffer = ByteBuffer.wrap(b);
        }

        private object _syncRoot = new object();

        public ByteBuffer get(long offset, long size)
        {
            lock (_syncRoot)
            {
                ((Java.Buffer)buffer).position(CastUtils.l2i(offset));
                return (ByteBuffer)((Java.Buffer)buffer.slice()).limit(CastUtils.l2i(size));
            }
        }

        public void close()
        {

        }
    }
}