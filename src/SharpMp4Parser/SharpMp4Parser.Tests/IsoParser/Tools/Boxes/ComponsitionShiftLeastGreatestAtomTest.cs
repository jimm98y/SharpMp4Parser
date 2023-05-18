using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.Tests.IsoParser.Tools.Boxes
{
    /**
     * Created by IntelliJ IDEA.
     * User: sannies
     * Date: 24.02.11
     * Time: 12:41
     * To change this template use File | Settings | File Templates.
     */
    [TestClass]
    public class ComponsitionShiftLeastGreatestAtomTest
    {
        [TestMethod]
        public void testParse()
        {
            CompositionToDecodeBox clsg = new CompositionToDecodeBox();
            clsg.setCompositionOffsetToDisplayOffsetShift(2);
            clsg.setDisplayEndTime(3);
            clsg.setDisplayStartTime(4);
            clsg.setGreatestDisplayOffset(-2);
            clsg.setLeastDisplayOffset(-4);


            ByteStream baos = new ByteStream();
            clsg.getBox(Channels.newChannel(baos));
            IsoFile isoFile = new IsoFile(new ByteBufferByteChannel(baos.toByteArray()));

            CompositionToDecodeBox clsg2 = isoFile.getBoxes<CompositionToDecodeBox>(typeof(CompositionToDecodeBox))[0];
            Assert.AreEqual(baos.toByteArray().Length, clsg2.getSize());
            Assert.AreEqual(clsg.getCompositionOffsetToDisplayOffsetShift(), clsg2.getCompositionOffsetToDisplayOffsetShift());
            Assert.AreEqual(clsg.getGreatestDisplayOffset(), clsg2.getGreatestDisplayOffset());
            Assert.AreEqual(clsg.getDisplayEndTime(), clsg2.getDisplayEndTime());
            Assert.AreEqual(clsg.getDisplayStartTime(), clsg2.getDisplayStartTime());
            Assert.AreEqual(clsg.getLeastDisplayOffset(), clsg2.getLeastDisplayOffset());
        }
    }
}