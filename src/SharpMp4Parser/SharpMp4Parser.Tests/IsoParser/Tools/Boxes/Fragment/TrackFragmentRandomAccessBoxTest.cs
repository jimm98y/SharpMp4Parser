using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.Tests.IsoParser.Tools.Boxes.Fragment
{
    [TestClass]
    public class TrackFragmentRandomAccessBoxTest
    {
        [TestMethod]
        public void testRoundtrip()
        {
            testRoundtrip(1, 1, 1);
            testRoundtrip(2, 1, 1);
            testRoundtrip(4, 1, 1);
            testRoundtrip(1, 2, 1);
            testRoundtrip(2, 2, 1);
            testRoundtrip(4, 2, 1);
            testRoundtrip(1, 4, 1);
            testRoundtrip(2, 4, 1);
            testRoundtrip(4, 4, 1);

            testRoundtrip(1, 1, 2);
            testRoundtrip(2, 1, 2);
            testRoundtrip(4, 1, 2);
            testRoundtrip(1, 2, 2);
            testRoundtrip(2, 2, 2);
            testRoundtrip(4, 2, 2);
            testRoundtrip(1, 4, 2);
            testRoundtrip(2, 4, 2);
            testRoundtrip(4, 4, 2);

            testRoundtrip(1, 1, 4);
            testRoundtrip(2, 1, 4);
            testRoundtrip(4, 1, 4);
            testRoundtrip(1, 2, 4);
            testRoundtrip(2, 2, 4);
            testRoundtrip(4, 2, 4);
            testRoundtrip(1, 4, 4);
            testRoundtrip(2, 4, 4);
            testRoundtrip(4, 4, 4);
        }

        public void testRoundtrip(int sizeOfSampleNum, int lengthSizeOfTrafNum, int lengthSizeOfTrunNum)
        {
            TrackFragmentRandomAccessBox traf = new TrackFragmentRandomAccessBox();
            traf.setLengthSizeOfSampleNum(sizeOfSampleNum);
            traf.setLengthSizeOfTrafNum(lengthSizeOfTrafNum);
            traf.setLengthSizeOfTrunNum(lengthSizeOfTrunNum);
            List<TrackFragmentRandomAccessBox.Entry> entries = new List<TrackFragmentRandomAccessBox.Entry>();
            entries.Add(new TrackFragmentRandomAccessBox.Entry(1, 2, 3, 4, 5));

            traf.setEntries(entries);

            ByteStream fc = new ByteStream();
            traf.getBox(fc);
            //fc.close();
            fc.position(0);

            IsoFile isoFile = new IsoFile(fc);
            TrackFragmentRandomAccessBox traf2 = (TrackFragmentRandomAccessBox)isoFile.getBoxes()[0];
            Assert.AreEqual(traf.getNumberOfEntries(), traf2.getNumberOfEntries());
            Assert.AreEqual(traf.getReserved(), traf2.getReserved());
            Assert.AreEqual(traf.getTrackId(), traf2.getTrackId());
            //System.err.println("" + sizeOfSampleNum + " " + lengthSizeOfTrafNum + " " + lengthSizeOfTrunNum);
            Assert.IsTrue(traf.getEntries().SequenceEqual(traf2.getEntries()));
        }
    }
}