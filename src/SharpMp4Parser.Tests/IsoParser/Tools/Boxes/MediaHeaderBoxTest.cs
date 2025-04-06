using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.Tests.IsoParser.Boxes;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Tools.Boxes
{
    /**
     * Created by sannies on 03.06.13.
     */
    [TestClass]
    public class MediaHeaderBoxTest : BoxWriteReadBase<MediaHeaderBox>
    {

        public override Type getBoxUnderTest()
        {
            return typeof(MediaHeaderBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, MediaHeaderBox box)
        {
            addPropsHere.Add("creationTime", new DateTime(1904, 1, 1).AddMilliseconds(1370253188000L));
            addPropsHere.Add("duration", (long)12423);
            addPropsHere.Add("language", "ger");
            addPropsHere.Add("modificationTime", new DateTime(1904, 1, 1).AddMilliseconds(1370253188000L));
            addPropsHere.Add("timescale", (long)24);
        }
    }
}
