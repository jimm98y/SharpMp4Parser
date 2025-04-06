using SharpMp4Parser.Muxer.Builder;
using SharpMp4Parser.Muxer.Container.MP4;
using SharpMp4Parser.Muxer;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.Tests.Muxer
{
    /**
     * Not really a test but at least makes sure muxing kind of works
     */
    [TestClass]
    public class FragmentedMp4BuilderTest
    {
        [TestMethod]
        public void testSimpleMuxing()
        {
            Movie m = new Movie();
            Movie v = MovieCreator.build("BBB_fixedres_B_180x320_80.mp4");
            Movie a = MovieCreator.build("output_audio-2ch-20s.mp4");

            m.addTrack(v.getTracks()[0]);
            m.addTrack(a.getTracks()[0]);

            FragmentedMp4Builder fragmentedMp4Builder = new FragmentedMp4Builder();
            fragmentedMp4Builder.setFragmenter(new DefaultFragmenterImpl(5));

            Container c = fragmentedMp4Builder.build(m);
            c.writeContainer(Channels.newChannel(new ByteStream()));
        }
    }
}