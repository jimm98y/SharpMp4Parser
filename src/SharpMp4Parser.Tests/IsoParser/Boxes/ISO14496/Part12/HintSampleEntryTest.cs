using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12.ItemLocationBox;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.ISO14496.Part12
{
    /**
     * Created by sannies on 14.07.2015.
     */
    [TestClass]
    public class HintSampleEntryTest : BoxRoundtripTest
    {

        [TestMethod]
        public void roundtrip()
        {
            base.roundtrip(new HintSampleEntry("rtp "),
                        new KeyValuePair<string, object>[]{
                                new KeyValuePair<string, object>("dataReferenceIndex", 0x0102),
                                new KeyValuePair<string, object>("data", new byte[]{1, 2, 3, 4})}
                );
        }
    }
}
