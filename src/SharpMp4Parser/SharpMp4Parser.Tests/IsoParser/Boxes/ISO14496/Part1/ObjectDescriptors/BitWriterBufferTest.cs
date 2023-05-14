using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System.Text;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors
{
    [TestClass]
    public class BitWriterBufferTest
    {
        public static string toString(ByteBuffer bb)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bb.limit(); i++)
            {
                int b = bb.get(i);
                b = b < 0 ? b + 256 : b;


                for (int j = 7; j >= 0; j--)
                {
                    sb.Append((b >> j & 1) == 1 ? "1" : "0");
                }

            }
            return sb.ToString();
        }

        [TestMethod]
        public void testWriteWithinBuffer()
        {
            ByteBuffer b = ByteBuffer.allocate(2);
            b.put((byte)0);
            BitWriterBuffer bwb = new BitWriterBuffer(b);
            bwb.writeBits(15, 4);
            Assert.AreEqual("0000000011110000", toString(b));

        }

        [TestMethod]
        public void testSimple()
        {
            ByteBuffer bb = ByteBuffer.allocate(4);
            BitWriterBuffer bitWriterBuffer = new BitWriterBuffer(bb);
            bitWriterBuffer.writeBits(15, 4);
            ((Java.Buffer)bb).rewind();
            int test = IsoTypeReader.readUInt8(bb);
            Assert.AreEqual(15 << 4, test);
        }

        [TestMethod]
        public void testSimpleOnByteBorder()
        {
            ByteBuffer bb = ByteBuffer.allocate(4);
            BitWriterBuffer bitWriterBuffer = new BitWriterBuffer(bb);
            bitWriterBuffer.writeBits(15, 4);
            bitWriterBuffer.writeBits(15, 4);
            bitWriterBuffer.writeBits(15, 4);
            ((Java.Buffer)bb).rewind();
            int test = IsoTypeReader.readUInt8(bb);
            Assert.AreEqual(255, test);
            test = IsoTypeReader.readUInt8(bb);
            Assert.AreEqual(15 << 4, test);
        }

        [TestMethod]
        public void testSimpleCrossByteBorder()
        {
            ByteBuffer bb = ByteBuffer.allocate(2);
            BitWriterBuffer bitWriterBuffer = new BitWriterBuffer(bb);

            bitWriterBuffer.writeBits(1, 4);
            bitWriterBuffer.writeBits(1, 5);
            bitWriterBuffer.writeBits(1, 3);

            Assert.AreEqual("0001000010010000", toString(bb));
        }

        [TestMethod]
        public void testMultiByte()
        {
            ByteBuffer bb = ByteBuffer.allocate(4);
            BitWriterBuffer bitWriterBuffer = new BitWriterBuffer(bb);
            bitWriterBuffer.writeBits(0, 1);
            bitWriterBuffer.writeBits(65535, 16);
            ((Java.Buffer)bb).rewind();
            int test = IsoTypeReader.readUInt8(bb);
            Assert.AreEqual(127, test);
            test = IsoTypeReader.readUInt8(bb);
            Assert.AreEqual(255, test);
            test = IsoTypeReader.readUInt8(bb);
            Assert.AreEqual(1 << 7, test);
        }

        [TestMethod]
        public void testPattern()
        {
            ByteBuffer bb = ByteBuffer.allocate(1);
            BitWriterBuffer bwb = new BitWriterBuffer(bb);
            bwb.writeBits(1, 1);
            bwb.writeBits(1, 2);
            bwb.writeBits(1, 3);
            bwb.writeBits(1, 2);

            Assert.AreEqual("10100101", toString(bb));
        }

        [TestMethod]
        public void testWriterReaderRoundTrip()
        {
            ByteBuffer b = ByteBuffer.allocate(3);
            BitWriterBuffer bwb = new BitWriterBuffer(b);
            bwb.writeBits(1, 1);
            bwb.writeBits(1, 2);
            bwb.writeBits(1, 3);
            bwb.writeBits(1, 4);
            bwb.writeBits(1, 5);
            bwb.writeBits(7, 6);
            ((Java.Buffer)b).rewind();

            Assert.AreEqual("101001000100001000111000", toString(b));
        }
    }
}
