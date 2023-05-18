using SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.ISO23001.Part7
{
    [TestClass]
    public class TrackEncryptionBoxTest : BoxRoundtripTest
    {
        [TestMethod]
        public void roundtrip()
        {
            base.roundtrip(
                new TrackEncryptionBox(),
                        new KeyValuePair<string, object>[]{
                                new KeyValuePair<string, object>("default_KID", Uuid.randomUUID()),
                                new KeyValuePair<string, object>("defaultAlgorithmId", 0x1),
                                new KeyValuePair<string, object>("defaultIvSize", 8)
                        }
                );
    }
}
}
