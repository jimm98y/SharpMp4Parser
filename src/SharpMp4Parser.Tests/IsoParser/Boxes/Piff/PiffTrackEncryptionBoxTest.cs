using SharpMp4Parser.IsoParser.Boxes.Microsoft;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.Piff
{
    [TestClass]
    public class PiffTrackEncryptionBoxTest : AbstractTrackEncryptionBoxTest
    {
        [TestInitialize]
        public void setUp()
        {
            tenc = new PiffTrackEncryptionBox();
        }
    }
}
