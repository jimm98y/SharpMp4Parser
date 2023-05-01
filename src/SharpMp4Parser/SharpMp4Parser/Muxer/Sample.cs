using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.Muxer
{
    public interface Sample
    {
        void writeTo(WritableByteChannel channel);

        long getSize();

        ByteBuffer asByteBuffer();

        SampleEntry getSampleEntry();
    }
}
