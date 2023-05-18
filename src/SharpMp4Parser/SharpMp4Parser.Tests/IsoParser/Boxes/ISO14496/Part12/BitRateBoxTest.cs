using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.ISO14496.Part12
{
    [TestClass]
    public class BitRateBoxTest : BoxRoundtripTest
    {
        [TestMethod]
        public void roundtrip()
        {
            base.roundtrip(
                new BitRateBox(),
                        new KeyValuePair<string,object>[]{
                                new KeyValuePair<string,object>("bufferSizeDb", 1L),
                                new KeyValuePair<string,object>("maxBitrate", 1L),
                                new KeyValuePair<string,object>("avgBitrate", 21L)}
                );
    }
}
}
