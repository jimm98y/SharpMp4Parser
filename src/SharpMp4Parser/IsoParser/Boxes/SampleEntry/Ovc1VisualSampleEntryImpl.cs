﻿using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.SampleEntry
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

        public override void parse(ByteStream dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            ByteBuffer byteBuffer = ByteBuffer.allocate(CastUtils.l2i(contentSize));
            dataSource.read(byteBuffer);
            ((Buffer)byteBuffer).position(6);
            dataReferenceIndex = IsoTypeReader.readUInt16(byteBuffer);
            vc1Content = new byte[byteBuffer.remaining()];
            byteBuffer.get(vc1Content);
        }


        public override void getBox(ByteStream writableByteChannel)
        {
            writableByteChannel.write(getHeader());
            ByteBuffer byteBuffer = ByteBuffer.allocate(8);
            ((Buffer)byteBuffer).position(6);
            IsoTypeWriter.writeUInt16(byteBuffer, dataReferenceIndex);
            writableByteChannel.write((ByteBuffer)byteBuffer.rewind());
            writableByteChannel.write(ByteBuffer.wrap(vc1Content));
        }

        public override long getSize()
        {
            long header = largeBox || vc1Content.LongLength + 16 >= (1L << 32) ? 16 : 8;
            return header + vc1Content.Length + 8;
        }
    }
}
