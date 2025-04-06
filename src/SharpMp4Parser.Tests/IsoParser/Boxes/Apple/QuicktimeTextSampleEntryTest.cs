using SharpMp4Parser.IsoParser.Boxes.Apple;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.Apple
{
    [TestClass]
    public class QuicktimeTextSampleEntryTest : BoxWriteReadBase<QuicktimeTextSampleEntry>
    {
        public QuicktimeTextSampleEntryTest() : base("stsd")
        {

        }

        public override Type getBoxUnderTest()
        {
            return typeof(QuicktimeTextSampleEntry);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, QuicktimeTextSampleEntry box)
        {
            addPropsHere.Add("backgroundB", 5);
            addPropsHere.Add("backgroundG", 10);
            addPropsHere.Add("backgroundR", 15);
            addPropsHere.Add("dataReferenceIndex", 1);
            addPropsHere.Add("defaultTextBox", 54634562222L);
            addPropsHere.Add("displayFlags", 324);
            addPropsHere.Add("reserved1", (long)0);
            addPropsHere.Add("textJustification", 1);
            addPropsHere.Add("fontFace", (short)0);
            addPropsHere.Add("fontName", "45uku");
            addPropsHere.Add("fontNumber", (short)0);
            addPropsHere.Add("foregroundB", 115);
            addPropsHere.Add("foregroundG", 120);
            addPropsHere.Add("foregroundR", 125);
            addPropsHere.Add("reserved2", (byte)0);
            addPropsHere.Add("reserved3", (short)0);
        }
    }
}