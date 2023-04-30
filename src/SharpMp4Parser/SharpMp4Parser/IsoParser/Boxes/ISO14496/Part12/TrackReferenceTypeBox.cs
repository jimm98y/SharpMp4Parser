using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12
{
    /**
     * This box provides a reference from the containing track to another track in the presentation. These references
     * are typed. A 'hint' reference links from the containing hint track to the media data that it hints. A content
     * description reference 'cdsc' links a descriptive or metadata track to the content which it describes. The
     * 'hind' dependency indicates that the referenced track(s)may contain media data required for decoding of
     * the track containing the track reference. The referenced tracks shall be hint tracks. The 'hind' dependency
     * can, for example, be used for indicating the dependencies between hint tracks documenting layered IP
     * multicast over RTP.
     * Exactly one Track Reference Box can be contained within the Track Box.
     * If this box is not present, the track is not referencing anyother track in any way. The reference array is sized
     * to fill the reference type box.
     */
    public class TrackReferenceTypeBox : AbstractBox
    {

        long[] trackIds = new long[0];

        // ‘hint’  the referenced track(s) contain the original media for this hint track
        // ‘cdsc‘  this track describes the referenced track.
        // 'hind'  this track depends on the referenced hint track, i.e., it should only be used if the referenced hint track is used.
        // 'vdep'  this track contains auxiliary depth video information for the referenced video track
        // 'vplx'  this track contains auxiliary parallax video information for the referenced video track

        public TrackReferenceTypeBox(string type) : base(type)
        { }

        protected override long getContentSize()
        {
            return trackIds.Length * 4;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            foreach (long trackId in trackIds)
            {
                IsoTypeWriter.writeUInt32(byteBuffer, trackId);
            }
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            while (content.remaining() >= 4)
            {
                trackIds = Mp4Arrays.copyOfAndAppend(trackIds, new long[] { IsoTypeReader.readUInt32(content) });
            }
        }

        public long[] getTrackIds()
        {
            return trackIds;
        }

        public void setTrackIds(long[] trackIds)
        {
            this.trackIds = trackIds;
        }
    }
}
