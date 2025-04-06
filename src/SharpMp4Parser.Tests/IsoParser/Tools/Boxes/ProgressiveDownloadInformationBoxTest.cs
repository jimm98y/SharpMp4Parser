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
    public class ProgressiveDownloadInformationBoxTest : BoxWriteReadBase<ProgressiveDownloadInformationBox>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(ProgressiveDownloadInformationBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, ProgressiveDownloadInformationBox box)
        {
            List<ProgressiveDownloadInformationBox.Entry> entries = new List<ProgressiveDownloadInformationBox.Entry>();
            entries.Add(new ProgressiveDownloadInformationBox.Entry(10, 20));
            entries.Add(new ProgressiveDownloadInformationBox.Entry(20, 10));
            addPropsHere.Add("entries", entries);
        }
    }
}