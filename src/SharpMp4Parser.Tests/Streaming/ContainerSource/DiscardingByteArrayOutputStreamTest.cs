using SharpMp4Parser.Streaming.Input.MP4;
using System.Linq;

namespace SharpMp4Parser.Tests.Streaming.ContainerSource
{
    /**
     * Created by sannies on 05.08.2015.
     */
    [TestClass]
    public class DiscardingByteArrayOutputStreamTest
    {

        [TestMethod]
        public void testSimple()
        {
            DiscardingByteArrayOutputStream dbaos = new DiscardingByteArrayOutputStream();
            dbaos.write(0);
            dbaos.write(1);
            dbaos.write(2);
            dbaos.write(3);
            dbaos.write(4);
            dbaos.write(5);
            dbaos.write(6);
            dbaos.write(7);
            byte[] b = dbaos.get(3, 3);
            Assert.IsTrue(Enumerable.SequenceEqual(new byte[] { 3, 4, 5 }, b));
            dbaos.discardTo(3);
            b = dbaos.get(3, 3);
            Assert.IsTrue(Enumerable.SequenceEqual(new byte[] { 3, 4, 5 }, b));
            dbaos.discardTo(3);
            b = dbaos.get(3, 3);
            Assert.IsTrue(Enumerable.SequenceEqual(new byte[] { 3, 4, 5 }, b));

            dbaos.discardTo(4);
            b = dbaos.get(4, 3);
            Assert.IsTrue(Enumerable.SequenceEqual(new byte[] { 4, 5, 6 }, b));

        }
    }
}