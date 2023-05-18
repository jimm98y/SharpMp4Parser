using SharpMp4Parser.IsoParser.Boxes.Apple;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.IsoParser;
using System.Diagnostics;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.Apple
{
    [TestClass]
    public class Apple_geIDBoxTest : BoxWriteReadBase<Apple_geIDBox>
    {

        public override Type getBoxUnderTest()
        {
            return typeof(Apple_geIDBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, Apple_geIDBox box)
        {
            addPropsHere.Add("value", 1233L);
        }

        [TestMethod]
        public void testRealLifeBox()
        {
            Apple_geIDBox geid = (Apple_geIDBox)new IsoFile(new ByteBufferByteChannel(Hex.decodeHex("0000001C67654944000000146461746100000015000000000000000A"))).getBoxes()[0];
            Debug.WriteLine(geid.getValue());
        }
    }
}
