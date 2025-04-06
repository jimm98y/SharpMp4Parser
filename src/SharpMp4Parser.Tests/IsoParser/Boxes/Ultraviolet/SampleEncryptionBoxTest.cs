using SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.Ultraviolet
{
    [TestClass]
    public class SampleEncryptionBoxTest
    {
        SampleEncryptionBox senc = new SampleEncryptionBox();

        [TestMethod]
        public void testRoundTripFlagsZero()
        {
            List<CencSampleAuxiliaryDataFormat> entries = new List<CencSampleAuxiliaryDataFormat>();

            CencSampleAuxiliaryDataFormat entry = new CencSampleAuxiliaryDataFormat();
            entry.iv = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            entries.Add(entry);

            senc.setEntries(entries);

            ByteStream fc = new ByteStream();
            senc.getBox(fc);
            //fc.close();
            Assert.AreEqual(fc.position(), senc.getSize());
            fc.position(0);

            IsoFile iso = new IsoFile(fc);

            Assert.IsTrue(iso.getBoxes()[0] is AbstractSampleEncryptionBox);
            AbstractSampleEncryptionBox senc2 = (AbstractSampleEncryptionBox)iso.getBoxes()[0];
            Assert.AreEqual(0, senc2.getFlags());
            Assert.IsTrue(senc.Equals(senc2));
            Assert.IsTrue(senc2.Equals(senc));
        }

        [TestMethod]
        public void testRoundTripFlagsTwo()
        {
            senc.setSubSampleEncryption(true);
            List<CencSampleAuxiliaryDataFormat> entries = new List<CencSampleAuxiliaryDataFormat>();
            CencSampleAuxiliaryDataFormat entry = new CencSampleAuxiliaryDataFormat();
            entry.iv = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            entry.pairs = new CencSampleAuxiliaryDataFormat.Pair[5];
            entry.pairs[0] = entry.createPair(5, 15);
            entry.pairs[1] = entry.createPair(5, 16);
            entry.pairs[2] = entry.createPair(5, 17);
            entry.pairs[3] = entry.createPair(5, 18);
            entry.pairs[4] = entry.createPair(5, 19);
            entries.Add(entry);


            senc.setEntries(entries);

            ByteStream fc = new ByteStream();
            senc.getBox(fc);
            //fc.close();
            fc.position(0);

            IsoFile iso = new IsoFile(fc);

            Assert.IsTrue(iso.getBoxes()[0] is AbstractSampleEncryptionBox);
            AbstractSampleEncryptionBox senc2 = (AbstractSampleEncryptionBox)iso.getBoxes()[0];
            Assert.AreEqual(2, senc2.getFlags());
            Assert.IsTrue(senc.Equals(senc2));
            Assert.IsTrue(senc2.Equals(senc));

        }
    }
}