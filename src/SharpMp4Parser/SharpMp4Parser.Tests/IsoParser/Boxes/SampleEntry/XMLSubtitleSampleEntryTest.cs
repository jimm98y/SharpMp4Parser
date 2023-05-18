using SharpMp4Parser.IsoParser;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part30;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.SampleEntry
{
    /**
     * Created by sannies on 23.05.13.
     */
    [TestClass]
    public class XMLSubtitleSampleEntryTest : BoxWriteReadBase<XMLSubtitleSampleEntry>
    {

        public override Type getBoxUnderTest()
        {
            return typeof(XMLSubtitleSampleEntry);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, XMLSubtitleSampleEntry box)
        {
            addPropsHere.Add("boxes", new List<Box>() { new FreeBox(100) });
            addPropsHere.Add("dataReferenceIndex", 12);
            addPropsHere.Add("auxiliaryMimeTypes", "image/jpeg");
            addPropsHere.Add("namespace", "urn:namespace:dunno");
            addPropsHere.Add("schemaLocation", "here");
        }
    }
}