using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System.Text;

namespace SharpMp4Parser.Tests.IsoParser.Tools
{
    /**
     * Test symmetrie of IsoBufferWrapper and Iso
     */
    [TestClass]
    public class IsoTypeReaderTest
    {
        [TestMethod]
        public void testInt()
        {
            ByteBuffer bb = ByteBuffer.allocate(20);

            IsoTypeWriter.writeUInt8(bb, 0);
            IsoTypeWriter.writeUInt8(bb, 255);
            IsoTypeWriter.writeUInt16(bb, 0);
            IsoTypeWriter.writeUInt16(bb, (1 << 16) - 1);
            IsoTypeWriter.writeUInt24(bb, 0);
            IsoTypeWriter.writeUInt24(bb, (1 << 24) - 1);
            IsoTypeWriter.writeUInt32(bb, 0);
            IsoTypeWriter.writeUInt32(bb, (1 << 32) - 1);
            ((Java.Buffer)bb).rewind();

            Assert.AreEqual(0, IsoTypeReader.readUInt8(bb));
            Assert.AreEqual(255, IsoTypeReader.readUInt8(bb));
            Assert.AreEqual(0, IsoTypeReader.readUInt16(bb));
            Assert.AreEqual((1 << 16) - 1, IsoTypeReader.readUInt16(bb));
            Assert.AreEqual(0, IsoTypeReader.readUInt24(bb));
            Assert.AreEqual((1 << 24) - 1, IsoTypeReader.readUInt24(bb));
            Assert.AreEqual(0, IsoTypeReader.readUInt32(bb));
            Assert.AreEqual((1 << 32) - 1, IsoTypeReader.readUInt32(bb));
        }

        [TestMethod]
        public void testFixedPoint1616()
        {
            const double fixedPointTest1 = 10.13;
            const double fixedPointTest2 = -10.13;

            ByteBuffer bb = ByteBuffer.allocate(8);

            IsoTypeWriter.writeFixedPoint1616(bb, fixedPointTest1);
            IsoTypeWriter.writeFixedPoint1616(bb, fixedPointTest2);
            ((Java.Buffer)bb).rewind();

            Assert.AreEqual(fixedPointTest1, IsoTypeReader.readFixedPoint1616(bb), 1d / 65536, "fixedPointTest1");
            Assert.AreEqual(fixedPointTest2, IsoTypeReader.readFixedPoint1616(bb), 1d / 65536, "fixedPointTest2");
        }

        [TestMethod]
        public void testFixedPoint0230()
        {
            const double fixedPointTest1 = 1.13;
            const double fixedPointTest2 = -1.13;

            ByteBuffer bb = ByteBuffer.allocate(8);

            IsoTypeWriter.writeFixedPoint0230(bb, fixedPointTest1);
            IsoTypeWriter.writeFixedPoint0230(bb, fixedPointTest2);
            ((Java.Buffer)bb).rewind();

            Assert.AreEqual(fixedPointTest1, IsoTypeReader.readFixedPoint0230(bb), 1d / 65536, "fixedPointTest1");
            Assert.AreEqual(fixedPointTest2, IsoTypeReader.readFixedPoint0230(bb), 1d / 65536, "fixedPointTest2");
        }

        [TestMethod]
        public void testFixedPoint88()
        {
            const double fixedPointTest1 = 10.13;
            const double fixedPointTest2 = -10.13;
            ByteBuffer bb = ByteBuffer.allocate(4);

            IsoTypeWriter.writeFixedPoint88(bb, fixedPointTest1);
            IsoTypeWriter.writeFixedPoint88(bb, fixedPointTest2);
            ((Java.Buffer)bb).rewind();

            Assert.AreEqual(fixedPointTest1, IsoTypeReader.readFixedPoint88(bb), 1d / 256, "fixedPointTest1");
            Assert.AreEqual(fixedPointTest2, IsoTypeReader.readFixedPoint88(bb), 1d / 256, "fixedPointTest2");
        }

        [TestMethod]
        public void testRead4cc()
        {
            ByteBuffer bb = ByteBuffer.wrap(Encoding.UTF8.GetBytes("abcd"));
            string code = IsoTypeReader.read4cc(bb);
            Assert.AreEqual(4, bb.position());
            Assert.AreEqual("abcd", code);
        }
    }
}
