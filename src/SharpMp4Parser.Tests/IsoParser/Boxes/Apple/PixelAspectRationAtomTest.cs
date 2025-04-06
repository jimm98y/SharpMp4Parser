using SharpMp4Parser.IsoParser.Boxes.Apple;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.Apple
{
    [TestClass]
    public class PixelAspectRationAtomTest : BoxWriteReadBase<PixelAspectRationAtom>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(PixelAspectRationAtom);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, PixelAspectRationAtom box)
        {
            addPropsHere.Add("hSpacing", 25);
            addPropsHere.Add("vSpacing", 26);
        }
    }
}
