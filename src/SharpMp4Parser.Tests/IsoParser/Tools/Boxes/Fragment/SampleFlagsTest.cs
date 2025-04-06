using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.Tests.IsoParser.Tools.Boxes.Fragment
{
    /**
     *
     */
    [TestClass]
    public class SampleFlagsTest
    {
        [TestMethod]
        public void testSimple()
        {
            long l = 0x11223344;
            SampleFlags sf = new SampleFlags(ByteBuffer.wrap(new byte[] { 0x11, 0x22, 0x33, 0x44 }));
            ByteBuffer b = ByteBuffer.allocate(4);
            sf.getContent(b);
            b.rewind();
            Assert.AreEqual(l, IsoTypeReader.readUInt32(b));
        }

        [TestMethod]
        public void testSetterGetterRoundTrip()
        {
            SampleFlags sf = new SampleFlags();
            sf.setReserved(1);
            sf.setSampleDegradationPriority(1);
            sf.setSampleDependsOn(1);
            sf.setSampleHasRedundancy(2);
            sf.setSampleIsDependedOn(3);
            sf.setSampleIsDifferenceSample(true);
            sf.setSamplePaddingValue(3);
            ByteBuffer bb = ByteBuffer.allocate(4);
            sf.getContent(bb);
            bb.rewind();
            //System.err.println(BitWriterBufferTest.toString(bb));
            SampleFlags sf2 = new SampleFlags(bb);


            Assert.AreEqual(sf.getReserved(), sf2.getReserved());
            Assert.AreEqual(sf.getSampleDependsOn(), sf2.getSampleDependsOn());
            Assert.AreEqual(sf.isSampleIsDifferenceSample(), sf2.isSampleIsDifferenceSample());
            Assert.AreEqual(sf.getSamplePaddingValue(), sf2.getSamplePaddingValue());

            Assert.AreEqual(sf.getSampleDegradationPriority(), sf2.getSampleDegradationPriority());
            Assert.AreEqual(sf.getSampleHasRedundancy(), sf2.getSampleHasRedundancy());
            Assert.AreEqual(sf.getSampleIsDependedOn(), sf2.getSampleIsDependedOn());

        }
    }
}
