using SharpMp4Parser.Java;
using SharpMp4Parser.Tools;

namespace SharpMp4Parser.Boxes.SampleEntry
{
    public class MpegSampleEntry : AbstractSampleEntry
    {
        public MpegSampleEntry() : base("mp4s")
        { }

        public MpegSampleEntry(string type) : base(type)
        { }

        public override void parse(ReadableByteChannel dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            ByteBuffer bb = ByteBuffer.allocate(8);
            dataSource.read(bb);
            ((Java.Buffer)bb).position(6);// ignore 6 reserved bytes;
            dataReferenceIndex = IsoTypeReader.readUInt16(bb);
            initContainer(dataSource, contentSize - 8, boxParser);
        }

        public override void getBox(WritableByteChannel writableByteChannel)
        {
            writableByteChannel.write(getHeader());
            ByteBuffer bb = ByteBuffer.allocate(8);
            ((Java.Buffer)bb).position(6);
            IsoTypeWriter.writeUInt16(bb, dataReferenceIndex);
            writableByteChannel.write((ByteBuffer)((Java.Buffer)bb).rewind());
            writeContainer(writableByteChannel);
        }

        public override string ToString()
        {
            return "MpegSampleEntry" + getBoxes();
        }


        public override long getSize()
        {
            long s = getContainerSize();
            long t = 8; // bytes to container start
            return s + t + ((largeBox || (s + t) >= (1L << 32)) ? 16 : 8);
        }
    }
}
