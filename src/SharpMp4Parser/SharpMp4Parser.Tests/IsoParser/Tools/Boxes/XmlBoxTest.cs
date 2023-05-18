using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.Tests.IsoParser.Boxes;

namespace SharpMp4Parser.Tests.IsoParser.Tools.Boxes
{
    /**
     *
     */
    [TestClass]
    public class XmlBoxTest : BoxWriteReadBase<XmlBox>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(XmlBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, XmlBox box)
        {
            addPropsHere.Add("xml", "<a></a>");
        }
    }
}