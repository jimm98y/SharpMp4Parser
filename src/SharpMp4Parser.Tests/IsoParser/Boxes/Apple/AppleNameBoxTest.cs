using SharpMp4Parser.IsoParser.Boxes.Apple;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.Apple
{
    /**
    * Created by sannies on 10/15/13.
    */
    [TestClass]
    public class AppleNameBoxTest : BoxWriteReadBase<AppleNameBox>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(AppleNameBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, AppleNameBox box)
        {
            addPropsHere.Add("value", "The Arrangement");
        }
    }
}
