using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using SharpMp4Parser.Muxer.Tracks.H265;
using SharpMp4Parser.Muxer.Tracks;

namespace SharpMp4Parser.Tests.Muxer.Tracks.H265
{
    [TestClass]
    public class SequenceParameterSetRbspTest
    {
        [TestMethod]
        public void test1()
        {
            byte[] hecCBytes = Hex.decodeHex("0000009068766343010220000000B0000000000096F000FCFDFAFA00000F03200001002040010C01FFFF022000000300B0000003000003009698903000003E900005DC052100010039420101022000000300B00000030000030096A001E020021C4D94626491B6BC05A84880482000007D20000BB80C25BDEFC0006C948000BEBC1022000100094401C1625B162C1ED9");
            HevcConfigurationBox hvcC = (HevcConfigurationBox)new IsoFile(Channels.newChannel(new ByteStream(hecCBytes))).getBoxes()[0];
            foreach (HevcDecoderConfigurationRecord.Array array in hvcC.getArrays())
            {
                if (array.nal_unit_type == 33)
                {
                    foreach (byte[] nalUnit in array.nalUnits)
                    {
                        ByteStream bais = new CleanByteStreamBase(new ByteStream(nalUnit));
                        bais.read(); // nal unit header
                        bais.read(); // nal unit header
                        SequenceParameterSetRbsp sps = new SequenceParameterSetRbsp(bais);
                        Assert.IsTrue(sps.vuiParameters.colour_description_present_flag);
                        Assert.AreEqual(9, sps.vuiParameters.colour_primaries);
                        Assert.AreEqual(16, sps.vuiParameters.transfer_characteristics);
                        Assert.AreEqual(9, sps.vuiParameters.matrix_coeffs);
                    }
                }
            }
        }
    }
}