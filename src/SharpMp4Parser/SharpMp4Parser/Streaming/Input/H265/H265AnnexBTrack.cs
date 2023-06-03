using SharpMp4Parser.Java;
using SharpMp4Parser.Streaming.Extensions;
using SharpMp4Parser.Tests.Streaming.Input;
using System.Diagnostics;

namespace SharpMp4Parser.Streaming.Input.H265
{
    /**
     * Reads H265 data from an Annex B InputStream.
     */
    public class H265AnnexBTrack : H265NalConsumingTrack
    {
        private ByteStream inputStream;

        public H265AnnexBTrack(ByteStream inputStream)
        {
            Debug.Assert(inputStream != null);
            this.inputStream = new ByteStream(inputStream); // BufferedInputStream
        }

        public void call()
        {
            byte[] nal;
            NalStreamTokenizer st = new NalStreamTokenizer(inputStream);

            while ((nal = st.getNext()) != null)
            {
                //Debug.WriteLine("NAL before consume");
                consumeNal(ByteBuffer.wrap(nal));
                //Debug.WriteLine("NAL after consume");
            }
            pushSample(createSample(nals), true, true);
        }

        public override string ToString()
        {
            TrackIdTrackExtension trackIdTrackExtension = this.getTrackExtension<TrackIdTrackExtension>(typeof(TrackIdTrackExtension));
            if (trackIdTrackExtension != null)
            {
                return "H265AnnexBTrack{trackId=" + trackIdTrackExtension.getTrackId() + "}";
            }
            else
            {
                return "H265AnnexBTrack{}";
            }
        }
    }
}
