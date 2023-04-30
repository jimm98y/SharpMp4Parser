using System;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    public class HintSampleEntry : AbstractSampleEntry
    {
        protected byte[] data;

        public HintSampleEntry(string type) : base(type)
        { }

        public override void parse(ReadableByteChannel dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            ByteBuffer b1 = ByteBuffer.allocate(8);
            dataSource.read(b1);
            ((Buffer)b1).position(6);
            dataReferenceIndex = IsoTypeReader.readUInt16(b1);
            data = new byte[CastUtils.l2i(contentSize - 8)];
            dataSource.read(ByteBuffer.wrap(data));
        }

        public override void getBox(WritableByteChannel writableByteChannel)
        {
            writableByteChannel.write(getHeader());

            ByteBuffer byteBuffer = ByteBuffer.allocate(8);
            ((Buffer)byteBuffer).position(6);
            IsoTypeWriter.writeUInt16(byteBuffer, dataReferenceIndex);
            ((Buffer)byteBuffer).rewind();
            writableByteChannel.write(byteBuffer);
            writableByteChannel.write(ByteBuffer.wrap(data));
        }

        public byte[] getData()
        {
            return data;
        }

        public void setData(byte[] data)
        {
            this.data = data;
        }

        public override long getSize()
        {
            long s = 8 + data.Length;
            return s + ((largeBox || (s + 8) >= (1L << 32)) ? 16 : 8);
        }
    }
}