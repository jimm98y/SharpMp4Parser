using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using SharpMp4Parser.Muxer.Tracks.H264.Parsing.Read;

namespace SharpMp4Parser.Muxer.Tracks.H265
{
    public class PictureParameterSetRbsp
    {
        ByteBuffer pps;
        private int pic_parameter_set_id;

        public PictureParameterSetRbsp(ByteBuffer pps)
        {
            this.pps = pps;
            CAVLCReader r = new CAVLCReader(new ByteBufferByteChannel(pps));

            pic_parameter_set_id = r.readUE("PPS: pic_parameter_set_id");

            // TODO
        }
    }
}