﻿using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Tests.Streaming.Input.H264;

namespace SharpMp4Parser.Tests.IsoParser.MiscRepro
{
    [TestClass]
    public class InitDataChromeCenc
    {
        string eInitData = "000000347073736800000000edef8ba979d64acea3c827dcd51d21ed000000140801121030313233343536373839303132333435000002ea70737368000000009a04f07998404286ab92e65be0885f95000002caca02000001000100c0023c00570052004d00480045004100440045005200200078006d006c006e0073003d00220068007400740070003a002f002f0073006300680065006d00610073002e006d006900630072006f0073006f00660074002e0063006f006d002f00440052004d002f0032003000300037002f00300033002f0050006c00610079005200650061006400790048006500610064006500720022002000760065007200730069006f006e003d00220034002e0030002e0030002e00300022003e003c0044004100540041003e003c00500052004f00540045004300540049004e0046004f003e003c004b00450059004c0045004e003e00310036003c002f004b00450059004c0045004e003e003c0041004c004700490044003e004100450053004300540052003c002f0041004c004700490044003e003c002f00500052004f00540045004300540049004e0046004f003e003c004b00490044003e004d007a00490078004d004400550030004e007a00590034004f005400410078004d006a004d0030004e0051003d003d003c002f004b00490044003e003c0043004800450043004b00530055004d003e0044004d0039004c00590079006d003000470049006f003d003c002f0043004800450043004b00530055004d003e003c004c0041005f00550052004c003e00680074007400700073003a002f002f007700770077002e0079006f00750074007500620065002e0063006f006d002f006100700069002f00640072006d002f0070006c0061007900720065006100640079003f0073006f0075007200630065003d0059004f0055005400550042004500260061006d0070003b0076006900640065006f005f00690064003d00540065007300740056006900640065006f00490064003c002f004c0041005f00550052004c003e003c002f0044004100540041003e003c002f00570052004d004800450041004400450052003e0000000030707373680000000058147ec80423465992e6f52c5ce8c3cc0000001030313233343536373839303132333435";

        [TestMethod]
        public void ausprobieren()
        {
            IsoFile isoFile = new IsoFile(new ByteBufferByteChannel(Hex.decodeHex(eInitData)));
            Walk.through(isoFile);
        }
    }
}
