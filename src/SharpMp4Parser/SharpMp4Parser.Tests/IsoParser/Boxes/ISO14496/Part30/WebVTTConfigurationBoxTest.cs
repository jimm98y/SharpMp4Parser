using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part30;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.ISO14496.Part30
{
    [TestClass]
    public class WebVTTConfigurationBoxTest : BoxRoundtripTest
    {
        [TestMethod]
        public void roundtrip()
        {
            base.roundtrip(new WebVTTConfigurationBox(),
                        new KeyValuePair<string, object>[]{
                                new KeyValuePair<string, object>("config", "1234 \n ljhsdjkshdj \n\n")}
                );
        }
    }
}
