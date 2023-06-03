using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using SharpMp4Parser.Muxer.Container.MP4;
using SharpMp4Parser.Muxer;
using SharpMp4Parser.Streaming.Output.MP4;
using SharpMp4Parser.Streaming;
using SharpMp4Parser.Streaming.Input.H265;

namespace SharpMp4Parser.Tests.Streaming.Input.H265
{
    /**
    * Created by Jimm98y on 5/29/2023.
    */
    [TestClass]
    public class H265AnnexBTrackTest
    {
        [TestMethod]
        public async Task testMuxing()
        {
            FileStream h265Fis = File.OpenRead("hevc.h265");
            var h265Stream = new ByteStream(h265Fis);

            H265AnnexBTrack b = new H265AnnexBTrack(h265Stream);
            ByteStream baos = new ByteStream();
            StandardMp4Writer writer = new StandardMp4Writer(new List<StreamingTrack> { b }, Channels.newChannel(baos));
            //FragmentedMp4Writer writer = new FragmentedMp4Writer(new List<StreamingTrack> { b }, Channels.newChannel(baos));
            //MultiTrackFragmentedMp4Writer writer = new MultiTrackFragmentedMp4Writer(new StreamingTrack[]{b}, new ByteArrayOutputStream());
            await Task.Run(() => b.call());
            writer.close();

            File.WriteAllBytes("C:\\Temp\\AAA.mp4", baos.toByteArray());

            IsoFile isoFile = new IsoFile(Channels.newChannel(new ByteStream(baos.toByteArray())));
            Walk.through(isoFile);
            IList<Sample> s = new Mp4SampleList(1, isoFile, baos);
            foreach (Sample sample in s)
            {
                //            System.err.println("s: " + sample.getSize());
                sample.asByteBuffer();
            }

            h265Fis.Close();
        }
    }
}
