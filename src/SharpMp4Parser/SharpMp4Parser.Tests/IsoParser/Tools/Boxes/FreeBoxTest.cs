using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.Tests.IsoParser.Tools.Boxes
{
    [TestClass]
    public class FreeBoxTest
    {
        [TestMethod]
        public void testInOutNoChange()
        {
            ByteStream baos = new ByteStream();
            FreeBox fb = new FreeBox(1000);
            ByteBuffer data = fb.getData();
            ((Java.Buffer)data).rewind();
            data.put(new byte[] { 1, 2, 3, 4, 5, 6 });
            fb.getBox(Channels.newChannel(baos));
            Assert.AreEqual(baos.toByteArray()[8], 1);
            Assert.AreEqual(baos.toByteArray()[9], 2);
            Assert.AreEqual(baos.toByteArray()[10], 3);
            Assert.AreEqual(baos.toByteArray()[11], 4);
        }

        [TestMethod]
        public void tesAddAndReplace()
        {
            FreeBox fb = new FreeBox(1000);
            long startSize = fb.getSize();
            ByteBuffer data = fb.getData();
            ((Java.Buffer)data).position(994);
            data.put(new byte[] { 1, 2, 3, 4, 5, 6 });
            FreeSpaceBox fsb = new FreeSpaceBox();
            fsb.setData(new byte[100]);
            fb.addAndReplace(fsb);
            ByteStream fc = new ByteStream();
            fb.getBox(fc);
            //fc.close();
            fc.position(0);

            IsoFile isoFile = new IsoFile(fc);
            Assert.AreEqual(2, isoFile.getBoxes().Count);
            Assert.AreEqual(FreeSpaceBox.TYPE, isoFile.getBoxes()[0].getType());
            Assert.AreEqual(FreeBox.TYPE, isoFile.getBoxes()[1].getType());
            Assert.AreEqual(startSize, isoFile.getBoxes()[0].getSize() + isoFile.getBoxes()[1].getSize());
        }
    }
}