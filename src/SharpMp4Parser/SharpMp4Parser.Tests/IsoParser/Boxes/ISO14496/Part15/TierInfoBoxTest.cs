using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.ISO14496.Part15
{
    [TestClass]
    public class TierInfoBoxTest : BoxRoundtripTest
    {
        [TestMethod]
        public void roundtrip()
        {
            base.roundtrip(new TierInfoBox(),
                        new KeyValuePair<string, object>[]{
                                new KeyValuePair<string, object>("constantFrameRate", (int) 1),
                                new KeyValuePair<string, object>("discardable", (int) 2),
                                new KeyValuePair<string, object>("frameRate", (int) 32),
                                new KeyValuePair<string, object>("levelIndication", (int) 2),
                                new KeyValuePair<string, object>("profileIndication", (int) 3),
                                new KeyValuePair<string, object>("profile_compatibility", (int) 4),
                                new KeyValuePair<string, object>("reserved1", (int) 0),
                                new KeyValuePair<string, object>("reserved2", (int) 0),
                                new KeyValuePair<string, object>("tierID", (int) 21),
                                new KeyValuePair<string, object>("visualHeight", (int) 100),
                                new KeyValuePair<string, object>("visualWidth", (int) 200)
                        }
                );

        }
    }
}
