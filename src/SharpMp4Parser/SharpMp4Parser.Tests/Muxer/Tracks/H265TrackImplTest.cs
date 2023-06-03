using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Muxer.Builder;
using SharpMp4Parser.Muxer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpMp4Parser.Java;
using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.Muxer.Tracks.H265;

namespace SharpMp4Parser.Tests.Muxer.Tracks
{
    /**
     * Simple test to make sure nothing breaks.
     */
    [TestClass]
    public class H265TrackImplTest
    {
        [TestMethod]
        public void freeze()
        {
            using (MemoryStream h265Ms = new MemoryStream())
            {
                FileStream h265Fis = File.OpenRead("hevc.h265");
                h265Fis.CopyTo(h265Ms);
                h265Ms.Position = 0;
                h265Fis.Close();

                var h265DataSource = new MemoryDataSourceImpl(h265Ms.ToArray());

                SharpMp4Parser.Muxer.Tracks.AbstractH26XTrack.BUFFER = 65535; 

                Track t = new H265TrackImpl(h265DataSource);
                Movie m = new Movie();
                m.addTrack(t);

                DefaultMp4Builder mp4Builder = new DefaultMp4Builder();
                Container c = mp4Builder.build(m);

                //var fs = File.OpenWrite("C:\\Temp\\h265-sample.mp4");
                //c.writeContainer(new ByteStream(fs));
                //fs.Close();

                FileStream resFis = File.OpenRead("h265-sample.mp4");

                var resBuff = new ByteStream(resFis);
                IsoFile isoFileReference = new IsoFile(resBuff);
                BoxComparator.check(c, isoFileReference, "moov[0]/mvhd[0]", "moov[0]/trak[0]/tkhd[0]", "moov[0]/trak[0]/mdia[0]/mdhd[0]", "moov[0]/trak[0]/mdia[0]/minf[0]/stbl[0]/stco[0]");
                resFis.Close();

            }
        }
    }
}