using SharpMp4Parser.IsoParser.Boxes.Dece;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.Ultraviolet
{
    /**
    *
    */
    [TestClass]
    public class BaseLocationBoxTest : BoxWriteReadBase<BaseLocationBox>
    {

        public override Type getBoxUnderTest()
        {
            return typeof(BaseLocationBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, BaseLocationBox box)
        {
            addPropsHere.Add("baseLocation", " ");
            addPropsHere.Add("purchaseLocation", " ");
        }
    }
}
