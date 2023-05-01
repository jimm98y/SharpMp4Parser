using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Tools;
using System.Collections.Generic;

namespace SharpMp4Parser.Muxer.Container.MP4
{
    /**
     * Creates a list of <code>ByteBuffer</code>s that represent the samples of a given track.
     */
    public class Mp4SampleList : List<Sample>
    {
        private List<Sample> samples;

        public Mp4SampleList(long trackId, IsoParser.Container isofile, RandomAccessSource source)
        {

            if (Path.getPaths< TrackExtendsBox>(isofile, "moov/mvex/trex").Count == 0)
            {
                samples = new DefaultMp4SampleList(trackId, isofile, source);
            }
            else
            {
                samples = new FragmentedMp4SampleList(trackId, isofile, source);
            }
        }

        public override Sample get(int index)
        {
            return samples[index];
        }

        public override int size()
        {
            return samples.Count;
        }
    }
}
