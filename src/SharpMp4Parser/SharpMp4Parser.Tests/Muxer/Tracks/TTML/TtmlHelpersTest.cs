#if REMOVED
using System.Reflection.Metadata;
using System.Xml.XPath;

namespace SharpMp4Parser.Tests.Muxer.Tracks.TTML
{
    /**
     * Created by sannies on 06.08.2015.
     */
    [TestClass]
    public class TtmlHelpersTest
    {
        [TestMethod]
        public void testToTime()
        {
            Assert.AreEqual(-3599000, toTime("-00:59:59.000"));
            Assert.AreEqual(3599000, toTime("00:59:59.000"));
        }

        [TestMethod]
        public void testToTimeExpression()
        {
            Assert.AreEqual("-00:59:59.009", toTimeExpression(-3599009));
            Assert.AreEqual("00:59:59.010", toTimeExpression(3599010));
        }

        [TestMethod]
        public void testDeepCopyDocument()
        {
            DocumentBuilderFactory documentBuilderFactory = DocumentBuilderFactory.newInstance();
            DocumentBuilder db = documentBuilderFactory.newDocumentBuilder();
            Document ttml = db.parse(new InputSource("/org/mp4parser/muxer/tracks/ttml/tos-chapters-en.xml"));
            //Document ttml = db.parse(new InputSource("http://localhost/mp4parser/isoparser/src/test/resources/com/googlecode/mp4parser/authoring/tracks/ttml/tos-chapters-en.xml"));
            File master = File.createTempFile("TtmlHelpersTest", "testDeepCopyDocument");
            master.delete();
            master.mkdir();

            File f = new File(master, "target");


            File targetFile = new File(f, "subs.xml");

            TtmlHelpers.deepCopyDocument(ttml, targetFile);


            Document copy = db.parse(new InputSource(targetFile.getAbsolutePath()));

            XPathFactory xPathfactory = XPathFactory.newInstance();
            XPath xpath = xPathfactory.newXPath();
            XPathExpression expr = xpath.compile("//*/@backgroundImage");
            NodeList nl = (NodeList)expr.evaluate(copy, XPathConstants.NODESET);
            for (int i = 0; i < nl.getLength(); i++)
            {
                Node backgroundImage = nl.item(i);
                URI backgroundImageUri = URI.create(backgroundImage.getNodeValue());
                File bgImg = new File(new URI(copy.getDocumentURI()).resolve(backgroundImageUri));
                Assert.assertTrue(bgImg.exists());
                Assert.assertTrue(bgImg.delete());
                bgImg.getParentFile().delete();
            }
            Assert.IsTrue(targetFile.delete());
            Assert.IsTrue(f.delete());
            Assert.IsTrue(master.delete());
        }
    }
}
#endif