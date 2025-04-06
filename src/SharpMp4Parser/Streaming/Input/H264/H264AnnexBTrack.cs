using SharpMp4Parser.Java;
using SharpMp4Parser.Streaming.Extensions;
using SharpMp4Parser.Tests.Streaming.Input;
using System.Diagnostics;

namespace SharpMp4Parser.Streaming.Input.H264
{
    /**
     * Reads H264 data from an Annex B InputStream.
     */
    public class H264AnnexBTrack : H264NalConsumingTrack /*, Callable<Void> */
    {

        private ByteStream inputStream;

        public H264AnnexBTrack(ByteStream inputStream)
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
            pushSample(createSample(buffered, fvnd.sliceHeader, sliceNalUnitHeader), true, true);
        }

        public override string ToString()
        {
            TrackIdTrackExtension trackIdTrackExtension = this.getTrackExtension< TrackIdTrackExtension>(typeof(TrackIdTrackExtension));
            if (trackIdTrackExtension != null)
            {
                return "H264AnnexBTrack{trackId=" + trackIdTrackExtension.getTrackId() + "}";
            }
            else
            {
                return "H264AnnexBTrack{}";
            }
        }
    }
}
