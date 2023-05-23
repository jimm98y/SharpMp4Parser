using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.Tests.IsoParser.Tools
{
    [TestClass]
    public class PathTest
    {
        [TestMethod]
        public void setup()
        {
            FileStream multiFis = File.OpenRead("multiTrack.3gp");

            var isoBuff = new ByteStream(multiFis);

            IsoFile isoFile = new IsoFile(isoBuff);
        }
    }
}
