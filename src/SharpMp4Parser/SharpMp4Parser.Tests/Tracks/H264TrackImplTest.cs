using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Muxer.Builder;
using SharpMp4Parser.Muxer.Tracks.H264;
using SharpMp4Parser.Muxer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpMp4Parser.Java;
using SharpMp4Parser.IsoParser.Support;

namespace SharpMp4Parser.Tests.Tracks
{
    /**
     * Simple test to make sure nothing breaks.
     */
    [TestClass]
    public class H264TrackImplTest
    {
        [TestMethod]
        public void freeze()
        {
            using (MemoryStream h264Ms = new MemoryStream())
            {
                FileStream h264Fis = File.OpenRead("h264-sample.h264");
                h264Fis.CopyTo(h264Ms);
                h264Ms.Position = 0;

                var h264DataSource = new MemoryDataSourceImpl(h264Ms.ToArray());

                H264TrackImpl.BUFFER = 65535; // make sure we are not just in one buffer
                Track t = new H264TrackImpl(h264DataSource);
                Movie m = new Movie();
                m.addTrack(t);

                DefaultMp4Builder mp4Builder = new DefaultMp4Builder();
                Container c = mp4Builder.build(m);

                // c.writeContainer(new FileOutputStream("/Users/sannies/dev/mp4parser/muxer/src/test/resources/org/mp4parser/muxer/tracks/h264-sample.mp4").getChannel());

                using (MemoryStream resMs = new MemoryStream())
                {
                    FileStream resFis = File.OpenRead("h264-sample.mp4");
                    resFis.CopyTo(resMs);
                    resMs.Position = 0;

                    var resBuff = new ReadableByteChannel(resMs.ToArray());
                    IsoFile isoFileReference = new IsoFile(resBuff);
                    BoxComparator.check(c, isoFileReference, "moov[0]/mvhd[0]", "moov[0]/trak[0]/tkhd[0]", "moov[0]/trak[0]/mdia[0]/mdhd[0]", "moov[0]/trak[0]/mdia[0]/minf[0]/stbl[0]/stco[0]");
                    resFis.Close();
                }

                h264Fis.Close();
            }
        }
    }
}