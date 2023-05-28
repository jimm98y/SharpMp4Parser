using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.Java;
using System.Collections.Generic;
using SharpMp4Parser.Muxer.Tracks.H265;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using System.IO;
using SharpMp4Parser.Streaming.Extensions;

namespace SharpMp4Parser.Streaming.Input.H265
{
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
        bool[] vclNalUnitSeenInAU = new bool[] { false };
        bool[] isIdr = new bool[] { true };
        private SequenceParameterSetRbsp currentSeqParameterSet;

        protected H265NalUnitHeader sliceNalUnitHeader;

        public H265NalConsumingTrack()
        {
        }

        protected void consumeNal(ByteBuffer nal)
        {
            //Java.LOG.finest("Consume NAL of " + nal.position() + " bytes." + Hex.encodeHex(new byte[]{nal.get(0), nal.get(1), nal.get(2), nal.get(3), nal.get(4)}));
            H265NalUnitHeader unitHeader = getNalUnitHeader(nal);
            //
            if (vclNalUnitSeenInAU[0])
            { // we need at least 1 VCL per AU
              // This branch checks if we encountered the start of a samples/AU
                if (isVcl(unitHeader))
                {
                    if ((nal.get(2) & -128) != 0)
                    { // this is: first_slice_segment_in_pic_flag  u(1)
                        wrapUp(nals, vclNalUnitSeenInAU, isIdr, unitHeader);
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
                            wrapUp(nals, vclNalUnitSeenInAU, isIdr, sliceNalUnitHeader);
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
                    currentSeqParameterSet = new SequenceParameterSetRbsp(Channels.newInputStream(new ByteBufferByteChannel(nal.slice()).position(2)));                    
                    Java.LOG.debug("Stored SPS");
                    break;
                case H265NalUnitTypes.NAL_TYPE_PREFIX_SEI_NUT:
                    //new SEIMessage(new BitReaderBuffer(nal.slice()));
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
                        sliceNalUnitHeader = unitHeader;
                        break;
                    default:
                        isIdr[0] = false;
                        break;
                }
            }

            vclNalUnitSeenInAU[0] |= isVcl(unitHeader);
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
                hevcConfigurationBox.getHevcDecoderConfigurationRecord().setGeneral_profile_idc(1);

                SequenceParameterSetRbsp sps = this.currentSeqParameterSet;
                DimensionTrackExtension dte = this.getTrackExtension<DimensionTrackExtension>(typeof(DimensionTrackExtension));
                if (dte == null)
                {
                    this.addTrackExtension(new DimensionTrackExtension(sps.pic_width_in_luma_samples, sps.pic_height_in_luma_samples));
                }
                visualSampleEntry.setWidth(sps.pic_width_in_luma_samples);
                visualSampleEntry.setHeight(sps.pic_height_in_luma_samples);

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

                hevcConfigurationBox.getArrays().AddRange(Arrays.asList(spsArray, vpsArray, ppsArray));

                visualSampleEntry.addBox(hevcConfigurationBox);
                stsd = new SampleDescriptionBox();
                stsd.addBox(visualSampleEntry);

                long _timescale;
                long _frametick;
                if (sps.vuiParameters != null)
                {
                    _timescale = sps.vuiParameters.vui_time_scale >> 1; // Not sure why, but I found this in several places, and it works...
                    _frametick = sps.vuiParameters.vui_num_units_in_tick;
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
                    Java.LOG.warn("Can't determine frame rate as SPS does not contain vuiParama");
                    _timescale = 0;
                    _frametick = 0;
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

        public void wrapUp(List<ByteBuffer> nals, bool[] vclNalUnitSeenInAU, bool[] isIdr, H265NalUnitHeader unitHeader)
        {
            pushSample(createSample(nals, unitHeader), false, false);
            Java.LOG.debug("Create AU from " + nals.Count + " NALs");
            if (isIdr[0])
            {
                Java.LOG.debug("  IDR");
            }

            vclNalUnitSeenInAU[0] = false;
            isIdr[0] = true;
            nals.Clear();
        }

        protected StreamingSample createSample(List<ByteBuffer> nals, H265NalUnitHeader nu)
        {
            Java.LOG.debug("Create Sample");
            configure();
            if (timescale == 0 || frametick == 0)
            {
                throw new IOException("Frame Rate needs to be configured either by hand or by SPS before samples can be created");
            }

            StreamingSample ss = new StreamingSampleImpl(nals, frametick);
            ss.addSampleExtension(createSampleFlagsSampleExtension(nu));
            //ss.addSampleExtension(createPictureOrderCountType0SampleExtension(sliceHeader));

            return ss;
        }

        protected SampleFlagsSampleExtension createSampleFlagsSampleExtension(H265NalUnitHeader nu)
        {
            SampleFlagsSampleExtension sampleFlagsSampleExtension = new SampleFlagsSampleExtension();

            if (H265NalUnitTypes.NAL_TYPE_IDR_N_LP == nu.nalUnitType || H265NalUnitTypes.NAL_TYPE_IDR_W_RADL != nu.nalUnitType)
            {
                sampleFlagsSampleExtension.setSampleDependsOn(2);
            }
            else
            {
                sampleFlagsSampleExtension.setSampleDependsOn(1);
            }
            sampleFlagsSampleExtension.setSampleIsNonSyncSample(H265NalUnitTypes.NAL_TYPE_IDR_N_LP != nu.nalUnitType && H265NalUnitTypes.NAL_TYPE_IDR_W_RADL != nu.nalUnitType);
            return sampleFlagsSampleExtension;
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