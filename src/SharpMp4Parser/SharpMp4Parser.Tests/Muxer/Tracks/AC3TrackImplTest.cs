using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Muxer.Builder;
using SharpMp4Parser.Muxer;
using SharpMp4Parser.Muxer.Tracks;
using SharpMp4Parser.Java;
using SharpMp4Parser.IsoParser.Support;

namespace SharpMp4Parser.Tests.Muxer.Tracks
{
    [TestClass]
    public class AC3TrackImplTest
    {
        [TestMethod]
        public void freeze()
        {
            using (MemoryStream ac3Ms = new MemoryStream())
            {
                FileStream ac3Fis = File.OpenRead("ac3-sample.ac3");
                ac3Fis.CopyTo(ac3Ms);
                ac3Ms.Position = 0;

                var ac3DataSource = new MemoryDataSourceImpl(ac3Ms.ToArray());

                Track t = new AC3TrackImpl(ac3DataSource);
                Movie m = new Movie();
                m.addTrack(t);

                DefaultMp4Builder mp4Builder = new DefaultMp4Builder();
                Container isoFile = mp4Builder.build(m);
                //WritableByteChannel fc = new FileOutputStream("ac3-sample.mp4").getChannel();
                //isoFile.writeContainer(fc);
                //fc.close();

                using (MemoryStream isoMs = new MemoryStream())
                {
                    FileStream isoFis = File.OpenRead("ac3-sample.mp4");
                    isoFis.CopyTo(isoMs);
                    isoMs.Position = 0;

                    var isoBuff = new ByteStream(isoMs.ToArray());

                    IsoFile isoFileReference = new IsoFile(isoBuff);
                    BoxComparator.check(isoFile, isoFileReference, "moov[0]/mvhd[0]", "moov[0]/trak[0]/tkhd[0]", "moov[0]/trak[0]/mdia[0]/mdhd[0]");

                    isoFis.Close();
                }

                ac3Fis.Close();
            }
        }
    }
}
