using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System.Text;

namespace SharpMp4Parser.Muxer
{
    public class SampleImpl : Sample
    {
        private readonly long offset;
        private readonly long size;
        private ByteBuffer[] data;
        private SampleEntry sampleEntry;

        public SampleImpl(ByteBuffer buf, SampleEntry sampleEntry)
        {
            this.offset = -1;
            this.size = buf.limit();
            this.data = new ByteBuffer[] { buf };
            this.sampleEntry = sampleEntry;
        }

        public SampleImpl(ByteBuffer[] data, SampleEntry sampleEntry)
        {
            this.offset = -1;
            int _size = 0;
            foreach (ByteBuffer byteBuffer in data)
            {
                _size += byteBuffer.remaining();
            }
            this.size = _size;
            this.data = data;
            this.sampleEntry = sampleEntry;
        }

        public SampleImpl(long offset, long sampleSize, ByteBuffer data, SampleEntry sampleEntry)
        {
            this.offset = offset;
            this.size = sampleSize;
            this.data = new ByteBuffer[] { data };
            this.sampleEntry = sampleEntry;
        }

        public void writeTo(WritableByteChannel channel)
        {
            foreach (ByteBuffer b in data)
            {
                channel.write(b.duplicate());
            }
        }

        public override SampleEntry getSampleEntry()
        {
            return sampleEntry;
        }

        public long getSize()
        {
            return size;
        }

        public ByteBuffer asByteBuffer()
        {
            byte[] bCopy = new byte[CastUtils.l2i(size)];
            ByteBuffer copy = ByteBuffer.wrap(bCopy);
            foreach (ByteBuffer b in data)
            {
                copy.put(b.duplicate());
            }
            ((Java.Buffer)copy).rewind();
            return copy;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SampleImpl");
            sb.Append("{offset=").Append(offset);
            sb.Append("{size=").Append(size);
            sb.Append('}');
            return sb.ToString();
        }
    }
}
