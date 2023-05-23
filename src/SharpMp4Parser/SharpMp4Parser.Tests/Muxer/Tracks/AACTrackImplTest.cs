using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using SharpMp4Parser.Muxer.Builder;
using SharpMp4Parser.Muxer;
using SharpMp4Parser.Muxer.Tracks;
using SharpMp4Parser.IsoParser.Support;

namespace SharpMp4Parser.Tests.Muxer.Tracks
{
    /**
     * Simple test to make sure nothing breaks.
     */
    [TestClass]
    public class AACTrackImplTest
    {
        [TestMethod]
        public void freeze()
        {
            using (MemoryStream aacMs = new MemoryStream())
            {
                FileStream aacFis = File.OpenRead("aac-sample.aac");
                aacFis.CopyTo(aacMs);
                aacMs.Position = 0;

                var aacDataSource = new MemoryDataSourceImpl(aacMs.ToArray());

                Track t = new AACTrackImpl(aacDataSource);
                //Track t = new AACTrackImpl2(new FileInputStream(this.getClass().getProtectionDomain().getCodeSource().getLocation().getFile() + "/com/googlecode/mp4parser/authoring/tracks/aac-sample.aac"));
                Movie m = new Movie();
                m.addTrack(t);

                DefaultMp4Builder mp4Builder = new DefaultMp4Builder();
                Container c = mp4Builder.build(m);
                //c.writeContainer(new FileOutputStream("C:\\dev\\mp4parser\\isoparser\\src\\test\\resources\\com\\googlecode\\mp4parser\\authoring\\tracks\\aac-sample.mp4").getChannel());

                FileStream isoFis = File.OpenRead("aac-sample.mp4");
                var isoBuff = new ByteStream(isoFis);

                IsoFile isoFileReference = new IsoFile(isoBuff);
                BoxComparator.check(c, isoFileReference, "moov[0]/mvhd[0]", "moov[0]/trak[0]/tkhd[0]", "moov[0]/trak[0]/mdia[0]/mdhd[0]", "moov[0]/trak[0]/mdia[0]/minf[0]/stbl[0]/stco[0]");

                isoFis.Close();
                aacFis.Close();
            }
        }
    }
}
