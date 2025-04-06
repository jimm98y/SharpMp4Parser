using SharpMp4Parser.IsoParser.Tools;

namespace SharpMp4Parser.Tests.IsoParser.Util
{
    [TestClass]
    public class RangeStartMapTest
    {
        [TestMethod]
        public void basicTest()
        {
            RangeStartMap<int, string> a = new RangeStartMap<int, string>();
            a.Add(0, "Null");
            a.Add(10, "Zehn");
            a.Add(20, null);
            a.Add(30, "Dreißig");

            Assert.AreEqual("Null", a[0]);
            Assert.AreEqual("Null", a[1]);
            Assert.AreEqual("Null", a[9]);
            Assert.AreEqual("Zehn", a[10]);
            Assert.AreEqual("Zehn", a[11]);
            Assert.AreEqual("Zehn", a[19]);
            Assert.AreEqual(null, a[20]);
            Assert.AreEqual(null, a[21]);
            Assert.AreEqual(null, a[29]);
            Assert.AreEqual("Dreißig", a[30]);
            Assert.AreEqual("Dreißig", a[31]);
            Assert.AreEqual("Dreißig", a[int.MaxValue]);
        }
    }
}
