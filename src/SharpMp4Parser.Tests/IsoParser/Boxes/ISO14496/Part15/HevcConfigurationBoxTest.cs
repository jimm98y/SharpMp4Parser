using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System.Linq;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.ISO14496.Part15
{
    [TestClass]
    public class HevcConfigurationBoxTest
    {
        byte[] input = Hex.decodeHex("000000E1687663310000000000000001000000000000000000000000000000000780043800480000004800000000000000010C0B4845564320436F64696E67000000000000000000000000000000000000000018FFFF0000008B68766343010200000001B0000000000096F000FCFDFAFA00000F03A00001002040010C01FFFF02A000000300B0000003000003009694903000003E900005DC05A10001003542010102A000000300B00000030000030096A003C08010E4D94526491B6BC040400000FA4000177018077BDF8000C95A000192B420A2000100084401C1625B6C1ED9");

        [TestMethod]
        public void testInOutIdent()
        {
            IsoFile isoFile = new IsoFile(new ByteBufferByteChannel(input));
            HevcConfigurationBox hevC = SharpMp4Parser.IsoParser.Tools.Path.getPath<HevcConfigurationBox>(isoFile, "hvc1/hvcC");
            Assert.IsNotNull(hevC);
            hevC.parseDetails();
            ByteStream baos = new ByteStream();
            isoFile.getBox(Channels.newChannel(baos));
            Assert.IsTrue(Enumerable.SequenceEqual(input, baos.toByteArray()));
        }
    }
}