using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.Tests.IsoParser
{
    [TestClass]
    public class SkippingBoxTest
    {
        [TestMethod]
        public void testBoxesHaveBeenSkipped()
        {
            FileStream fis = File.OpenRead("test.m4p");
            var isoBuff = new ByteStream(fis);

            //FileInputStream fis = new FileInputStream(PathTest.class.getProtectionDomain().getCodeSource().getLocation().getFile() + "/test.m4p");
            var isoFile = new IsoFile(isoBuff, new PropertyBoxParserImpl().skippingBoxes("mdat", "mvhd"));
            fis.Close();

            MovieBox movieBox = isoFile.getMovieBox();
            Assert.IsNotNull(movieBox);
            Assert.AreEqual(4, movieBox.getBoxes().Count);
            Assert.AreEqual("mvhd", movieBox.getBoxes()[0].getType());
            Assert.AreEqual("iods", movieBox.getBoxes()[1].getType());
            Assert.AreEqual("trak", movieBox.getBoxes()[2].getType());
            Assert.AreEqual("udta", movieBox.getBoxes()[3].getType());

            Box box = (Box)SharpMp4Parser.IsoParser.Tools.Path.getPath<TrackHeaderBox>(isoFile, "moov/trak/tkhd");
            Assert.IsInstanceOfType(box, typeof(TrackHeaderBox));

            TrackHeaderBox thb = (TrackHeaderBox)box;
            Assert.IsTrue(thb.getDuration() == 102595);

            box = (Box)SharpMp4Parser.IsoParser.Tools.Path.getPath<SkipBox>(isoFile, "mdat");
            Assert.IsInstanceOfType(box, typeof(SkipBox));

            box = (Box)SharpMp4Parser.IsoParser.Tools.Path.getPath<SkipBox>(isoFile, "moov/mvhd");
            Assert.IsInstanceOfType(box, typeof(SkipBox));
        }
    }
}
