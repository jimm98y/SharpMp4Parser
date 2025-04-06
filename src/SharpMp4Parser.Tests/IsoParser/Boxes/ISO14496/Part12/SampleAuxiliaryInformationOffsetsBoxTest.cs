using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.ISO14496.Part12
{
    [TestClass]
    public class SampleAuxiliaryInformationOffsetsBoxTest : BoxRoundtripTest
    {
        [TestMethod]
        public void roundtrip()
        {
            base.roundtrip(new SampleAuxiliaryInformationOffsetsBox(),
                        new KeyValuePair<string, object>[]{
                                new KeyValuePair<string, object>("version", 0),
                                new KeyValuePair<string, object>("flags", 0),
                                new KeyValuePair<string, object>("auxInfoType", null),
                                new KeyValuePair<string, object>("auxInfoTypeParameter", null),
                                new KeyValuePair<string, object>("offsets", new long[]{12, 34, 56, 78})
                        });


            base.roundtrip(new SampleAuxiliaryInformationOffsetsBox(),
                        new KeyValuePair<string, object>[]{
                                new KeyValuePair<string, object>("version", 0),
                                new KeyValuePair<string, object>("flags", 1),
                                new KeyValuePair<string, object>("auxInfoType", "abcd"),
                                new KeyValuePair<string, object>("auxInfoTypeParameter", "defg"),
                                new KeyValuePair<string, object>("offsets", new long[]{12, 34, 56, 78})
                        }
                );
            base.roundtrip(new SampleAuxiliaryInformationOffsetsBox(),
                        new KeyValuePair<string, object>[]{
                                new KeyValuePair<string, object>("version", 1),
                                new KeyValuePair<string, object>("flags", 0),
                                new KeyValuePair<string, object>("auxInfoType", null),
                                new KeyValuePair<string, object>("auxInfoTypeParameter", null),
                                new KeyValuePair<string, object>("offsets", new long[]{12, 34, 56, 78})
                        }
                );
            base.roundtrip(new SampleAuxiliaryInformationOffsetsBox(),
                        new KeyValuePair<string, object>[]{
                                new KeyValuePair<string, object>("version", 0),
                                new KeyValuePair<string, object>("flags", 0),
                                new KeyValuePair<string, object>("auxInfoType", null),
                                new KeyValuePair<string, object>("auxInfoTypeParameter", null),
                                new KeyValuePair<string, object>("offsets", new long[]{12, 34, 56, 78})
                        }
                );
    }
}
}
