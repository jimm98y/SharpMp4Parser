using SharpMp4Parser.IsoParser;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.Tests.IsoParser.Boxes;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Tools.Boxes
{
    /**
     * Created by sannies on 23.05.13.
     */
    [TestClass]
    public class DataReferenceBoxTest : BoxWriteReadBase<DataReferenceBox>
    {

        public override Type getBoxUnderTest()
        {
            return typeof(DataReferenceBox);
        }


        public override void setupProperties(Dictionary<String, Object> addPropsHere, DataReferenceBox box)
        {
            addPropsHere.Add("boxes", new List<Box>() { new FreeBox(100) });
        }
    }
}