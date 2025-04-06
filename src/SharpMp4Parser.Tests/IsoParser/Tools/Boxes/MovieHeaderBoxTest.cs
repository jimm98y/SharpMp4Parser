using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.Tests.IsoParser.Boxes;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Tools.Boxes
{
    [TestClass]
    public class MovieHeaderBoxTest : BoxWriteReadBase<MovieHeaderBox>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(MovieHeaderBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, MovieHeaderBox box)
        {
            addPropsHere.Add("creationTime", new DateTime(1904, 1, 1).AddMilliseconds(1369296286000L));
            addPropsHere.Add("currentTime", (int)2342);
            addPropsHere.Add("duration", (long)243423);
            addPropsHere.Add("matrix", Matrix.ROTATE_270);
            addPropsHere.Add("modificationTime", new DateTime(1904, 1, 1).AddMilliseconds(1369296286000L));
            addPropsHere.Add("nextTrackId", (long)5543);
            addPropsHere.Add("posterTime", 5433);
            addPropsHere.Add("previewDuration", 5343);
            addPropsHere.Add("previewTime", 666);
            addPropsHere.Add("rate", (double)1);
            addPropsHere.Add("selectionDuration", 32);
            addPropsHere.Add("selectionTime", 4456);
            addPropsHere.Add("timescale", (long)7565);
            addPropsHere.Add("volume", (float)1.0);
        }
    }
}
