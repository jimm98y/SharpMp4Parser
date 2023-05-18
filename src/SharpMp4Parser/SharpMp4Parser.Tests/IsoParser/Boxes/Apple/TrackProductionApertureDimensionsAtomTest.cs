using SharpMp4Parser.IsoParser.Boxes.Apple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12.ItemLocationBox;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.Apple
{
    /**
 * Created with IntelliJ IDEA.
 * User: sannies
 * Date: 11/18/12
 * Time: 11:32 AM
 * To change this template use File | Settings | File Templates.
 */
    [TestClass]
    public class TrackProductionApertureDimensionsAtomTest : BoxWriteReadBase<TrackProductionApertureDimensionsAtom>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(TrackProductionApertureDimensionsAtom);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, TrackProductionApertureDimensionsAtom box)
        {
            addPropsHere.Add("height", 123.0);
            addPropsHere.Add("width", 321.0);
        }
    }
}
