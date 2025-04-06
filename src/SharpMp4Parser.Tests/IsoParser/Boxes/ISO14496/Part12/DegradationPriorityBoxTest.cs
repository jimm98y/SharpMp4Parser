using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.ISO14496.Part12
{
    /**
     * Created by sannies on 06.08.2015.
     */
    [TestClass]
    public class DegradationPriorityBoxTest : BoxRoundtripTest
    {
        [TestMethod]
        public void roundtrip()
        {
            base.roundtrip(new DegradationPriorityBox(),
                        new KeyValuePair<string, object>[]{
                                new KeyValuePair<string, object>("priorities", new int[]{1, 2, 4, 6, 8, 2, 22, 4343, 6545, 44})}
                );
        }
    }
}
