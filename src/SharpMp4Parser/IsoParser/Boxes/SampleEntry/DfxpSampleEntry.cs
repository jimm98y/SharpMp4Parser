﻿using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.SampleEntry
{
    public class DfxpSampleEntry : AbstractSampleEntry
    {
        public DfxpSampleEntry() : base("dfxp")
        { }

        public override void parse(ByteStream dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            ByteBuffer content = ByteBuffer.allocate(8);
            dataSource.read(content);
            ((Buffer)content).position(6);
            dataReferenceIndex = IsoTypeReader.readUInt16(content);
        }

        public override void getBox(ByteStream writableByteChannel)
        {
            writableByteChannel.write(getHeader());
            ByteBuffer byteBuffer = ByteBuffer.allocate(8);
            ((Buffer)byteBuffer).position(6);
            IsoTypeWriter.writeUInt16(byteBuffer, dataReferenceIndex);
            writableByteChannel.write(byteBuffer);
        }

        public override long getSize()
        {
            long s = getContainerSize();
            long t = 8;
            return s + t + (largeBox || s + t + 8 >= 1L << 32 ? 16 : 8);
        }
    }
}
