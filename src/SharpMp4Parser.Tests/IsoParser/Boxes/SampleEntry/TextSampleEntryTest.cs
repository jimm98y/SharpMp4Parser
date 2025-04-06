using SharpMp4Parser.IsoParser;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.SampleEntry
{
    [TestClass]
    public class TextSampleEntryTest : BoxWriteReadBase<TextSampleEntry>
    {

        [TestMethod]
        public void testBitSetters()
        {
            TextSampleEntry tx3g = new TextSampleEntry();
            tx3g.setContinuousKaraoke(true);
            Assert.IsTrue(tx3g.isContinuousKaraoke());
            tx3g.setContinuousKaraoke(false);
            Assert.IsFalse(tx3g.isContinuousKaraoke());
        }

        public override Type getBoxUnderTest()
        {
            return typeof(TextSampleEntry);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, TextSampleEntry box)
        {
            addPropsHere.Add("backgroundColorRgba", new int[] { 1, 2, 3, 4 });
            addPropsHere.Add("boxRecord", new TextSampleEntry.BoxRecord(31, 41, 51, 61));
            addPropsHere.Add("boxes", new List<Box>() { new FreeBox(100) });
            //addPropsHere.Add("continuousKaraoke", true); // not a field in C#
            addPropsHere.Add("dataReferenceIndex", 4);
            //addPropsHere.Add("fillTextRegion", true); // not a field in C#
            addPropsHere.Add("horizontalJustification", 20);
            //addPropsHere.Add("scrollDirection", false); // not a field in C#
            //addPropsHere.Add("scrollIn", false);
            //addPropsHere.Add("scrollOut", true);
            addPropsHere.Add("styleRecord", new TextSampleEntry.StyleRecord(7, 8, 9, 10, 11, new byte[] { 0xfe, 0xfd, 0xfc, 0xfb }));
            addPropsHere.Add("verticalJustification", 43);
            //addPropsHere.Add("writeTextVertically", true);
        }
    }
}
