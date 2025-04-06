using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.Tests.IsoParser.Boxes;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Tools.Boxes
{
    /**
     * Created by IntelliJ IDEA.
     * User: sannies
     * Date: 24.02.11
     * Time: 12:41
     * To change this template use File | Settings | File Templates.
     */
    [TestClass]
    public class SampleToChunkBoxTest : BoxWriteReadBase<SampleToChunkBox>
    {

        public override Type getBoxUnderTest()
        {
            return typeof(SampleToChunkBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, SampleToChunkBox box)
        {
            List<SampleToChunkBox.Entry> l = new List<SampleToChunkBox.Entry>();
            for (int i = 0; i < 5; i++)
            {
                SampleToChunkBox.Entry e = new SampleToChunkBox.Entry(i, 1, i * i);
                l.Add(e);
            }

            addPropsHere.Add("entries", l);
        }
    }
}