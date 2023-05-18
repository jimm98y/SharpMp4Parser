using SharpMp4Parser.IsoParser.Boxes.SampleEntry;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.SampleEntry
{
    /**
     * Created by sannies on 23.05.13.
     */
    [TestClass]
    public class VisualSampleEntryTest : BoxWriteReadBase<VisualSampleEntry>
    {

        public override Type getBoxUnderTest()
        {
            return typeof(VisualSampleEntry);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, VisualSampleEntry box)
        {

        }
    }
}