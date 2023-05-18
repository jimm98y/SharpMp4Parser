using SharpMp4Parser.IsoParser;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.Tests.IsoParser.Boxes;

namespace SharpMp4Parser.Tests.IsoParser.Tools.Boxes
{
    /**
     * Created by sannies on 25.05.13.
     */
    [TestClass]
    public class SampleDescriptionBoxTest : BoxWriteReadBase<SampleDescriptionBox>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(SampleDescriptionBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, SampleDescriptionBox box)
        {
            addPropsHere.Add("boxes", new List<Box>() { new FreeBox(100) });
        }
    }
}
