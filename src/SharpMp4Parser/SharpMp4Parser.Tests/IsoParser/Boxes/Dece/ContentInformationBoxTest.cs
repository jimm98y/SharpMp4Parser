using SharpMp4Parser.IsoParser.Boxes.Dece;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.Dece
{
    [TestClass]
    public class ContentInformationBoxTest : BoxRoundtripTest
    {
        [TestMethod]
        public void roundtrip()
        {
            Dictionary<String, String> aBrandEntries = new Dictionary<String, String>();
            aBrandEntries.Add("abcd", "561326");

            Dictionary<String, String> aIdEntries = new Dictionary<String, String>();
            aIdEntries.Add("urn:dece:dece:asset_id", "urn:dece:apid:org:dunno:1234");
            base.roundtrip(new ContentInformationBox(),
                        new KeyValuePair<string, object>[]{
                                new KeyValuePair<string,object>("mimeSubtypeName", "urn:dece:apid:org:castlabs:abc"),
                                new KeyValuePair<string,object>("profileLevelIdc", "stringding"),
                                new KeyValuePair<string,object>("codecs", "avc1.21.2121, mp4a"),
                                new KeyValuePair<string,object>("protection", "none, cenc"),
                                new KeyValuePair<string,object>("languages", "fr-FR, fr-CA"),
                                //new KeyValuePair<string,object>("profileLevelIdc", "urn:dece:abc"),
                                //new KeyValuePair<string,object>("brandEntries", aBrandEntries),
                                //new KeyValuePair<string,object>("idEntries", aIdEntries)
                        });

            base.roundtrip(new ContentInformationBox(),
                        new KeyValuePair<string, object>[]{
                                new KeyValuePair<string, object>("mimeSubtypeName", "urn:dece:apid:org:castlabs:abc"),
                                new KeyValuePair<string, object>("profileLevelIdc", "stringding"),
                                new KeyValuePair<string, object>("codecs", "avc1.21.2121, mp4a"),
                                new KeyValuePair<string, object>("protection", "none, cenc"),
                                new KeyValuePair<string, object>("languages", "fr-FR, fr-CA"),
                                //new KeyValuePair<string, object>("profileLevelIdc", "urn:dece:abc"),
                                //new KeyValuePair<string, object>("brandEntries", new Dictionary<String, String>()),
                                //new KeyValuePair<string, object>("idEntries", new Dictionary<String, String>())
                        });
        }
    }
}