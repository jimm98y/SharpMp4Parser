using SharpMp4Parser.IsoParser.Boxes.Apple;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.Apple
{
    /**
     * Created with IntelliJ IDEA.
     * User: sannies2
     * Date: 2/1/13
     * Time: 11:16 AM
     * To change this template use File | Settings | File Templates.
     */
    [TestClass]
    public class TrackLoadSettingsAtomTest : BoxWriteReadBase<TrackLoadSettingsAtom>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(TrackLoadSettingsAtom);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, TrackLoadSettingsAtom box)
        {
            addPropsHere.Add("defaultHints", 34);
            addPropsHere.Add("preloadDuration", 35);
            addPropsHere.Add("preloadFlags", 36);
            addPropsHere.Add("preloadStartTime", 37);
        }
    }
}