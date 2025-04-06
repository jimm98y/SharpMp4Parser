using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part14;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.IsoParser;
using Path = SharpMp4Parser.IsoParser.Tools.Path;
using System.Diagnostics;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.MP4
{
    [TestClass]
    public class ESDescriptorBoxTest
    {
        [TestMethod]
        public void testEsDescriptor()
        {
            string esdsBytes = "0000002A6573647300000000031C000000041440150018000001F4000001F4000505131056E598060102";
            //String esdsBytes = "0000003365736473000000000380808022000200048080801440150000000006AD650006AD65058080800211B0068080800102";
            IsoFile isoFile = new IsoFile(new ByteBufferByteChannel(Hex.decodeHex(esdsBytes)));
            ESDescriptorBox esds = Path.getPath<ESDescriptorBox>(isoFile, "esds");
            Assert.IsNotNull(esds);
            Debug.WriteLine(esds.getEsDescriptor());
        }
    }
}
