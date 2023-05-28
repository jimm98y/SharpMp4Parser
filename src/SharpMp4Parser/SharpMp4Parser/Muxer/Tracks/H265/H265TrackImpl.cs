using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharpMp4Parser.Muxer.Tracks.H265
{
    /**
     * Takes a raw H265 stream and muxes into an MP4.
     */
    public class H265TrackImpl : AbstractH26XTrack
    {
        List<ByteBuffer> sps = new List<ByteBuffer>();
        List<ByteBuffer> pps = new List<ByteBuffer>();
        List<ByteBuffer> vps = new List<ByteBuffer>();
        List<Sample> samples = new List<Sample>();

        VisualSampleEntry visualSampleEntry;

        public H265TrackImpl(DataSource dataSource) : base(dataSource)
        {
            List<ByteBuffer> nals = new List<ByteBuffer>();
            LookAhead la = new LookAhead(dataSource);
            ByteBuffer nal;
            bool[] vclNalUnitSeenInAU = new bool[] { false };
            bool[] isIdr = new bool[] { true };

            while ((nal = findNextNal(la)) != null)
            {

                H265NalUnitHeader unitHeader = getNalUnitHeader(nal);
                //
                if (vclNalUnitSeenInAU[0])
                { // we need at least 1 VCL per AU
                  // This branch checks if we encountered the start of a samples/AU
                    if (isVcl(unitHeader))
                    {
                        if ((nal.get(2) & -128) != 0)
                        { // this is: first_slice_segment_in_pic_flag  u(1)
                            wrapUp(nals, vclNalUnitSeenInAU, isIdr);
                        }
                    }
                    else
                    {
                        switch (unitHeader.nalUnitType)
                        {
                            case H265NalUnitTypes.NAL_TYPE_PREFIX_SEI_NUT:
                            case H265NalUnitTypes.NAL_TYPE_AUD_NUT:
                            case H265NalUnitTypes.NAL_TYPE_PPS_NUT:
                            case H265NalUnitTypes.NAL_TYPE_VPS_NUT:
                            case H265NalUnitTypes.NAL_TYPE_SPS_NUT:
                            case H265NalUnitTypes.NAL_TYPE_RSV_NVCL41:
                            case H265NalUnitTypes.NAL_TYPE_RSV_NVCL42:
                            case H265NalUnitTypes.NAL_TYPE_RSV_NVCL43:
                            case H265NalUnitTypes.NAL_TYPE_RSV_NVCL44:
                            case H265NalUnitTypes.NAL_TYPE_UNSPEC48:
                            case H265NalUnitTypes.NAL_TYPE_UNSPEC49:
                            case H265NalUnitTypes.NAL_TYPE_UNSPEC50:
                            case H265NalUnitTypes.NAL_TYPE_UNSPEC51:
                            case H265NalUnitTypes.NAL_TYPE_UNSPEC52:
                            case H265NalUnitTypes.NAL_TYPE_UNSPEC53:
                            case H265NalUnitTypes.NAL_TYPE_UNSPEC54:
                            case H265NalUnitTypes.NAL_TYPE_UNSPEC55:

                            case H265NalUnitTypes.NAL_TYPE_EOB_NUT: // a bit special but also causes a sample to be formed
                            case H265NalUnitTypes.NAL_TYPE_EOS_NUT:
                                wrapUp(nals, vclNalUnitSeenInAU, isIdr);
                                break;
                        }
                    }
                }
                // collect sps/vps/pps
                switch (unitHeader.nalUnitType)
                {
                    case H265NalUnitTypes.NAL_TYPE_PPS_NUT:
                        ((Java.Buffer)nal).position(2);
                        pps.Add(nal.slice());
                        Java.LOG.debug("Stored PPS");
                        break;
                    case H265NalUnitTypes.NAL_TYPE_VPS_NUT:
                        ((Java.Buffer)nal).position(2);
                        vps.Add(nal.slice());
                        Java.LOG.debug("Stored VPS");
                        break;
                    case H265NalUnitTypes.NAL_TYPE_SPS_NUT:
                        ((Java.Buffer)nal).position(2);
                        sps.Add(nal.slice());
                        new SequenceParameterSetRbsp(Channels.newInputStream(new ByteBufferByteChannel(nal.slice()).position(2)));
                        Java.LOG.debug("Stored SPS");
                        break;
                    case H265NalUnitTypes.NAL_TYPE_PREFIX_SEI_NUT:
                        new SEIMessage(new BitReaderBuffer(nal.slice()));
                        break;
                }


                switch (unitHeader.nalUnitType)
                {
                    case H265NalUnitTypes.NAL_TYPE_SPS_NUT:
                    case H265NalUnitTypes.NAL_TYPE_VPS_NUT:
                    case H265NalUnitTypes.NAL_TYPE_PPS_NUT:
                    case H265NalUnitTypes.NAL_TYPE_EOB_NUT:
                    case H265NalUnitTypes.NAL_TYPE_EOS_NUT:
                    case H265NalUnitTypes.NAL_TYPE_AUD_NUT:
                    case H265NalUnitTypes.NAL_TYPE_FD_NUT:
                        // ignore these
                        break;
                    default:
                        Java.LOG.debug("Adding " + unitHeader.nalUnitType);
                        nals.Add(nal);
                        break;
                }
                if (isVcl(unitHeader))
                {
                    switch (unitHeader.nalUnitType)
                    {
                        case H265NalUnitTypes.NAL_TYPE_IDR_W_RADL:
                        case H265NalUnitTypes.NAL_TYPE_IDR_N_LP:
                            isIdr[0] &= true;
                            break;
                        default:
                            isIdr[0] = false;
                            break;
                    }
                }

                vclNalUnitSeenInAU[0] |= isVcl(unitHeader);

            }
            visualSampleEntry = createSampleEntry();
            decodingTimes = new long[samples.Count];
            getTrackMetaData().setTimescale(25);
            Arrays.fill(decodingTimes, 1);
        }

        protected override SampleEntry getCurrentSampleEntry()
        {
            return visualSampleEntry;
        }

        public static H265NalUnitHeader getNalUnitHeader(ByteBuffer nal)
        {
            ((Java.Buffer)nal).position(0);
            int nal_unit_header = IsoTypeReader.readUInt16(nal);

            H265NalUnitHeader nalUnitHeader = new H265NalUnitHeader();
            nalUnitHeader.forbiddenZeroFlag = (nal_unit_header & 0x8000) >> 15;
            nalUnitHeader.nalUnitType = (nal_unit_header & 0x7E00) >> 9;
            nalUnitHeader.nuhLayerId = (nal_unit_header & 0x1F8) >> 3;
            nalUnitHeader.nuhTemporalIdPlusOne = (nal_unit_header & 0x7);
            return nalUnitHeader;
        }

        //public static void main(String[] args) throws IOException
        //{
        //    Track track = new H265TrackImpl(new FileDataSourceImpl("c:\\content\\test-UHD-HEVC_01_FMV_Med_track1.hvc"));
        //    Movie movie = new Movie();
        //    movie.addTrack(track);
        //    DefaultMp4Builder mp4Builder = new DefaultMp4Builder();
        //    Container c = mp4Builder.build(movie);
        //    c.writeContainer(new FileByteStreamBase("output.mp4").getChannel());
        //}

        private VisualSampleEntry createSampleEntry()
        {
            VisualSampleEntry visualSampleEntry = new VisualSampleEntry("hvc1");
            visualSampleEntry.setDataReferenceIndex(1);
            visualSampleEntry.setDepth(24);
            visualSampleEntry.setFrameCount(1);
            visualSampleEntry.setHorizresolution(72);
            visualSampleEntry.setVertresolution(72);
            visualSampleEntry.setWidth(640);
            visualSampleEntry.setHeight(480);
            visualSampleEntry.setCompressorname("HEVC Coding");

            HevcConfigurationBox hevcConfigurationBox = new HevcConfigurationBox();
            hevcConfigurationBox.getHevcDecoderConfigurationRecord().setGeneral_profile_idc(1);

            HevcDecoderConfigurationRecord.Array spsArray = new HevcDecoderConfigurationRecord.Array();
            spsArray.array_completeness = true;
            spsArray.nal_unit_type = H265NalUnitTypes.NAL_TYPE_SPS_NUT;
            spsArray.nalUnits = new List<byte[]>();
            foreach (ByteBuffer sp in sps)
            {
                spsArray.nalUnits.Add(toArray(sp));
            }

            HevcDecoderConfigurationRecord.Array ppsArray = new HevcDecoderConfigurationRecord.Array();
            ppsArray.array_completeness = true;
            ppsArray.nal_unit_type = H265NalUnitTypes.NAL_TYPE_PPS_NUT;
            ppsArray.nalUnits = new List<byte[]>();
            foreach (ByteBuffer pp in pps)
            {
                ppsArray.nalUnits.Add(toArray(pp));
            }

            HevcDecoderConfigurationRecord.Array vpsArray = new HevcDecoderConfigurationRecord.Array();
            vpsArray.array_completeness = true;
            vpsArray.nal_unit_type = H265NalUnitTypes.NAL_TYPE_VPS_NUT;
            vpsArray.nalUnits = new List<byte[]>();
            foreach (ByteBuffer vp in vps)
            {
                vpsArray.nalUnits.Add(toArray(vp));
            }

            hevcConfigurationBox.getArrays().AddRange(Arrays.asList(spsArray, vpsArray, ppsArray));

            visualSampleEntry.addBox(hevcConfigurationBox);
            return visualSampleEntry;
        }

        public void wrapUp(List<ByteBuffer> nals, bool[] vclNalUnitSeenInAU, bool[] isIdr)
        {
            samples.Add(createSampleObject(nals));
            Java.LOG.debug("Create AU from " + nals.Count + " NALs");
            if (isIdr[0])
            {
                Java.LOG.debug("  IDR");
            }

            vclNalUnitSeenInAU[0] = false;
            isIdr[0] = true;
            nals.Clear();
        }

        public override List<SampleEntry> getSampleEntries()
        {
            return null;
        }

        public override string getHandler()
        {
            return "vide";
        }

        public override IList<Sample> getSamples()
        {
            return samples;
        }

        bool isVcl(H265NalUnitHeader nalUnitHeader)
        {
            return nalUnitHeader.nalUnitType >= 0 && nalUnitHeader.nalUnitType <= 31;
        }
    }
}