using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.Tests.IsoParser.Boxes;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Tools.Boxes
{
    /**
      *
      */
    [TestClass]
    public class SampleDependencyTypeBoxTest : BoxWriteReadBase<SampleDependencyTypeBox>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(SampleDependencyTypeBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, SampleDependencyTypeBox box)
        {
            List<SampleDependencyTypeBox.Entry> l = new List<SampleDependencyTypeBox.Entry>();
            for (int i = 0; i < 0xcf; i++)
            {
                SampleDependencyTypeBox.Entry e = new SampleDependencyTypeBox.Entry(i);
                l.Add(e);
            }
            addPropsHere.Add("entries", l);
        }
    }
}