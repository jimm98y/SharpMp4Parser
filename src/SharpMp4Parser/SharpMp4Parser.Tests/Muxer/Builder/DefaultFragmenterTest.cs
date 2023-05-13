using SharpMp4Parser.Muxer.Builder;
using SharpMp4Parser.Muxer.Container.MP4;
using SharpMp4Parser.Muxer;
using System.Diagnostics;

namespace SharpMp4Parser.Tests.Muxer.Builder
{
    /**
     * Just check it works.
     */
    [TestClass]
    public class DefaultFragmenterTest
    {
        long[] samples = new long[] { 1, 87, 174, 261, 348, 435, 522, 609, 696, 783, 870, 957, 1044, 1131, 1218, 1305, 1392, 1479, 1566, 1653, 1740, 1827, 1914, 2001, 2088, 2175, 2262, 2349, 2436, 2523, 2610, 2697, 2784, 2871, 2958, 3045, 3132, 3219, 3306, 3393, 3480, 3567, 3654, 3741, 3828, 3915, 4002, 4089, 4176, 4263, 4350, 4437, 4524, 4611, 4698, 4785 };

        [TestMethod]
        public void testSampleNumbers()
        {
            string f = "Beethoven - Bagatelle op.119 no.11 i.m4a";
            Movie m = MovieCreator.build(f);
            DefaultFragmenterImpl intersectionFinder = new DefaultFragmenterImpl(2);
            long[] s = intersectionFinder.sampleNumbers(m.getTracks()[0]);
            string sss = "";
            foreach (long l in s)
            {
                sss += l + ", ";
            }
            Debug.WriteLine(sss);
            Assert.IsTrue(samples.SequenceEqual(s));
        }
    }
}