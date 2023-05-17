using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Tests.Streaming.Input.H264;
using SharpMp4Parser.Java;
using SharpMp4Parser.IsoParser.Support;

namespace SharpMp4Parser.Tests.IsoParser
{
    /**
     * Tests ISO Roundtrip.
     */
    [TestClass]
    public class RoundTripTest
    {
        [TestMethod]
        public void testRoundTrip_TinyExamples_Old()
        {
            testRoundTrip_1("Tiny Sample - OLD.mp4");
        }

        [TestMethod]
        public void testRoundTrip_TinyExamples_Metaxed()
        {
            testRoundTrip_1("Tiny Sample - NEW - Metaxed.mp4");
        }

        [TestMethod]
        public void testRoundTrip_TinyExamples_Untouched()
        {
            testRoundTrip_1("Tiny Sample - NEW - Untouched.mp4");
        }

        [TestMethod]
        public void testRoundTrip_1a()
        {
            testRoundTrip_1("multiTrack.3gp");
        }

        [TestMethod]
        public void testRoundTrip_1b()
        {
            testRoundTrip_1("MOV00006.3gp");
        }

        [TestMethod]
        public void testRoundTrip_1c()
        {
            testRoundTrip_1("Beethoven - Bagatelle op.119 no.11 i.m4a");
        }

        [TestMethod]
        public void testRoundTrip_1d()
        {
            testRoundTrip_1("test.m4p");
        }

        [TestMethod]
        public void testRoundTrip_1e()
        {
            testRoundTrip_1("test-pod.m4a");
        }

        [TestMethod]
        public void testRoundTrip_QuickTimeFormat()
        {
            testRoundTrip_1("QuickTimeFormat.mp4");
        }

        public void testRoundTrip_1(string originalFile)
        {
            DateTime start1 = DateTime.UtcNow;
            DateTime start2 = DateTime.UtcNow;

            using (MemoryStream isoMs = new MemoryStream())
            {
                FileStream isoFis = File.OpenRead(originalFile);
                isoFis.CopyTo(isoMs);
                isoMs.Position = 0;

                var isoStream = new ByteStream(isoMs.ToArray());

                IsoFile isoFile = new IsoFile(isoStream);

                DateTime start3 = DateTime.UtcNow;

                DateTime start4 = DateTime.UtcNow;
                Walk.through(isoFile);
                DateTime start5 = DateTime.UtcNow;

                ByteStream baos = new ByteStream();
                isoFile.getBox(Channels.newChannel(baos));

                DateTime start6 = DateTime.UtcNow;

                /*   System.err.println("Preparing tmp copy took: " + (start2 - start1) + "ms");
                   System.err.println("Parsing took           : " + (start3 - start2) + "ms");
                   System.err.println("Writing took           : " + (start6 - start3) + "ms");
                   System.err.println("Walking took           : " + (start5 - start4) + "ms");*/

                IsoFile copyViaIsoFileReparsed = new IsoFile(new ByteBufferByteChannel(baos.toByteArray()));
                BoxComparator.check(isoFile, copyViaIsoFileReparsed, "moov[0]/mvhd[0]", "moov[0]/trak[0]/tkhd[0]", "moov[0]/trak[0]/mdia[0]/mdhd[0]");
                isoFile.close();
                copyViaIsoFileReparsed.close();
                // as windows cannot delete file when something is memory mapped and the garbage collector
                // doesn't necessarily free the Buffers quickly enough we cannot delete the file here (we could but only for linux)

                isoFile.close();
            }
        }
    }
}
