using SharpMp4Parser.Java;
using SharpMp4Parser.Muxer.Tracks.WebVTT;
using System.Globalization;
using System.Text;

namespace SharpMp4Parser.Tests.Muxer.Tracks.WebVTT
{
    /**
     * Created by sannies on 27.08.2015.
     */
    [TestClass]
    public class WebVttTrackTest
    {
        [TestMethod]
        public void testVerySimpleCheck()
        {
            TestVtt("Tears_Of_Steel_per.vtt", "fas", ".تو یه احمقی،تام");
            TestVtt("Tears_Of_Steel_rus.vtt", "rus", "Ты придурок, Том!");
            TestVtt("Tears_Of_Steel_nld.vtt", "nld", "Je bent een eikel, Thom.");
        }

        private static void TestVtt(string fileName, string culture, string expected)
        {
            using (MemoryStream perMs = new MemoryStream())
            {
                FileStream perFis = File.OpenRead(fileName);
                perFis.CopyTo(perMs);
                perMs.Position = 0;

                var per = new ByteStream(perMs.ToArray());

                WebVttTrack t1 = new WebVttTrack(per, "test", FromISOName(culture));

                Assert.AreEqual(expected, Encoding.UTF8.GetString(t1.getSamples()[1].asByteBuffer().array().Skip(16).ToArray()));
            }
        }

        public static CultureInfo FromISOName(string name)
        {
            return CultureInfo
                .GetCultures(CultureTypes.NeutralCultures)
                .First(c => c.ThreeLetterISOLanguageName == name);
        }
    }
}