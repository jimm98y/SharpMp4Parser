using SharpMp4Parser.IsoParser.Boxes.Apple;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using SharpMp4Parser.IsoParser.Tools;
using System.Diagnostics;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.Apple
{
    [TestClass]
    public class TimeCodeBoxTest : BoxWriteReadBase<TimeCodeBox>
    {
        String tcmd = "00000026746D6364000000000000" +
            "0001000000000000000000005DC00000" +
            "03E918B200000000";

        [TestMethod]
        public void checkRealLifeBox()
        {
            ByteStream fos = new ByteStream();
            fos.write(Hex.decodeHex(tcmd));
            //fos.close();
            fos.position(0);

            IsoFile isoFile = new IsoFile(fos);
            TimeCodeBox ttcmd = (TimeCodeBox)isoFile.getBoxes()[0];
            Debug.WriteLine(ttcmd);
            isoFile.close();
            //f.delete();
        }

        public override Type getBoxUnderTest()
        {
            return typeof(TimeCodeBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, TimeCodeBox box)
        {
            addPropsHere.Add("dataReferenceIndex", 666);
            addPropsHere.Add("frameDuration", 1001);
            addPropsHere.Add("numberOfFrames", 24);
            addPropsHere.Add("reserved1", 0);
            addPropsHere.Add("reserved2", 0);
            addPropsHere.Add("timeScale", 24000);
            addPropsHere.Add("rest", new byte[] { 4, 5 });
        }
    }
}
