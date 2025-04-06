using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.Tests.IsoParser.Boxes;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Tools.Boxes
{
    [TestClass]
    public class TrackHeaderBoxTest : BoxWriteReadBase<TrackHeaderBox>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(TrackHeaderBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, TrackHeaderBox box)
        {
            addPropsHere.Add("alternateGroup", (int)2);
            addPropsHere.Add("creationTime", new DateTime(1907, 1, 1).AddMilliseconds(1369296286000L));
            addPropsHere.Add("duration", (long)423);
            //addPropsHere.Add("enabled", true); // not a C# property
            addPropsHere.Add("height", 480.0);
            //addPropsHere.Add("inMovie", true);
            //addPropsHere.Add("inPoster", false);
            //addPropsHere.Add("inPreview", true);
            addPropsHere.Add("layer", (int)213);
            addPropsHere.Add("matrix", Matrix.ROTATE_180);
            addPropsHere.Add("modificationTime", new DateTime(1907, 1, 1).AddMilliseconds(1369296386000L));
            addPropsHere.Add("trackId", (long)23423);
            addPropsHere.Add("volume", (float)1.0);
            addPropsHere.Add("width", 640.0);
        }
    }
}
