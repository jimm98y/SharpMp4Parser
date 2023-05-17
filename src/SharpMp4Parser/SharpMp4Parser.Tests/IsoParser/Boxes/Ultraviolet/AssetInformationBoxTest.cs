using SharpMp4Parser.IsoParser.Boxes.Dece;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.Ultraviolet
{
    [TestClass()]
    public class AssetInformationBoxTest : BoxRoundtripTest
    {
        [TestMethod]
        public void roundtrip()
        {
            base.roundtrip(new AssetInformationBox(),
                        new KeyValuePair<string, object>[]{
                                new KeyValuePair<string, object>("apid", "urn:dece:apid:org:castlabs:abc"),
                                //new KeyValuePair<string, object>("hidden", false), // hidden is not a field in c#
                                new KeyValuePair<string, object>("profileVersion", "1001")});

            base.roundtrip(new AssetInformationBox(),
                        new KeyValuePair<string, object>[]{
                                new KeyValuePair<string, object>("apid", "urn:dece:apid:org:castlabs:abc2"),
                                //new KeyValuePair<string, object>("hidden", true),
                                new KeyValuePair<string, object>("profileVersion", "0001")});
        }
    }
}