using SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.Tests.IsoParser
{
    public abstract class AbstractTrackEncryptionBoxTest
    {

        protected AbstractTrackEncryptionBox tenc;

        [TestMethod]
        public void testRoundTrip()
        {
            tenc.setDefault_KID(UUIDConverter.convert(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6 }));
            tenc.setDefaultAlgorithmId(0x0a0b0c);
            tenc.setDefaultIvSize(8);


            ByteStream fc = new ByteStream();

            tenc.getBox(fc);

            fc.position(0);

            IsoFile iso = new IsoFile(fc);
            Assert.IsTrue(iso.getBoxes()[0] is AbstractTrackEncryptionBox);
            AbstractTrackEncryptionBox tenc2 = (AbstractTrackEncryptionBox)iso.getBoxes()[0];
            Assert.AreEqual(0, tenc2.getFlags());
            Assert.IsTrue(tenc.Equals(tenc2));
            Assert.IsTrue(tenc2.Equals(tenc));
            iso.close();

        }
    }
}
