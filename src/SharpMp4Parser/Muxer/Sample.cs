using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.Muxer
{
    public interface Sample
    {
        void writeTo(ByteStream channel);

        long getSize();

        ByteBuffer asByteBuffer();

        SampleEntry getSampleEntry();
    }
}
