using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using SharpMp4Parser.Muxer.Builder;
using SharpMp4Parser.Muxer.Container.MP4;
using SharpMp4Parser.Muxer.Tracks.Encryption;
using SharpMp4Parser.Muxer;
using SharpMp4Parser.IsoParser;

namespace SharpMp4Parser.Tests.Muxer
{
    [TestClass]
    public class CencFileRoundtripTest
    {
        private Dictionary<Uuid, SecretKey> keys;
        private RangeStartMap<int, Uuid> keyRotation1;
        private RangeStartMap<int, Uuid> keyRotation2;
        private RangeStartMap<int, Uuid> keyRotation3;

        [TestInitialize]
        public void setUp()
        {
            Uuid Uuid1 = Uuid.randomUUID();
            Uuid Uuid2 = Uuid.randomUUID();

            SecretKey cek1 = new SecretKey(new byte[] { 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1 }, "AES");
            SecretKey cek2 = new SecretKey(new byte[] { 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1 }, "AES");

            keys = new Dictionary<Uuid, SecretKey>();
            keys.Add(Uuid1, cek1);
            keys.Add(Uuid2, cek2);

            keyRotation1 = new RangeStartMap<int, Uuid>();
            keyRotation1.Add(0, Uuid1);

            keyRotation2 = new RangeStartMap<int, Uuid>();
            keyRotation2.Add(0, Uuid.Empty);
            keyRotation2.Add(24, Uuid1);


            keyRotation3 = new RangeStartMap<int, Uuid>();
            keyRotation3.Add(0, Uuid.Empty);
            keyRotation3.Add(24, Uuid1);
            keyRotation3.Add(48, Uuid2);
        }

        [TestMethod]
        public void testSingleKeysStdMp4_cbc1()
        {
            testMultipleKeys(new DefaultMp4Builder(), "BBB_fixedres_B_180x320_80.mp4", keys, keyRotation1, "cbc1", false);
        }

        [TestMethod]
        public void testSingleKeysFragMp4_cbc1()
        {
            testMultipleKeys(new FragmentedMp4Builder(), "BBB_fixedres_B_180x320_80.mp4", keys, keyRotation1, "cbc1", false);
        }

        [TestMethod]
        public void testSingleKeysStdMp4_cenc()
        {
            testMultipleKeys(new DefaultMp4Builder(), "BBB_fixedres_B_180x320_80.mp4", keys, keyRotation1, "cenc", false);
        }

        [TestMethod]
        public void testSingleKeysFragMp4_cenc()
        {
            testMultipleKeys(new FragmentedMp4Builder(), "BBB_fixedres_B_180x320_80.mp4", keys, keyRotation1, "cenc", false);
        }

        [TestMethod]
        public void testClearLeadStdMp4_2_cbc1()
        {
            testMultipleKeys(new DefaultMp4Builder(), "BBB_fixedres_B_180x320_80.mp4", keys, keyRotation2, "cbc1", false);
        }

        [TestMethod]
        public void testClearLeadFragMp4_2_cbc1()
        {
            testMultipleKeys(new FragmentedMp4Builder(), "BBB_fixedres_B_180x320_80.mp4", keys, keyRotation2, "cbc1", false);
        }

        [TestMethod]
        public void testClearLeadStdMp4_2_cenc()
        {
            testMultipleKeys(new DefaultMp4Builder(), "BBB_fixedres_B_180x320_80.mp4", keys, keyRotation2, "cenc", false);
        }

        [TestMethod]
        public void testClearLeadFragMp4_2_cenc()
        {
            testMultipleKeys(new FragmentedMp4Builder(), "BBB_fixedres_B_180x320_80.mp4", keys, keyRotation2, "cenc", false);
        }

        [TestMethod]
        public void testMultipleKeysStdMp4_2_cbc1()
        {
            testMultipleKeys(new DefaultMp4Builder(), "BBB_fixedres_B_180x320_80.mp4", keys, keyRotation3, "cbc1", false);
        }

        [TestMethod]
        public void testMultipleKeysFragMp4_2_cbc1()
        {
            testMultipleKeys(new FragmentedMp4Builder(), "BBB_fixedres_B_180x320_80.mp4", keys, keyRotation3, "cbc1", false);
        }

        [TestMethod]
        public void testMultipleKeysStdMp4_2_cenc()
        {
            testMultipleKeys(new DefaultMp4Builder(), "BBB_fixedres_B_180x320_80.mp4", keys, keyRotation3, "cenc", false);
        }

        [TestMethod]
        public void testMultipleKeysFragMp4_2_cenc()
        {
            testMultipleKeys(new FragmentedMp4Builder(), "BBB_fixedres_B_180x320_80.mp4", keys, keyRotation3, "cenc", false);
        }

        [TestMethod]
        public void testMultipleKeysFragMp4_2_cenc_pseudo_encrypted()
        {
            testMultipleKeys(new FragmentedMp4Builder(), "BBB_fixedres_B_180x320_80.mp4", keys, keyRotation2, "cenc", true);
        }

        private void testMultipleKeys(Mp4Builder builder, string testFile, Dictionary<Uuid, SecretKey> keys,
                                      RangeStartMap<int, Uuid> keyRotation,
                                      string encAlgo, bool encryptButClear)
        {
            Movie m1 = MovieCreator.build(testFile);
            Movie m2 = new Movie();
            foreach (Track track in m1.getTracks())
            {
                CencEncryptingTrackImpl cencEncryptingTrack =
                        new CencEncryptingTrackImpl(track, keyRotation, keys, encAlgo, false, encryptButClear);
                m2.addTrack(cencEncryptingTrack);
            }
            Container c = builder.build(m2);

            ByteStream baos = new ByteStream();
            c.writeContainer(Channels.newChannel(baos));
            new ByteStream().write(baos.toByteArray()); // write only in memory as we don't have file access yet

            Movie m3 = MovieCreator.build(new ByteBufferByteChannel(baos.toByteArray()), "inmem");

            Movie m4 = new Movie();
            foreach (Track track in m3.getTracks())
            {
                CencDecryptingTrackImpl cencDecryptingTrack =
                        new CencDecryptingTrackImpl((CencEncryptedTrack)track, keys);
                m4.addTrack(cencDecryptingTrack);
            }
            Container c2 = builder.build(m4);

            ByteStream baos2 = new ByteStream();
            c2.writeContainer(Channels.newChannel(baos2));
            Movie m5 = MovieCreator.build(new ByteBufferByteChannel(baos2.toByteArray()), "inmem");

            List<Track>.Enumerator tracksPlainIter = m1.getTracks().GetEnumerator();
            List<Track>.Enumerator roundTrippedTracksIter = m5.getTracks().GetEnumerator();

            while (tracksPlainIter.MoveNext() && roundTrippedTracksIter.MoveNext())
            {
                verifySampleEquality(
                        tracksPlainIter.Current.getSamples(),
                        roundTrippedTracksIter.Current.getSamples());
            }
        }

        public void verifySampleEquality(IList<Sample> orig, IList<Sample> roundtripped)
        {
            int i = 0;
            IEnumerator<Sample> origIter = orig.GetEnumerator();
            IEnumerator<Sample> roundTrippedIter = roundtripped.GetEnumerator();
            
            while (origIter.MoveNext() && roundTrippedIter.MoveNext())
            {
                ByteStream baos1 = new ByteStream();
                ByteStream baos2 = new ByteStream();
                origIter.Current.writeTo(Channels.newChannel(baos1));
                roundTrippedIter.Current.writeTo(Channels.newChannel(baos2));
                Assert.IsTrue(Enumerable.SequenceEqual(baos1.toByteArray(), baos2.toByteArray()), "Sample " + i + " differs");
                i++;
            }

        }
    }
}