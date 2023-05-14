using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.Muxer.Tracks;

namespace SharpMp4Parser.Tests.Muxer.Tracks
{
    /**
     * Created with IntelliJ IDEA.
     * User: sannies
     * Date: 10/28/12
     * Time: 1:33 PM
     * To change this template use File | Settings | File Templates.
     */
    [TestClass]
    public class ClippedTrackTest
    {
        [TestMethod]
        public void testGetDecodingTimeEntries()
        {
            List<TimeToSampleBox.Entry> e = new List<TimeToSampleBox.Entry>();
            e.Add(new TimeToSampleBox.Entry(2, 3));
            e.Add(new TimeToSampleBox.Entry(3, 4));
            e.Add(new TimeToSampleBox.Entry(3, 5));
            e.Add(new TimeToSampleBox.Entry(2, 6));
            List<TimeToSampleBox.Entry> r = ClippedTrack.getDecodingTimeEntries(e, 0, 1);
            Assert.AreEqual(1, r.Count);
            Assert.AreEqual(1, r[0].getCount());
            Assert.AreEqual(3, r[0].getDelta());
            r = ClippedTrack.getDecodingTimeEntries(e, 0, 2);
            Assert.AreEqual(1, r.Count);
            Assert.AreEqual(2, r[0].getCount());
            Assert.AreEqual(3, r[0].getDelta());
            r = ClippedTrack.getDecodingTimeEntries(e, 1, 2);
            Assert.AreEqual(1, r.Count);
            Assert.AreEqual(1, r[0].getCount());
            Assert.AreEqual(3, r[0].getDelta());
            r = ClippedTrack.getDecodingTimeEntries(e, 1, 3);
            Assert.AreEqual(2, r.Count);
            Assert.AreEqual(1, r[0].getCount());
            Assert.AreEqual(3, r[0].getDelta());
            Assert.AreEqual(1, r[1].getCount());
            Assert.AreEqual(4, r[1].getDelta());
            r = ClippedTrack.getDecodingTimeEntries(e, 3, 4);
            Assert.AreEqual(1, r.Count);
            Assert.AreEqual(1, r[0].getCount());
            Assert.AreEqual(4, r[0].getDelta());
            r = ClippedTrack.getDecodingTimeEntries(e, 1, 6);
            Assert.AreEqual(3, r.Count);
            Assert.AreEqual(1, r[0].getCount());
            Assert.AreEqual(3, r[0].getDelta());
            Assert.AreEqual(3, r[1].getCount());
            Assert.AreEqual(4, r[1].getDelta());
            Assert.AreEqual(1, r[2].getCount());
            Assert.AreEqual(5, r[2].getDelta());
            r = ClippedTrack.getDecodingTimeEntries(e, 2, 6);
            Assert.AreEqual(2, r.Count);
            Assert.AreEqual(3, r[0].getCount());
            Assert.AreEqual(4, r[0].getDelta());
            Assert.AreEqual(1, r[1].getCount());
            Assert.AreEqual(5, r[1].getDelta());
            r = ClippedTrack.getDecodingTimeEntries(e, 2, 8);
            Assert.AreEqual(2, r.Count);
            Assert.AreEqual(3, r[0].getCount());
            Assert.AreEqual(4, r[0].getDelta());
            Assert.AreEqual(3, r[1].getCount());
            Assert.AreEqual(5, r[1].getDelta());

        }

        [TestMethod]
        public void testGetCompositionTimes()
        {
            List<CompositionTimeToSample.Entry> e = new List<CompositionTimeToSample.Entry>();
            e.Add(new CompositionTimeToSample.Entry(2, 3));
            e.Add(new CompositionTimeToSample.Entry(3, 4));
            e.Add(new CompositionTimeToSample.Entry(3, 5));
            e.Add(new CompositionTimeToSample.Entry(2, 6));
            List<CompositionTimeToSample.Entry> r = ClippedTrack.getCompositionTimeEntries(e, 0, 1);
            Assert.AreEqual(1, r.Count);
            Assert.AreEqual(1, r[0].getCount());
            Assert.AreEqual(3, r[0].getOffset());
            r = ClippedTrack.getCompositionTimeEntries(e, 0, 2);
            Assert.AreEqual(1, r.Count);
            Assert.AreEqual(2, r[0].getCount());
            Assert.AreEqual(3, r[0].getOffset());
            r = ClippedTrack.getCompositionTimeEntries(e, 1, 2);
            Assert.AreEqual(1, r.Count);
            Assert.AreEqual(1, r[0].getCount());
            Assert.AreEqual(3, r[0].getOffset());
            r = ClippedTrack.getCompositionTimeEntries(e, 1, 3);
            Assert.AreEqual(2, r.Count);
            Assert.AreEqual(1, r[0].getCount());
            Assert.AreEqual(3, r[0].getOffset());
            Assert.AreEqual(1, r[1].getCount());
            Assert.AreEqual(4, r[1].getOffset());
            r = ClippedTrack.getCompositionTimeEntries(e, 3, 4);
            Assert.AreEqual(1, r.Count);
            Assert.AreEqual(1, r[0].getCount());
            Assert.AreEqual(4, r[0].getOffset());
            r = ClippedTrack.getCompositionTimeEntries(e, 1, 6);
            Assert.AreEqual(3, r.Count);
            Assert.AreEqual(1, r[0].getCount());
            Assert.AreEqual(3, r[0].getOffset());
            Assert.AreEqual(3, r[1].getCount());
            Assert.AreEqual(4, r[1].getOffset());
            Assert.AreEqual(1, r[2].getCount());
            Assert.AreEqual(5, r[2].getOffset());
            r = ClippedTrack.getCompositionTimeEntries(e, 2, 6);
            Assert.AreEqual(2, r.Count);
            Assert.AreEqual(3, r[0].getCount());
            Assert.AreEqual(4, r[0].getOffset());
            Assert.AreEqual(1, r[1].getCount());
            Assert.AreEqual(5, r[1].getOffset());
            r = ClippedTrack.getCompositionTimeEntries(e, 2, 8);
            Assert.AreEqual(2, r.Count);
            Assert.AreEqual(3, r[0].getCount());
            Assert.AreEqual(4, r[0].getOffset());
            Assert.AreEqual(3, r[1].getCount());
            Assert.AreEqual(5, r[1].getOffset());
        }
    }
}