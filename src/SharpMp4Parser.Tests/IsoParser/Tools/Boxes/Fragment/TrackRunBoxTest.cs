using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Tools.Boxes.Fragment
{
    [TestClass]
    public class TrackRunBoxTest
    {
        [TestMethod]
        public void test()
        {
            testAllFlagsWithDataOffset(new SampleFlags(ByteBuffer.wrap(new byte[] { 32, 3, 65, 127 })));
            testAllFlagsWithDataOffset(null);
        }

        public void testAllFlagsWithDataOffset(SampleFlags sf)
        {
            testAllFlags(-1, sf);
            testAllFlags(1000, sf);
        }

        public void testAllFlags(int dataOffset, SampleFlags sf)
        {
            simpleRoundTrip(false, false, false, dataOffset, sf);
            simpleRoundTrip(false, false, true, dataOffset, sf);
            simpleRoundTrip(false, true, false, dataOffset, sf);
            simpleRoundTrip(false, true, true, dataOffset, sf);
            simpleRoundTrip(true, false, false, dataOffset, sf);
            simpleRoundTrip(true, false, true, dataOffset, sf);
            simpleRoundTrip(true, true, false, dataOffset, sf);
            simpleRoundTrip(true, true, true, dataOffset, sf);
        }

        public void simpleRoundTrip(bool isSampleSizePresent,
                                    bool isSampleDurationPresent,
                                    bool isSampleCompositionTimeOffsetPresent,
                                    int dataOffset, SampleFlags sf)
        {
            TrackRunBox trun = new TrackRunBox();
            trun.setFirstSampleFlags(sf);
            trun.setSampleSizePresent(!isSampleSizePresent);
            trun.setSampleSizePresent(isSampleSizePresent);
            trun.setSampleDurationPresent(!isSampleDurationPresent);
            trun.setSampleDurationPresent(isSampleDurationPresent);
            trun.setSampleCompositionTimeOffsetPresent(!isSampleCompositionTimeOffsetPresent);
            trun.setSampleCompositionTimeOffsetPresent(isSampleCompositionTimeOffsetPresent);
            trun.setDataOffset(dataOffset);
            List<TrackRunBox.Entry> entries = new List<TrackRunBox.Entry>();
            entries.Add(new TrackRunBox.Entry(1000, 2000, new SampleFlags(), 3000));
            entries.Add(new TrackRunBox.Entry(1001, 2001, new SampleFlags(), 3001));
            trun.setEntries(entries);

            ByteStream fc = new ByteStream();
            trun.getBox(fc);
            //fc.close();
            fc.position(0);


            IsoFile isoFile = new IsoFile(fc);
            TrackRunBox trun2 = (TrackRunBox)isoFile.getBoxes()[0];

            Assert.AreEqual(trun.isDataOffsetPresent(), trun2.isDataOffsetPresent());
            Assert.AreEqual(trun.isSampleCompositionTimeOffsetPresent(), trun2.isSampleCompositionTimeOffsetPresent());
            Assert.AreEqual(trun.isSampleDurationPresent(), trun2.isSampleDurationPresent());
            Assert.AreEqual(trun.isSampleFlagsPresent(), trun2.isSampleFlagsPresent());
            Assert.AreEqual(trun.isSampleSizePresent(), trun2.isSampleSizePresent());
            Assert.AreEqual(trun.getDataOffset(), trun2.getDataOffset());

            Assert.AreEqual(trun.getDataOffset(), trun2.getDataOffset());
            Assert.AreEqual(trun.getFirstSampleFlags(), trun2.getFirstSampleFlags());
        }
    }
}