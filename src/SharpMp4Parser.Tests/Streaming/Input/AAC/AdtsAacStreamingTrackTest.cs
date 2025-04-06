using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using SharpMp4Parser.Muxer.Container.MP4;
using SharpMp4Parser.Muxer;
using SharpMp4Parser.Streaming;
using SharpMp4Parser.Streaming.Input.AAC;
using SharpMp4Parser.Streaming.Output.MP4;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.Streaming.Input.AAC
{
    [TestClass]
    public class AdtsAacStreamingTrackTest
    {
        [TestMethod]
        public async Task testMuxing()
        {
            FileStream aacFis = File.OpenRead("somesound.aac");
            var aacStream = new ByteStream(aacFis);

            AdtsAacStreamingTrack b = new AdtsAacStreamingTrack(aacStream, 65000, 80000);
            ByteStream baos = new ByteStream();
            new FragmentedMp4Writer(new List<StreamingTrack>() { b }, Channels.newChannel(baos));
            //MultiTrackFragmentedMp4Writer writer = new MultiTrackFragmentedMp4Writer(new StreamingTrack[]{b}, new ByteArrayOutputStream());
            await Task.Run(() => b.call());
            IsoFile isoFile = new IsoFile(Channels.newChannel(new ByteStream(baos.toByteArray())));

            //new FileOutputStream("output.mp4").write(baos.toByteArray());

            Walk.through(isoFile);
            IList<Sample> s = new Mp4SampleList(1, isoFile, baos);
            foreach (Sample sample in s)
            {
                //System.err.println("s: " + sample.getSize());
                sample.asByteBuffer();
            }
        }
    }
}
