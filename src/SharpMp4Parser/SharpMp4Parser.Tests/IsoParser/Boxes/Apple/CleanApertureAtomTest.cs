using SharpMp4Parser.IsoParser.Boxes.Apple;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.Apple
{
    [TestClass]
    public class CleanApertureAtomTest : BoxWriteReadBase<CleanApertureAtom>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(CleanApertureAtom);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, CleanApertureAtom box)
        {
            addPropsHere.Add("height", 123.0);
            addPropsHere.Add("width", 321.0);
        }
    }
}
