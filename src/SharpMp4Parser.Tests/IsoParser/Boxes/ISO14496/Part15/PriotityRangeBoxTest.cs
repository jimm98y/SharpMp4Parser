using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.ISO14496.Part15
{
    [TestClass]
    public class PriotityRangeBoxTest : BoxRoundtripTest
    {
        [TestMethod]
        public void roundtrip()
        {
            base.roundtrip(new PriotityRangeBox(),
                        new KeyValuePair<string, object>[]{
                                new KeyValuePair<string, object>("reserved1", 1),
                                new KeyValuePair<string, object>("min_priorityId", 21),
                                new KeyValuePair<string, object>("reserved2", 2),
                                new KeyValuePair<string, object>("max_priorityId", 61)}
                );
        }
    }
}