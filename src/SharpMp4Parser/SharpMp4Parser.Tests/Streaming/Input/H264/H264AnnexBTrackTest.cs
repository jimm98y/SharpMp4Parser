using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using SharpMp4Parser.Muxer.Container.MP4;
using SharpMp4Parser.Muxer;
using SharpMp4Parser.Streaming.Output.MP4;
using SharpMp4Parser.Streaming;
using SharpMp4Parser.Streaming.Input.H264;

namespace SharpMp4Parser.Tests.Streaming.Input.H264
{
    [TestClass]
    public class H264AnnexBTrackTest
    {
        [TestMethod]
        public async Task testMuxing()
        {
            using (MemoryStream h264Ms = new MemoryStream())
            {
                FileStream h264Fis = File.OpenRead("tos.h264");
                h264Fis.CopyTo(h264Ms);
                h264Ms.Position = 0;

                var h264Stream = new ByteStream(h264Ms.ToArray());

                H264AnnexBTrack b = new H264AnnexBTrack(h264Stream);
                //H264AnnexBTrack b = new H264AnnexBTrack(new FileInputStream("C:\\dev\\mp4parser\\out.264"));
                ByteStream baos = new ByteStream();
                FragmentedMp4Writer writer = new FragmentedMp4Writer(new List<StreamingTrack> { b }, Channels.newChannel(baos));
                //MultiTrackFragmentedMp4Writer writer = new MultiTrackFragmentedMp4Writer(new StreamingTrack[]{b}, new ByteArrayOutputStream());
                await Task.Run(() => b.call());
                writer.close();
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
}
