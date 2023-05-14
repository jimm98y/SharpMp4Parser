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
            using (MemoryStream multiMs = new MemoryStream())
            {
                FileStream multiFis = File.OpenRead("multiTrack.3gp");
                multiFis.CopyTo(multiMs);
                multiMs.Position = 0;

                var isoBuff = new ByteStream(multiMs.ToArray());

                IsoFile isoFile = new IsoFile(isoBuff);
            }
        }
    }
}
