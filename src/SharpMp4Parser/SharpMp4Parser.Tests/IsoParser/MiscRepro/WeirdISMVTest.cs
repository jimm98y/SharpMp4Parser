﻿using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.IsoParser;
using System.Diagnostics;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.Tests.IsoParser.MiscRepro
{
    [TestClass]
    public class WeirdISMVTest
    {
        [TestMethod]
        public void checkMikesStream()
        {
            string hex = "000002346d6f6f760000006c6d76686400000000ceec8b72ceec8b7200989680000000000001000001000000000000000000000000010000000000000000000000000000000100000000000000000000000000004000000000000000000000000000000000000000000000000000000000000002000001987472616b0000005c746b686400000007ceec8b72ceec8b72000000010000000000000000000000000000000000000000010000000001000000000000000000000000000000010000000000000000000000000000400000000000000000000000000001346d646961000000206d64686400000000ceec8b72ceec8b720098968000000000000000000000003468646c720000000000000000736f756e000000000000000000000000536f756e64204d656469612048616e646c657200000000d86d696e6600000010736d686400000000000000000000002464696e660000001c6472656600000000000000010000000c75726c20000000010000009c7374626c00000050737473640000000000000001000000406f776d61000000000000000100000000000000000002000000000000bb8000006101020080bb0000c31e0000f00310000a00008800001f00000000000000001073747473000000000000000000000010737473630000000000000000000000147374737a000000000000000000000000000000107374636f0000000000000000000000286d7665780000002074726578000000000000000100000001000000000000000000000000";
            byte[] data = Hex.decodeHex(hex);
            IsoFile isoFile = new IsoFile(new ByteBufferByteChannel(data));
            AudioSampleEntry owma = SharpMp4Parser.IsoParser.Tools.Path.getPath<AudioSampleEntry>(isoFile, "moov[0]/trak[0]/mdia[0]/minf[0]/stbl[0]/stsd[0]/owma");
            Debug.WriteLine(owma);
            ByteStream baos = new ByteStream();

            isoFile.getBox(Channels.newChannel(baos));
            Assert.IsTrue(Enumerable.SequenceEqual(data, baos.toByteArray()));
            // List<Box> b = ((Container)isoFile.getMovieBox().getBoxes().get(1)).getBoxes().get(1).getBoxes().get(2).getBoxes().get(2).getBoxes().get(0).getBoxes();

        }
    }
}