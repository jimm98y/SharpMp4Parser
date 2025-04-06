using SharpMp4Parser.IsoParser;
using SharpMp4Parser.IsoParser.Boxes.Apple;

namespace SharpMp4Parser.Tests.IsoParser
{
    [TestClass]
    public class PropertyBoxParserImplTest
    {
        [TestMethod]
        public void test_isoparser_custom_properties()
        {
            PropertyBoxParserImpl bp = new PropertyBoxParserImpl();
            Assert.AreEqual(typeof(AppleItemListBox), bp.mapping["meta-ilst"]);
        }
    }
}
