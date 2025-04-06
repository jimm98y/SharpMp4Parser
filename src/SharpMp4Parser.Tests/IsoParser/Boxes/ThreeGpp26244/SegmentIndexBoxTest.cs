using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.ThreeGpp26244
{
    [TestClass]
    public class SegmentIndexBoxTest : BoxWriteReadBase<SegmentIndexBox>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(SegmentIndexBox);
        }

        public override void setupProperties(Dictionary<String, Object> values, SegmentIndexBox box)
        {
            values.Add("referenceId", 726L);
            values.Add("timeScale", 725L);
            values.Add("earliestPresentationTime", 724L);
            values.Add("firstOffset", 34567L);
            values.Add("reserved", 0);
            values.Add("entries", new List<SegmentIndexBox.Entry>() { new SegmentIndexBox.Entry((byte)1, 2, 3, true, (byte)5, 6) });
        }
    }
}
