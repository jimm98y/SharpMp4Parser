using SharpMp4Parser.Java;
using SharpMp4Parser.Streaming.Extensions;

namespace SharpMp4Parser.Streaming.Input.H265
{
    /**
    * Created by Jimm98y on 5/25/2023.
    */
    public class H265StreamingTrack : H265NalConsumingTrack
    {
        public void ProcessNal(byte[] nal)
        {
            consumeNal(ByteBuffer.wrap(AnnexBUtils.RemoveEmulationPreventionBytes(nal)));
        }

        public void ProcessNalFinalize()
        {
            pushSample(createSample(nals), true, true);
        }

        public override string ToString()
        {
            TrackIdTrackExtension trackIdTrackExtension = this.getTrackExtension<TrackIdTrackExtension>(typeof(TrackIdTrackExtension));
            if (trackIdTrackExtension != null)
            {
                return "H265StreamingTrack{trackId=" + trackIdTrackExtension.getTrackId() + "}";
            }
            else
            {
                return "H265StreamingTrack{}";
            }
        }
    }
}
