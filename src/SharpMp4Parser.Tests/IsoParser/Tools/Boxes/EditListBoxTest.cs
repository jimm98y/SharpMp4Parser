using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.Tests.IsoParser.Boxes;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Tools.Boxes
{
    /**
     * Created with IntelliJ IDEA.
     * User: sannies
     * Date: 11/18/12
     * Time: 10:42 PM
     * To change this template use File | Settings | File Templates.
     */
    public class EditListBoxTest : BoxWriteReadBase<EditListBox>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(EditListBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, EditListBox box)
        {
            addPropsHere.Add("entries", new List<EditListBox.Entry>() { new EditListBox.Entry(box, 12423, 0, 1), new EditListBox.Entry(box, 12423, 0, 0.5) });
        }
    }
}