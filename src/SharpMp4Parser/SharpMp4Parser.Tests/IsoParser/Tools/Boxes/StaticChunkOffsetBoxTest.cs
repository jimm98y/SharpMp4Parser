using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.Tests.IsoParser.Boxes;

namespace SharpMp4Parser.Tests.IsoParser.Tools.Boxes
{
    /**
     * Created by sannies on 30.05.13.
     */
    [TestClass]
    public class StaticChunkOffsetBoxTest : BoxWriteReadBase<StaticChunkOffsetBox>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(StaticChunkOffsetBox);
        }
        public override void setupProperties(Dictionary<String, Object> addPropsHere, StaticChunkOffsetBox box)
        {
            addPropsHere.Add("chunkOffsets", new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 });
        }
    }
}