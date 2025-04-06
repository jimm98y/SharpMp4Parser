using SharpMp4Parser.IsoParser.Boxes.Apple;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.Apple
{
    /**
     * Created with IntelliJ IDEA.
     * User: sannies
     * Date: 11/18/12
     * Time: 11:31 AM
     * To change this template use File | Settings | File Templates.
     */
    [TestClass]
    public class TrackEncodedPixelsDimensionsAtomTest : BoxWriteReadBase<TrackEncodedPixelsDimensionsAtom>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(TrackEncodedPixelsDimensionsAtom);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, TrackEncodedPixelsDimensionsAtom box)
        {
            addPropsHere.Add("height", 123.0);
            addPropsHere.Add("width", 321.0);
        }
    }
}
