using SharpMp4Parser.IsoParser;
using System.Text;

namespace SharpMp4Parser.Tests.IsoParser.Tools
{
    /**
     *
     */
    [TestClass]
    public class IsoFileTest
    {
        [TestMethod]
        public void testFourCC()
        {
            Assert.AreEqual("AA\0\0", IsoFile.bytesToFourCC(new byte[] { 65, 65 }));
            Assert.AreEqual("AAAA", IsoFile.bytesToFourCC(new byte[] { 65, 65, 65, 65, 65, 65 }));
            Assert.AreEqual("AAAA", Encoding.UTF8.GetString(IsoFile.fourCCtoBytes("AAAAAAA")));
            Assert.AreEqual("AA\0\0", Encoding.UTF8.GetString(IsoFile.fourCCtoBytes("AA")));
            Assert.AreEqual("\0\0\0\0", Encoding.UTF8.GetString(IsoFile.fourCCtoBytes(null)));
            Assert.AreEqual("\0\0\0\0", Encoding.UTF8.GetString(IsoFile.fourCCtoBytes("")));
            Assert.AreEqual("\0\0\0\0", IsoFile.bytesToFourCC(null));
            Assert.AreEqual("\0\0\0\0", IsoFile.bytesToFourCC(new byte[0]));
        }
    }
}