using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using SharpMp4Parser.Muxer.Builder;
using SharpMp4Parser.Muxer.Container.MP4;
using SharpMp4Parser.Muxer.Tracks.Encryption;
using SharpMp4Parser.Muxer;
using SharpMp4Parser.IsoParser;

namespace SharpMp4Parser.Tests.Muxer.Tracks
{
    [TestClass]
    public class CencTracksImplTest
    {
        [TestMethod]
        public void testEncryptDecryptDefaultMp4()
        {
            SecretKey sk = new SecretKey(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, "AES");
            Movie m = MovieCreator.build("1365070268951.mp4");

            List<Track> encTracks = new List<Track>();
            foreach (Track track in m.getTracks())
            {
                encTracks.Add(new CencEncryptingTrackImpl(track, Uuid.randomUUID(), sk, false));
            }
            m.setTracks(encTracks);

            Mp4Builder mp4Builder = new DefaultMp4Builder();
            Container c = mp4Builder.build(m);
            ByteStream baos = new ByteStream();

            c.writeContainer(Channels.newChannel(baos));

            //c.writeContainer(new FileOutputStream("output.mp4").getChannel());

            Movie m2 = MovieCreator.build(new ByteBufferByteChannel(baos.toByteArray()), new InMemRandomAccessSourceImpl(baos.toByteArray()), "inmem");
            List<Track> decTracks = new List<Track>();
            foreach (Track track in m2.getTracks())
            {
                decTracks.Add(new CencDecryptingTrackImpl((CencEncryptedTrack)track, sk));
            }
            m2.setTracks(decTracks);
            c = mp4Builder.build(m2);

            //c.writeContainer(new FileOutputStream("output2.mp4").getChannel());


        }

        [TestMethod]
        public void testEncryptDecryptFragmentedMp4()
        {
            SecretKey sk = new SecretKey(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, "AES");
            Movie m = MovieCreator.build("1365070268951.mp4");

            List<Track> encTracks = new List<Track>();
            foreach (Track track in m.getTracks())
            {
                encTracks.Add(new CencEncryptingTrackImpl(track, Uuid.randomUUID(), sk, false));
            }
            m.setTracks(encTracks);

            Mp4Builder mp4Builder = new FragmentedMp4Builder();
            Container c = mp4Builder.build(m);
            ByteStream baos = new ByteStream();

            c.writeContainer(Channels.newChannel(baos));

            //c.writeContainer(new FileOutputStream("output.mp4").getChannel());

            Movie m2 = MovieCreator.build(new ByteBufferByteChannel(baos.toByteArray()), new InMemRandomAccessSourceImpl(baos.toByteArray()), "inmem");
            List<Track> decTracks = new List<Track>();
            foreach (Track track in m2.getTracks())
            {
                decTracks.Add(new CencDecryptingTrackImpl((CencEncryptedTrack)track, sk));
            }
            m2.setTracks(decTracks);
            c = mp4Builder.build(m2);

            //c.writeContainer(new FileOutputStream("output2.mp4").getChannel());

        }

        [TestMethod]
        public void testEncryptDecryptCbc1FragmentedMp4()
        {
            Uuid keyId = Uuid.randomUUID();
            SecretKey key = new SecretKey(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, "AES");
            Movie m = MovieCreator.build("1365070268951.mp4");

            List<Track> encTracks = new List<Track>();
            foreach (Track track in m.getTracks())
            {
                encTracks.Add(new CencEncryptingTrackImpl(track, new RangeStartMap<int, Uuid>() { { 0, keyId } }, new Dictionary<Uuid, SecretKey>() { { keyId, key } }, "cbc1", true, false));
            }
            m.setTracks(encTracks);

            Mp4Builder mp4Builder = new FragmentedMp4Builder();
            Container c = mp4Builder.build(m);
            ByteStream baos = new ByteStream();

            c.writeContainer(Channels.newChannel(baos));

            //c.writeContainer(new FileOutputStream("output.mp4").getChannel());

            Movie m2 = MovieCreator.build(new ByteBufferByteChannel(baos.toByteArray()), new InMemRandomAccessSourceImpl(baos.toByteArray()), "inmem");
            List<Track> decTracks = new List<Track>();
            foreach (Track track in m2.getTracks())
            {
                decTracks.Add(new CencDecryptingTrackImpl((CencEncryptedTrack)track, key));
            }
            m2.setTracks(decTracks);
            c = mp4Builder.build(m2);

            ByteStream output = new ByteStream();
            c.writeContainer(output);
        }
    }
}
