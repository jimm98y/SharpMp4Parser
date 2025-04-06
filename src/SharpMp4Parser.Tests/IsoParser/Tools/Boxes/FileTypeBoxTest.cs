using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.Tests.IsoParser.Boxes;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Tools.Boxes
{
    [TestClass]
    public class FileTypeBoxTest : BoxRoundtripTest
    {
        [TestMethod]
        public void roundtrip()
        {
            base.roundtrip(new FileTypeBox(),
                        new KeyValuePair<string, object>[]{
                                new KeyValuePair<string, object>("majorBrand", "mp45"),
                                new KeyValuePair<string, object>("minorVersion", 0x124334L),
                                new KeyValuePair<string, object>("compatibleBrands", new List<string>() { "abcd", "hjkl" })}
                );
        }
    }
}
