using SharpMp4Parser.IsoParser;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.SampleEntry
{
    /**
     * Created by sannies on 21.05.13.
     */
    [TestClass]
    public class MpegSampleEntryTest : BoxWriteReadBase<MpegSampleEntry>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(MpegSampleEntry);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, MpegSampleEntry box)
        {
            addPropsHere.Add("boxes", new List<Box>() { new FreeBox(100) });
            addPropsHere.Add("dataReferenceIndex", (int)4344);
        }
    }
}