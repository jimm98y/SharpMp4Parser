using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpMp4Parser.Java;
using SharpMp4Parser.Streaming.Extensions;
using SharpMp4Parser.Muxer.Tracks.H265;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15;

namespace SharpMp4Parser.Streaming.Input.H265
{
    /**
    * Created by Jimm98y on 5/29/2023.
    */
    public abstract class H265NalConsumingTrack : AbstractStreamingTrack
    {
        //private static Logger LOG = LoggerFactory.getLogger(H265NalConsumingTrack.class.getName());
        List<ByteBuffer> sps = new List<ByteBuffer>();
        List<ByteBuffer> pps = new List<ByteBuffer>();
        List<ByteBuffer> vps = new List<ByteBuffer>();

        long timescale = 0;
        long frametick = 0;
        bool configured;

        SampleDescriptionBox stsd;
        public List<ByteBuffer> nals = new List<ByteBuffer>();
        bool vclNalUnitSeenInAU = false;
        bool isIdr = true;

        protected H265NalUnitHeader sliceNalUnitHeader;

        private SequenceParameterSetRbsp parsedSPS;
        private VideoParameterSet parsedVPS;
        private PictureParameterSetRbsp parsedPPS;

        public H265NalConsumingTrack()
        {
        }

        protected void consumeNal(ByteBuffer nal)
        {
            //Java.LOG.finest("Consume NAL of " + nal.position() + " bytes." + Hex.encodeHex(new byte[]{nal.get(0), nal.get(1), nal.get(2), nal.get(3), nal.get(4)}));
            H265NalUnitHeader unitHeader = getNalUnitHeader(nal);
            //
            if (vclNalUnitSeenInAU)
            { // we need at least 1 VCL per AU
              // This branch checks if we encountered the start of a samples/AU
                if (isVcl(unitHeader))
                {
                    if ((nal.get(2) & -128) != 0)
                    { // this is: first_slice_segment_in_pic_flag  u(1)
                        wrapUp(nals, ref vclNalUnitSeenInAU, ref isIdr);
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
                            wrapUp(nals, ref vclNalUnitSeenInAU, ref isIdr);
                            break;
                    }
                }
            }
                
            // collect sps/vps/pps
            switch (unitHeader.nalUnitType)
            {
                case H265NalUnitTypes.NAL_TYPE_PPS_NUT:
                    nal.position(0);
                    pps.Add(nal.slice());
                    ((Java.Buffer)nal).position(2);
                    // emulation prevention bytes are already removed here, do not remove them again as it would have corrupted the PPS
                    // TODO: appending zeroes at the end seems to be required because of the current parsing logic in order to prevent exception being thrown
                    byte[] bPPS = pps[pps.Count - 1].array().Skip(pps[pps.Count - 1].arrayOffset() + 2).Take(pps[pps.Count - 1].limit()).Concat(new byte[] { 0 }).ToArray();
                    parsedPPS = new PictureParameterSetRbsp(ByteBuffer.wrap(bPPS));
                    Java.LOG.debug("Stored PPS");
                    break;
                case H265NalUnitTypes.NAL_TYPE_VPS_NUT:
                    nal.position(0);
                    vps.Add(nal.slice());
                    ((Java.Buffer)nal).position(2);
                    // emulation prevention bytes are already removed here, do not remove them again as it would have corrupted the PPS
                    // TODO: appending zeroes at the end seems to be required because of the current parsing logic in order to prevent exception being thrown
                    byte[] bVPS = vps[vps.Count - 1].array().Skip(vps[vps.Count - 1].arrayOffset() + 2).Take(vps[vps.Count - 1].limit()).Concat(new byte[] { 0 }).ToArray();
                    parsedVPS = new VideoParameterSet(ByteBuffer.wrap(bVPS));
                    Java.LOG.debug("Stored VPS");
                    break;
                case H265NalUnitTypes.NAL_TYPE_SPS_NUT:
                    nal.position(0);
                    sps.Add(nal.slice());
                    ((Java.Buffer)nal).position(2);
                    // emulation prevention bytes are already removed here, do not remove them again as it would have corrupted the PPS
                    // TODO: appending zeroes at the end seems to be required because of the current parsing logic in order to prevent exception being thrown
                    byte[] bSPS = sps[sps.Count - 1].array().Skip(sps[sps.Count - 1].arrayOffset() + 2).Take(sps[sps.Count - 1].limit()).Concat(new byte[] { 0 }).ToArray();
                    parsedSPS = new SequenceParameterSetRbsp(new ByteBufferByteChannel(bSPS));
                    Java.LOG.debug("Stored SPS");
                    break;
                case H265NalUnitTypes.NAL_TYPE_PREFIX_SEI_NUT:
                    ((Java.Buffer)nal).position(2);
                    var sei = new SEIMessage(new BitReaderBuffer(nal.slice()));
                    break;
            }

            switch (unitHeader.nalUnitType)
            {
                // for hvc1 these must be in mdat!!! Otherwise the video is not playable.
                //case H265NalUnitTypes.NAL_TYPE_SPS_NUT:
                //case H265NalUnitTypes.NAL_TYPE_VPS_NUT:
                //case H265NalUnitTypes.NAL_TYPE_PPS_NUT:
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
                        isIdr = true;
                        sliceNalUnitHeader = unitHeader;
                        break;
                    default:
                        isIdr = false;
                        break;
                }
            }

            vclNalUnitSeenInAU |= isVcl(unitHeader);
        }

        public void configure()
        {
            if (!configured)
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
                hevcConfigurationBox.getHevcDecoderConfigurationRecord().setGeneral_profile_idc(parsedSPS.general_profile_idc);
                hevcConfigurationBox.getHevcDecoderConfigurationRecord().setConfigurationVersion(1);
                hevcConfigurationBox.getHevcDecoderConfigurationRecord().setChromaFormat(parsedSPS.chroma_format_idc);
                hevcConfigurationBox.getHevcDecoderConfigurationRecord().setGeneral_level_idc(parsedVPS.general_level_idc);
                hevcConfigurationBox.getHevcDecoderConfigurationRecord().setGeneral_profile_compatibility_flags(parsedVPS.general_profile_compatibility_flags);
                hevcConfigurationBox.getHevcDecoderConfigurationRecord().setGeneral_constraint_indicator_flags(parsedVPS.general_profile_constraint_indicator_flags);
                hevcConfigurationBox.getHevcDecoderConfigurationRecord().setLengthSizeMinusOne(3); // 4 bytes size block inserted in between NAL units

                DimensionTrackExtension dte = this.getTrackExtension<DimensionTrackExtension>(typeof(DimensionTrackExtension));
                if (dte == null)
                {
                    this.addTrackExtension(new DimensionTrackExtension(parsedSPS.pic_width_in_luma_samples, parsedSPS.pic_height_in_luma_samples));
                }

                HevcDecoderConfigurationRecord.Array spsArray = new HevcDecoderConfigurationRecord.Array();
                spsArray.array_completeness = true;
                spsArray.nal_unit_type = H265NalUnitTypes.NAL_TYPE_SPS_NUT;
                spsArray.nalUnits = new List<byte[]>();
                foreach (ByteBuffer sp in this.sps)
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

                // correct order is VPS, SPS, PPS. Other order produced ffmpeg errors such as "VPS 0 does not exist" and "SPS 0 does not exist."
                hevcConfigurationBox.getArrays().AddRange(Arrays.asList(vpsArray, spsArray, ppsArray));

                visualSampleEntry.addBox(hevcConfigurationBox);

                visualSampleEntry.setWidth(parsedSPS.pic_width_in_luma_samples);
                visualSampleEntry.setHeight(parsedSPS.pic_height_in_luma_samples);

                stsd = new SampleDescriptionBox();
                stsd.addBox(visualSampleEntry);

                long _timescale;
                long _frametick;

                if (parsedVPS.vps_timing_info_present_flag)
                {
                    _timescale = parsedVPS.vps_time_scale;
                    _frametick = parsedVPS.vps_num_units_in_tick;

                    if (_timescale == 0 || _frametick == 0)
                    {
                        Java.LOG.warn("vps contains invalid values: time_scale: " + _timescale + " and frame_tick: " + _frametick + ". Setting frame rate to 25fps");
                        _timescale = 0;
                        _frametick = 0;
                    }
                    if (_frametick > 0)
                    {
                        if (_timescale / _frametick > 100)
                        {
                            Java.LOG.warn("Framerate is " + (_timescale / _frametick) + ". That is suspicious.");
                        }
                    }
                    else
                    {
                        Java.LOG.warn("Frametick is " + _frametick + ". That is suspicious.");
                    }
                }
                else
                {
                    if (parsedSPS.vuiParameters != null && parsedSPS.vuiParameters.vui_timing_info_present_flag)
                    {
                        _timescale = parsedSPS.vuiParameters.vui_time_scale;
                        _frametick = parsedSPS.vuiParameters.vui_num_units_in_tick;
                        if (_timescale == 0 || _frametick == 0)
                        {
                            Java.LOG.warn("vuiParams contain invalid values: time_scale: " + _timescale + " and frame_tick: " + _frametick + ". Setting frame rate to 25fps");
                            _timescale = 0;
                            _frametick = 0;
                        }
                        if (_frametick > 0)
                        {
                            if (_timescale / _frametick > 100)
                            {
                                Java.LOG.warn("Framerate is " + (_timescale / _frametick) + ". That is suspicious.");
                            }
                        }
                        else
                        {
                            Java.LOG.warn("Frametick is " + _frametick + ". That is suspicious.");
                        }
                    }
                    else
                    {
                        Java.LOG.warn("Can't determine frame rate as SPS does not contain vuiParam");
                        _timescale = 0;
                        _frametick = 0;
                    }
                }

                if (timescale == 0)
                {
                    timescale = _timescale;
                }
                if (frametick == 0)
                {
                    frametick = _frametick;
                }
                configured = true;
            }
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

        protected static byte[] toArray(ByteBuffer buf)
        {
            buf = (ByteBuffer)buf.duplicate();
            byte[] b = new byte[buf.remaining()];
            buf.get(b, 0, b.Length);
            return b;
        }

        public void wrapUp(List<ByteBuffer> nals, ref bool vclNalUnitSeenInAU, ref bool isIdr)
        {
            pushSample(createSample(nals), false, false);
            Java.LOG.debug("Create AU from " + nals.Count + " NALs");
            if (isIdr)
            {
                Java.LOG.debug("  IDR");
            }

            vclNalUnitSeenInAU = false;
            isIdr = true;
            nals.Clear();
        }

        protected StreamingSample createSample(List<ByteBuffer> nals)
        {
            Java.LOG.debug("Create Sample");
            configure();
            if (timescale == 0 || frametick == 0)
            {
                throw new IOException("Frame Rate needs to be configured either by hand or by SPS before samples can be created");
            }

            StreamingSample ss = new StreamingSampleImpl(nals, frametick);
            return ss;
        }

        public override string getHandler()
        {
            return "vide";
        }

        public override string getLanguage()
        {
            return "eng";
        }

        public override void close()
        {

        }

        public override SampleDescriptionBox getSampleDescriptionBox()
        {
            configure();
            return stsd;
        }

        bool isVcl(H265NalUnitHeader nalUnitHeader)
        {
            return nalUnitHeader.nalUnitType >= 0 && nalUnitHeader.nalUnitType <= 31;
        }

        public override long getTimescale()
        {
            configure();
            return timescale;
        }

        public void setTimescale(int timescale)
        {
            this.timescale = timescale;
        }

        protected void pushSample(StreamingSample ss, bool all, bool force)
        {
            sampleSink.acceptSample(ss, this);
        }
    }
}