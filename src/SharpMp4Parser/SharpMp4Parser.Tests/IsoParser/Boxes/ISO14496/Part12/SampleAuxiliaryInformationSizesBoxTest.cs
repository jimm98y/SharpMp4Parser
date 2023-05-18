using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.ISO14496.Part12
{
    [TestClass]
    public class SampleAuxiliaryInformationSizesBoxTest
    {
        [TestMethod]
        public void roundTripFlags0()
        {
            SampleAuxiliaryInformationSizesBox saiz1 = new SampleAuxiliaryInformationSizesBox();
            short[] ss = new short[] { 1, 11, 10, 100 };
            saiz1.setSampleInfoSizes(ss);
            ByteStream fc = new ByteStream();
            saiz1.getBox(fc);
            //fc.close();
            fc.position(0);

            IsoFile isoFile = new IsoFile(fc);
            SampleAuxiliaryInformationSizesBox saiz2 = (SampleAuxiliaryInformationSizesBox)isoFile.getBoxes()[0];

            Assert.AreEqual(saiz1.getDefaultSampleInfoSize(), saiz2.getDefaultSampleInfoSize());
            Assert.IsTrue(Enumerable.SequenceEqual(saiz1.getSampleInfoSizes(), saiz2.getSampleInfoSizes()));
        }

        [TestMethod]
        public void roundTripFlags1()
        {
            SampleAuxiliaryInformationSizesBox saiz1 = new SampleAuxiliaryInformationSizesBox();
            saiz1.setFlags(1);
            saiz1.setAuxInfoType("piff");
            saiz1.setAuxInfoTypeParameter("trak");
            short[] ss = new short[] { 1, 11, 10, 100 };
            saiz1.setSampleInfoSizes(ss);
            ByteStream fc = new ByteStream();
            saiz1.getBox(fc);
            //fc.close();
            fc.position(0);

            IsoFile isoFile = new IsoFile(fc);
            SampleAuxiliaryInformationSizesBox saiz2 = (SampleAuxiliaryInformationSizesBox)isoFile.getBoxes()[0];

            Assert.AreEqual(saiz1.getDefaultSampleInfoSize(), saiz2.getDefaultSampleInfoSize());
            Assert.IsTrue(Enumerable.SequenceEqual(saiz1.getSampleInfoSizes(), saiz2.getSampleInfoSizes()));
            Assert.AreEqual(saiz1.getAuxInfoType(), saiz2.getAuxInfoType());
            Assert.AreEqual(saiz1.getAuxInfoTypeParameter(), saiz2.getAuxInfoTypeParameter());
        }
    }
}