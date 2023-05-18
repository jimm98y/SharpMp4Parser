using SharpMp4Parser.IsoParser.Boxes.Apple;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.Apple
{
    /**
     * Created with IntelliJ IDEA.
     * User: sannies
     * Date: 6/24/12
     * Time: 3:53 PM
     * To change this template use File | Settings | File Templates.
     */
    [TestClass]
    public class BaseMediaInfoAtomTest : BoxWriteReadBase<BaseMediaInfoAtom>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(BaseMediaInfoAtom);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, BaseMediaInfoAtom box)
        {
            addPropsHere.Add("balance", (short)321);
            addPropsHere.Add("graphicsMode", (short)43);
            addPropsHere.Add("opColorB", (int)124);
            addPropsHere.Add("opColorG", (int)445);
            addPropsHere.Add("opColorR", (int)5321);
            addPropsHere.Add("reserved", (short)344);
        }
    }
}
