using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Tests.IsoParser.Boxes;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Tools.Boxes
{
    /**
     * Created by sannies on 26.05.13.
     */
    [TestClass]
    public class UserDataBoxTest : BoxWriteReadBase<UserDataBox>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(UserDataBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, UserDataBox box)
        {
            addPropsHere.Add("boxes", new List<Box>() { (ParsableBox)new FreeBox(100), new FreeBox(200) });
        }
    }
}
