using SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7;

namespace SharpMp4Parser.Tests.IsoParser.BaseMediaFormat
{
    [TestClass]
    public class TrackEncryptionBoxTest : AbstractTrackEncryptionBoxTest
    {
        [TestInitialize]
        public void setUp()
        {
            tenc = new TrackEncryptionBox();
        }
    }
}