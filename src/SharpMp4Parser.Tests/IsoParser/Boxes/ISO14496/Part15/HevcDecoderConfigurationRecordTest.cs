using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.ISO14496.Part15
{
    [TestClass]
    public class HevcDecoderConfigurationRecordTest
    {
        [TestMethod]
        public void roundtrip()
        {
            String example = "01008000000000000000000000F000FCFDF8F800000F03200001001940010C01FFFF00800000030000030000030000030000B50240210001002842010100800000030000030000030000030000A00280802D1FE5B59246D0CE4924B724AA49F292C822000100074401C1A5581E48";

            ByteBuffer confRecordOrig = ByteBuffer.wrap(Hex.decodeHex(example.Replace(" ", "")));

            HevcDecoderConfigurationRecord h1 = new HevcDecoderConfigurationRecord();
            h1.parse(confRecordOrig);
            ByteBuffer confRecordWritten = ByteBuffer.allocate(h1.getSize());
            h1.write(confRecordWritten);

            HevcDecoderConfigurationRecord h2 = new HevcDecoderConfigurationRecord();
            h2.parse((ByteBuffer)((Java.Buffer)confRecordWritten).rewind());

            Assert.AreEqual(confRecordOrig, confRecordWritten);
            Assert.AreEqual(h1, h2);
        }
    }
}
