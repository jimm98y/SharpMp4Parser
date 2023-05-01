using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7;
using SharpMp4Parser.IsoParser;
using System.Collections.Generic;
using SharpMp4Parser.Java;
using SharpMp4Parser.Muxer.Tracks.Encryption;
using SharpMp4Parser.IsoParser.Tools;
using System.Diagnostics;

namespace SharpMp4Parser.Muxer
{
    /**
     * This track implementation is to be used when MP4 track is CENC encrypted.
     */
    public class PiffMp4TrackImpl : Mp4TrackImpl, CencEncryptedTrack
    {

        private List<CencSampleAuxiliaryDataFormat> sampleEncryptionEntries;
        private Uuid defaultKeyId;

        /**
         * Creates a track from a TrackBox and potentially fragments. Use <b>fragements parameter
         * only</b> to supply additional fragments that are not located in the main file.
         *
         * @param trackId      ID of the track to extract
         * @param isofile      the parsed MP4 file
         * @param randomAccess the RandomAccessSource to read the samples from
         * @param name         an arbitrary naem to identify track later - e.g. filename
         * @throws IOException if reading from underlying <code>DataSource</code> fails
         */
        public PiffMp4TrackImpl(long trackId, Container isofile, RandomAccessSource randomAccess, string name) : base(trackId, isofile, randomAccess, name)
        {
            TrackBox trackBox = null;
            foreach (TrackBox box in Path.getPaths<TrackBox>(isofile, "moov/trak"))
            {
                if (box.getTrackHeaderBox().getTrackId() == trackId)
                {
                    trackBox = box;
                    break;
                }
            }
            SchemeTypeBox schm = Path.getPath<SchemeTypeBox>(trackBox, "mdia[0]/minf[0]/stbl[0]/stsd[0]/enc.[0]/sinf[0]/schm[0]");
            Debug.Assert(schm != null && (schm.getSchemeType().Equals("piff")), "Track must be PIFF encrypted");

            sampleEncryptionEntries = new List<CencSampleAuxiliaryDataFormat>();

            List<MovieExtendsBox> movieExtendsBoxes = Path.getPaths(isofile, "moov/mvex");
            if (movieExtendsBoxes.Count != 0)
            {
                foreach (MovieFragmentBox movieFragmentBox in isofile.getBoxes(typeof(MovieFragmentBox)))
                {
                    List<TrackFragmentBox> trafs = movieFragmentBox.getBoxes<TrackFragmentBox>(typeof(TrackFragmentBox));
                    foreach (TrackFragmentBox traf in trafs)
                    {
                        if (traf.getTrackFragmentHeaderBox().getTrackId() == trackId)
                        {
                            AbstractTrackEncryptionBox tenc = Path.getPath<AbstractTrackEncryptionBox>(trackBox, "mdia[0]/minf[0]/stbl[0]/stsd[0]/enc.[0]/sinf[0]/schi[0]/uuid[0]");
                            Debug.Assert(tenc != null);
                            defaultKeyId = tenc.getDefault_KID();

                            long baseOffset;
                            if (traf.getTrackFragmentHeaderBox().hasBaseDataOffset())
                            {
                                baseOffset = traf.getTrackFragmentHeaderBox().getBaseDataOffset();
                            }
                            else
                            {
                                Iterator<Box> it = isofile.getBoxes().iterator();
                                baseOffset = 0;
                                for (Box b = it.next(); b != movieFragmentBox; b = it.next())
                                {
                                    baseOffset += b.getSize();
                                }
                            }


                            List<TrackRunBox> truns = traf.getBoxes<TrackRunBox>(typeof(TrackRunBox));
                            int sampleNo = 0;
                            AbstractSampleEncryptionBox senc = traf.getBoxes<AbstractSampleEncryptionBox>(typeof(AbstractSampleEncryptionBox))[0];
                            foreach (CencSampleAuxiliaryDataFormat cencSampleAuxiliaryDataFormat in senc.getEntries())
                            {
                                sampleEncryptionEntries.Add(cencSampleAuxiliaryDataFormat);
                            }

                        }
                    }

                }

            }
        }

        public Uuid getDefaultKeyId()
        {
            return defaultKeyId;
        }

        public bool hasSubSampleEncryption()
        {
            return false;
        }

        public List<CencSampleAuxiliaryDataFormat> getSampleEncryptionEntries()
        {
            return sampleEncryptionEntries;
        }

        public override string ToString()
        {
            return "PiffMp4TrackImpl{" +
                    "handler='" + getHandler() + '\'' +
                    '}';
        }

        public override string getName()
        {
            return "enc(" + base.getName() + ")";
        }
    }
}