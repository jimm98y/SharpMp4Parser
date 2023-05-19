using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15;
using SharpMp4Parser.IsoParser;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12.ItemLocationBox;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.ISO14496.Part15
{
    [TestClass]
    public class TierBitRateBoxTest : BoxRoundtripTest
    {
        [TestMethod]
        public void roundtrip()
        {
            base.roundtrip(new TierBitRateBox(),
                        new KeyValuePair<string, object>[]{
                                new KeyValuePair<string, object>("avgBitRate", 32L),
                                new KeyValuePair<string, object>("baseBitRate", (long) 21),
                                new KeyValuePair<string, object>("maxBitRate", (long) 32),
                                new KeyValuePair<string, object>("tierAvgBitRate", (long) 45),
                                new KeyValuePair<string, object>("tierBaseBitRate", (long) 65),
                                new KeyValuePair<string, object>("tierMaxBitRate", (long) 67)}
                );
        }
    }
}