using SharpMp4Parser.IsoParser;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.Tests.IsoParser.Boxes;

namespace SharpMp4Parser.Tests.IsoParser.Tools.Boxes
{
    /**
     * Created by sannies on 23.05.13.
     */
    [TestClass]
    public class SampleTableBoxTest : BoxWriteReadBase<SampleTableBox> {

        public override Type getBoxUnderTest()
        {
            return typeof(SampleTableBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, SampleTableBox box)
        {
            addPropsHere.Add("boxes", new List<Box>() { new FreeBox(100) });
        }
    }
}
