using SharpMp4Parser.Muxer.Builder;
using SharpMp4Parser.Muxer.Tracks;
using SharpMp4Parser.Muxer;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using SharpMp4Parser.IsoParser.Support;
using System.IO;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.Muxer.Tracks
{
    [TestClass]
    public class DTSTrackImplTest
    {
        [TestMethod]
        public void checkOutputIsStable()
        {
            using (MemoryStream dtsMs = new MemoryStream())
            {
                FileStream dtsFis = File.OpenRead("dts-sample.dtshd");
                dtsFis.CopyTo(dtsMs);
                dtsMs.Position = 0;

                var dtsDataSource = new MemoryDataSourceImpl(dtsMs.ToArray());

                Movie m = new Movie();
                DTSTrackImpl dts = new DTSTrackImpl(dtsDataSource);
                m.addTrack(dts);
                Fragmenter fif = new StaticFragmentIntersectionFinderImpl(new Dictionary<Track, long[]>() { { (Track)dts, new long[] { 1 } } });
                DefaultMp4Builder mp4Builder = new DefaultMp4Builder();
                mp4Builder.setFragmenter(fif);
                Container c = mp4Builder.build(m);

                // c.writeContainer(new FileOutputStream("C:\\dev\\mp4parser\\isoparser\\src\\test\\resources\\com\\googlecode\\mp4parser\\authoring\\tracks\\dts-sample.mp4").getChannel());
                ByteStream baos = new ByteStream();
                c.writeContainer(Channels.newChannel(baos));

                FileStream resFis = File.OpenRead("dts-sample.mp4");

                var resBuff = new ByteStream(resFis);
                IsoFile rf = new IsoFile(resBuff);
                BoxComparator.check(rf, c, "moov[0]/mvhd[0]", "moov[0]/trak[0]/tkhd[0]", "moov[0]/trak[0]/mdia[0]/mdhd[0]");

                dtsFis.Close();
            }
        }
    }
}
