using SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7;
using SharpMp4Parser.Java;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.ISO23001.Part7
{
    [TestClass]
    public class ProtectionSystemSpecificHeaderBoxTest : BoxRoundtripTest
    {
        [TestMethod]
        public void roundtrip()
        {
            base.roundtrip(new ProtectionSystemSpecificHeaderBox(),
                            new KeyValuePair<string, object>[]{
                                new KeyValuePair<string, object>("systemId", ProtectionSystemSpecificHeaderBox.OMA2_SYSTEM_ID),
                                new KeyValuePair<string, object>("content", new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 0})});
            base.roundtrip(new ProtectionSystemSpecificHeaderBox(),
                            new KeyValuePair<string, object>[]{
                                new KeyValuePair<string, object>("version", 1),
                                //new KeyValuePair<string, object>("keyIds", new List<Uuid>() { Uuid.randomUUID(), Uuid.randomUUID() }),
                                new KeyValuePair<string, object>("systemId", ProtectionSystemSpecificHeaderBox.OMA2_SYSTEM_ID),
                                new KeyValuePair<string, object>("content", new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 0})});
        }
    }
}
