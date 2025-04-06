using SharpMp4Parser.Java;
using SharpMp4Parser.Streaming.Extensions;

namespace SharpMp4Parser.Streaming.Input.H264
{
    /**
     * Created by Jimm98y on 5/20/2023.
     */
    public class H264StreamingTrack : H264NalConsumingTrack
    {
        public void ProcessNal(byte[] nal)
        {
            consumeNal(ByteBuffer.wrap(AnnexBUtils.RemoveEmulationPreventionBytes(nal)));
        }

        public void ProcessNalFinalize()
        {
            pushSample(createSample(buffered, fvnd.sliceHeader, sliceNalUnitHeader), true, true);
        }

        public override string ToString()
        {
            TrackIdTrackExtension trackIdTrackExtension = this.getTrackExtension<TrackIdTrackExtension>(typeof(TrackIdTrackExtension));
            if (trackIdTrackExtension != null)
            {
                return "H264StreamingTrack{trackId=" + trackIdTrackExtension.getTrackId() + "}";
            }
            else
            {
                return "H264StreamingTrack{}";
            }
        }
    }
}
