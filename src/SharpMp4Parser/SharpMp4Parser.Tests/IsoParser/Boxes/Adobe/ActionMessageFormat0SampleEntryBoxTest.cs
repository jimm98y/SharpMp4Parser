using SharpMp4Parser.IsoParser;
using SharpMp4Parser.IsoParser.Boxes.Adobe;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.Adobe
{
    /**
     * Created by sannies on 22.05.13.
     */
    [TestClass]
    public class ActionMessageFormat0SampleEntryBoxTest : BoxWriteReadBase<ActionMessageFormat0SampleEntryBox>
    {

        public override Type getBoxUnderTest()
        {
            return typeof(ActionMessageFormat0SampleEntryBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, ActionMessageFormat0SampleEntryBox box)
        {
            addPropsHere.Add("boxes", new List<Box>() { new FreeBox(100) });
            addPropsHere.Add("dataReferenceIndex", 4344);
        }
    }
}
