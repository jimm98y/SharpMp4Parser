using SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7;
using System.Collections.Generic;

namespace SharpMp4Parser.Muxer.Tracks.Encryption
{
    /**
     * Track encrypted with common (CENC). ISO/IEC 23001-7.
     */
    public interface CencEncryptedTrack : Track
    {
        List<CencSampleAuxiliaryDataFormat> getSampleEncryptionEntries();

        bool hasSubSampleEncryption();
    }
}
