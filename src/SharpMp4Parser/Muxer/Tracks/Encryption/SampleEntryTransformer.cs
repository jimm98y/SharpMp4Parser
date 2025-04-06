using SharpMp4Parser.IsoParser.Boxes.SampleEntry;

namespace SharpMp4Parser.Muxer.Tracks.Encryption
{
    public interface SampleEntryTransformer
    {
        SampleEntry transform(SampleEntry se);
    }
}
