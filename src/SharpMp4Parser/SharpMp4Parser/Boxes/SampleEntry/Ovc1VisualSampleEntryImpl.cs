using SharpMp4Parser.Java;
using SharpMp4Parser.Tools;

namespace SharpMp4Parser.Boxes.SampleEntry
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     */
    public class Ovc1VisualSampleEntryImpl : AbstractSampleEntry
    {
        public const string TYPE = "ovc1";
        private byte[] vc1Content = new byte[0];

        public Ovc1VisualSampleEntryImpl() : base(TYPE)
        { }

        public byte[] getVc1Content()
        {
            return vc1Content;
        }

        public void setVc1Content(byte[] vc1Content)
        {
            this.vc1Content = vc1Content;
        }

        public override void parse(ReadableByteChannel dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            ByteBuffer byteBuffer = ByteBuffer.allocate(CastUtils.l2i(contentSize));
            dataSource.read(byteBuffer);
            ((Buffer)byteBuffer).position(6);
            dataReferenceIndex = IsoTypeReader.readUInt16(byteBuffer);
            vc1Content = new byte[byteBuffer.remaining()];
            byteBuffer.get(vc1Content);
        }


        public override void getBox(WritableByteChannel writableByteChannel)
        {
            writableByteChannel.write(getHeader());
            ByteBuffer byteBuffer = ByteBuffer.allocate(8);
            ((Buffer)byteBuffer).position(6);
            IsoTypeWriter.writeUInt16(byteBuffer, dataReferenceIndex);
            writableByteChannel.write((ByteBuffer)((Buffer)byteBuffer).rewind());
            writableByteChannel.write(ByteBuffer.wrap(vc1Content));
        }

        public override long getSize()
        {
            long header = (largeBox || (vc1Content.Length + 16) >= (1 << 32)) ? 16 : 8;
            return header + vc1Content.Length + 8;
        }
    }
}
