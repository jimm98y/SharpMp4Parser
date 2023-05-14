using SharpMp4Parser.Java;
using SharpMp4Parser.Streaming.Input.H264;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpMp4Parser.Tests.Streaming.Input.H264
{
    /**
      * Created by sannies on 15.08.2015.
      */
    [TestClass]
    public class NalStreamTokenizerTest
    {
        [TestMethod]
        public void testTokenize()
        {
            using (MemoryStream h264Ms = new MemoryStream())
            {
                FileStream h264Fis = File.OpenRead("tos.h264");
                h264Fis.CopyTo(h264Ms);
                h264Ms.Position = 0;

                var h264Stream = new ByteStream(h264Ms.ToArray());

                H264AnnexBTrack.NalStreamTokenizer nst = new H264AnnexBTrack.NalStreamTokenizer(h264Stream);

                byte[] nal;

                int i = 0;
                while ((nal = nst.getNext()) != null) {
                    //System.err.println(Hex.encodeHex(nal));
                    i++;
                }
                Assert.AreEqual(1019, i);
                // not much of a test but hey ... better than nothing

                h264Fis.Close();
            }
        }
    }
}
