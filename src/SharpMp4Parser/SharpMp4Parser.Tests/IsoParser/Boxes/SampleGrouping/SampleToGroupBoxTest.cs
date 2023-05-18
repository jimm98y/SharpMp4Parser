using SharpMp4Parser.IsoParser.Boxes.SampleGrouping;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.SampleGrouping
{
    [TestClass]
    public class SampleToGroupBoxTest : BoxWriteReadBase<SampleToGroupBox>
    {

        public override Type getBoxUnderTest()
        {
            return typeof(SampleToGroupBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, SampleToGroupBox box)
        {
            addPropsHere.Add("entries", new List<SampleToGroupBox.Entry>() { new SampleToGroupBox.Entry(1, 2), new SampleToGroupBox.Entry(2, 3), new SampleToGroupBox.Entry(10, 20) });
            addPropsHere.Add("groupingType", "grp1");
            addPropsHere.Add("groupingTypeParameter", "gtyp");
            addPropsHere.Add("version", 1);
        }
    }
}
