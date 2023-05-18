using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part30;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.ISO14496.Part30
{
    [TestClass]
    public class WebVTTSampleEntryTest : BoxRoundtripTest
    {
        [TestMethod]
        public void roundtrip()
        {
            WebVTTSampleEntry wvtt = new WebVTTSampleEntry();
            WebVTTConfigurationBox vttC = new WebVTTConfigurationBox();
            vttC.setConfig("abc");
            WebVTTSourceLabelBox vlab = new WebVTTSourceLabelBox();
            vlab.setSourceLabel("dunno");
            wvtt.addBox(vttC);
            wvtt.addBox(vlab);

            base.roundtrip(wvtt,
                        new KeyValuePair<string, object>[] { }
                );
        }
    }
}
