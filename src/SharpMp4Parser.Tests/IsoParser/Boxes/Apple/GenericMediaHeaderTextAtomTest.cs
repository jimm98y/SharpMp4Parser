using SharpMp4Parser.IsoParser.Boxes.Apple;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.Apple
{
    /**
     * Created with IntelliJ IDEA.
     * User: sannies
     * Date: 6/24/12
     * Time: 4:59 PM
     * To change this template use File | Settings | File Templates.
     */
    [TestClass]
    public class GenericMediaHeaderTextAtomTest : BoxWriteReadBase<GenericMediaHeaderTextAtom>
    {
        public GenericMediaHeaderTextAtomTest() : base("gmhd")
        {

        }

        public override Type getBoxUnderTest()
        {
            return typeof(GenericMediaHeaderTextAtom);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, GenericMediaHeaderTextAtom box)
        {
            addPropsHere.Add("unknown_1", (int)1);
            addPropsHere.Add("unknown_2", (int)2);
            addPropsHere.Add("unknown_3", (int)3);
            addPropsHere.Add("unknown_4", (int)4);
            addPropsHere.Add("unknown_5", (int)5);
            addPropsHere.Add("unknown_6", (int)6);
            addPropsHere.Add("unknown_7", (int)7);
            addPropsHere.Add("unknown_8", (int)8);
            addPropsHere.Add("unknown_9", (int)9);
        }
    }
}