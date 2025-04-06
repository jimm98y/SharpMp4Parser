using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using SharpMp4Parser.Muxer.Container.MP4;
using SharpMp4Parser.Muxer;
using SharpMp4Parser.Streaming.Output.MP4;
using SharpMp4Parser.Streaming;
using SharpMp4Parser.Streaming.Input.H264;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.Streaming.Input.H264
{
    [TestClass]
    public class H264AnnexBTrackTest
    {
        [TestMethod]
        public async Task testMuxing()
        {
            FileStream h264Fis = File.OpenRead("tos.h264");
            var h264Stream = new ByteStream(h264Fis);

            H264AnnexBTrack b = new H264AnnexBTrack(h264Stream);
            //H264AnnexBTrack b = new H264AnnexBTrack(new FileInputStream("C:\\dev\\mp4parser\\out.264"));
            ByteStream baos = new ByteStream();
            StandardMp4Writer writer = new StandardMp4Writer(new List<StreamingTrack> { b }, Channels.newChannel(baos));
            //FragmentedMp4Writer writer = new FragmentedMp4Writer(new List<StreamingTrack> { b }, Channels.newChannel(baos));
            //MultiTrackFragmentedMp4Writer writer = new MultiTrackFragmentedMp4Writer(new StreamingTrack[]{b}, new ByteArrayOutputStream());
            await Task.Run(() => b.call());
            writer.close();

            //File.WriteAllBytes("C:\\Temp\\BBB.mp4", baos.toByteArray());

            IsoFile isoFile = new IsoFile(Channels.newChannel(new ByteStream(baos.toByteArray())));
            Walk.through(isoFile);
            IList<Sample> s = new Mp4SampleList(1, isoFile, baos);
            foreach (Sample sample in s)
            {
                //            System.err.println("s: " + sample.getSize());
                sample.asByteBuffer();
            }

            h264Fis.Close();
        }
    }
}
