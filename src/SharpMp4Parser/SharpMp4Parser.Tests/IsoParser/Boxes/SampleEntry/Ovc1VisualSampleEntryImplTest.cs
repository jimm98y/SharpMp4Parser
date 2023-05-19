using SharpMp4Parser.IsoParser.Boxes.SampleEntry;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.SampleEntry
{
    /**
     * Created by sannies on 22.05.13.
     */
    [TestClass]
    public class Ovc1VisualSampleEntryImplTest : BoxWriteReadBase<Ovc1VisualSampleEntryImpl>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(Ovc1VisualSampleEntryImpl);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, Ovc1VisualSampleEntryImpl box)
        {
            addPropsHere.Add("dataReferenceIndex", (int)546);
            addPropsHere.Add("vc1Content", (byte[])new byte[] { 1, 2, 3, 4, 5, 6, 1, 2, 3 });
        }
    }
}
