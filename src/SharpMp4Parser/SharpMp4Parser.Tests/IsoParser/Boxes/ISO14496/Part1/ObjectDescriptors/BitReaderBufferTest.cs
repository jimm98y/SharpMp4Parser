using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors
{
    [TestClass]
    public class BitReaderBufferTest
    {
        ByteBuffer testSequence = ByteBuffer.wrap(new byte[] { 0xff, 0, 0xff, 0 });

        [TestMethod]
        public void readFromTheMiddle()
        {
            ByteBuffer b = ByteBuffer.wrap(new byte[] { 0, 0xff });
            b.get();
            BitReaderBuffer brb = new BitReaderBuffer(b);
            Assert.AreEqual(15, brb.readBits(4));
            Assert.AreEqual(15, brb.readBits(4));

        }

        [TestMethod]
        public void testRead_8()
        {
            BitReaderBuffer bitReaderBuffer = new BitReaderBuffer(testSequence);
            Assert.AreEqual(15, bitReaderBuffer.readBits(4));
            Assert.AreEqual(15, bitReaderBuffer.readBits(4));
            Assert.AreEqual(0, bitReaderBuffer.readBits(4));
            Assert.AreEqual(0, bitReaderBuffer.readBits(4));
        }

        [TestMethod]
        public void testReadCrossByte()
        {
            BitReaderBuffer bitReaderBuffer = new BitReaderBuffer(testSequence);
            Assert.AreEqual(31, bitReaderBuffer.readBits(5));
            Assert.AreEqual(14, bitReaderBuffer.readBits(4));
            Assert.AreEqual(0, bitReaderBuffer.readBits(3));
            Assert.AreEqual(0, bitReaderBuffer.readBits(4));
        }

        [TestMethod]
        public void testReadMultiByte()
        {
            BitReaderBuffer bitReaderBuffer = new BitReaderBuffer(testSequence);
            Assert.AreEqual(510, bitReaderBuffer.readBits(9));
        }

        [TestMethod]
        public void testReadMultiByte2()
        {
            BitReaderBuffer bitReaderBuffer = new BitReaderBuffer(testSequence);
            Assert.AreEqual(0x1fe01, bitReaderBuffer.readBits(17));
        }


        [TestMethod]
        public void testRemainingBits()
        {
            BitReaderBuffer bitReaderBuffer = new BitReaderBuffer(testSequence);
            Assert.AreEqual(32, bitReaderBuffer.remainingBits());
            int six = 6;
            bitReaderBuffer.readBits(six);
            Assert.AreEqual(32 - six, bitReaderBuffer.remainingBits());
        }
    }
}
