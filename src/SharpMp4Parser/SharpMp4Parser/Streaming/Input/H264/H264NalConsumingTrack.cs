using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.Java;
using SharpMp4Parser.Streaming.Extensions;
using SharpMp4Parser.Streaming.Input.H264.SpsPps;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace SharpMp4Parser.Streaming.Input.H264
{
    public abstract class H264NalConsumingTrack : AbstractH264Track
    {
        //private static Logger LOG = LoggerFactory.getLogger(H264NalConsumingTrack.class.getName());
        int max_dec_frame_buffering = 16;
        List<StreamingSample> decFrameBuffer = new List<StreamingSample>();
        List<StreamingSample> decFrameBuffer2 = new List<StreamingSample>();
        Dictionary<int, ByteBuffer> spsIdToSpsBytes = new Dictionary<int, ByteBuffer>();
        Dictionary<int, SeqParameterSet> spsIdToSps = new Dictionary<int, SeqParameterSet>();
        Dictionary<int, ByteBuffer> ppsIdToPpsBytes = new Dictionary<int, ByteBuffer>();
        Dictionary<int, PictureParameterSet> ppsIdToPps = new Dictionary<int, PictureParameterSet>();
        BlockingCollection<SeqParameterSet> spsForConfig = new BlockingCollection<SeqParameterSet>();

        int timescale = 0;
        int frametick = 0;
        bool configured;

        SampleDescriptionBox stsd;
        SeqParameterSet currentSeqParameterSet = null;
        PictureParameterSet currentPictureParameterSet = null;
        public List<ByteBuffer> buffered = new List<ByteBuffer>();
        public FirstVclNalDetector fvnd = null;
        public H264NalUnitHeader sliceNalUnitHeader;

        public H264NalConsumingTrack()
        {
        }

        public static H264NalUnitHeader getNalUnitHeader(ByteBuffer nal)
        {
            H264NalUnitHeader nalUnitHeader = new H264NalUnitHeader();
            int type = nal.get(0);
            nalUnitHeader.nal_ref_idc = (type >> 5) & 3;
            nalUnitHeader.nal_unit_type = type & 0x1f;

            return nalUnitHeader;
        }

        protected void consumeNal(ByteBuffer nal)
        {
            //LOG.finest("Consume NAL of " + nal.length + " bytes." + Hex.encodeHex(new byte[]{nal[0], nal[1], nal[2], nal[3], nal[4]}));
            H264NalUnitHeader nalUnitHeader = getNalUnitHeader(nal);
            switch (nalUnitHeader.nal_unit_type)
            {
                case H264NalUnitTypes.CODED_SLICE_NON_IDR:
                case H264NalUnitTypes.CODED_SLICE_DATA_PART_A:
                case H264NalUnitTypes.CODED_SLICE_DATA_PART_B:
                case H264NalUnitTypes.CODED_SLICE_DATA_PART_C:
                case H264NalUnitTypes.CODED_SLICE_IDR:
                    FirstVclNalDetector current = new FirstVclNalDetector(nal,
                            nalUnitHeader.nal_ref_idc, nalUnitHeader.nal_unit_type, spsIdToSps, ppsIdToPps);
                    sliceNalUnitHeader = nalUnitHeader;
                    if (fvnd != null && fvnd.isFirstInNew(current))
                    {
                        //LOG.debug("Wrapping up cause of first vcl nal is found");
                        pushSample(createSample(buffered, fvnd.sliceHeader, sliceNalUnitHeader), false, false);
                        buffered.Clear();
                    }
                    fvnd = current;
                    //System.err.println("" + nalUnitHeader.nal_unit_type);
                    buffered.Add(nal);
                    //LOG.debug("NAL Unit Type: " + nalUnitHeader.nal_unit_type + " " + fvnd.frame_num);
                    break;

                case H264NalUnitTypes.SEI:
                    if (fvnd != null)
                    {
                        //LOG.debug("Wrapping up cause of SEI after vcl marks new sample");
                        pushSample(createSample(buffered, fvnd.sliceHeader, sliceNalUnitHeader), false, false);
                        buffered.Clear();
                        fvnd = null;
                    }
                    //System.err.println("" + nalUnitHeader.nal_unit_type);
                    buffered.Add(nal);
                    break;

                case H264NalUnitTypes.AU_UNIT_DELIMITER:
                    if (fvnd != null)
                    {
                        //LOG.debug("Wrapping up cause of AU after vcl marks new sample");
                        pushSample(createSample(buffered, fvnd.sliceHeader, sliceNalUnitHeader), false, false);
                        buffered.Clear();
                        fvnd = null;
                    }
                    //System.err.println("" + nalUnitHeader.nal_unit_type);
                    buffered.Add(nal);
                    break;
                case H264NalUnitTypes.SEQ_PARAMETER_SET:
                    if (fvnd != null)
                    {
                        //LOG.debug("Wrapping up cause of SPS after vcl marks new sample");
                        pushSample(createSample(buffered, fvnd.sliceHeader, sliceNalUnitHeader), false, false);
                        buffered.Clear();
                        fvnd = null;
                    }
                    handleSPS(nal);
                    break;
                case 8:
                    if (fvnd != null)
                    {
                        //LOG.debug("Wrapping up cause of PPS after vcl marks new sample");
                        pushSample(createSample(buffered, fvnd.sliceHeader, sliceNalUnitHeader), false, false);
                        buffered.Clear();
                        fvnd = null;
                    }
                    handlePPS(nal);
                    break;
                case H264NalUnitTypes.END_OF_SEQUENCE:
                case H264NalUnitTypes.END_OF_STREAM:

                    return;

                case H264NalUnitTypes.SEQ_PARAMETER_SET_EXT:
                    throw new IOException("Sequence parameter set extension is not yet handled. Needs TLC.");

                default:
                    //  buffered.add(nal);
                    //LOG.warn("Unknown NAL unit type: " + nalUnitHeader.nal_unit_type);
                    break;

            }


        }

        protected void pushSample(StreamingSample ss, bool all, bool force)
        {
            if (ss != null)
            {
                decFrameBuffer.Add(ss);
            }
            if (all)
            {
                while (decFrameBuffer.Count > 0)
                {
                    pushSample(null, false, true);
                }
            }
            else
            {
                if ((decFrameBuffer.Count - 1 > max_dec_frame_buffering) || force)
                {
                    StreamingSample first = decFrameBuffer[0];
                    decFrameBuffer.RemoveAt(0);
                    PictureOrderCountType0SampleExtension poct0se = first.getSampleExtension< PictureOrderCountType0SampleExtension>(typeof(PictureOrderCountType0SampleExtension));
                    if (poct0se == null)
                    {
                        sampleSink.acceptSample(first, this);
                    }
                    else
                    {
                        int delay = 0;
                        foreach (StreamingSample streamingSample in decFrameBuffer)
                        {
                            if (poct0se.getPoc() > streamingSample.getSampleExtension< PictureOrderCountType0SampleExtension>(typeof(PictureOrderCountType0SampleExtension)).getPoc())
                            {
                                delay++;
                            }
                        }
                        foreach (StreamingSample streamingSample in decFrameBuffer2)
                        {
                            if (poct0se.getPoc() < streamingSample.getSampleExtension< PictureOrderCountType0SampleExtension>(typeof(PictureOrderCountType0SampleExtension)).getPoc())
                            {
                                delay--;
                            }
                        }
                        decFrameBuffer2.Add(first);
                        if (decFrameBuffer2.Count > max_dec_frame_buffering)
                        {
                            decFrameBuffer2[0].removeSampleExtension< PictureOrderCountType0SampleExtension>(typeof(PictureOrderCountType0SampleExtension));
                            decFrameBuffer2.RemoveAt(0);
                        }

                        first.addSampleExtension(CompositionTimeSampleExtension.create(delay * frametick));
                        //System.err.println("Adding sample");
                        sampleSink.acceptSample(first, this);
                    }
                }
            }

        }


        protected SampleFlagsSampleExtension createSampleFlagsSampleExtension(H264NalUnitHeader nu, SliceHeader sliceHeader)
        {
            SampleFlagsSampleExtension sampleFlagsSampleExtension = new SampleFlagsSampleExtension();
            if (nu.nal_ref_idc == 0)
            {
                sampleFlagsSampleExtension.setSampleIsDependedOn(2);
            }
            else
            {
                sampleFlagsSampleExtension.setSampleIsDependedOn(1);
            }
            if ((sliceHeader.slice_type == SliceHeader.SliceType.I) || (sliceHeader.slice_type == SliceHeader.SliceType.SI))
            {
                sampleFlagsSampleExtension.setSampleDependsOn(2);
            }
            else
            {
                sampleFlagsSampleExtension.setSampleDependsOn(1);
            }
            sampleFlagsSampleExtension.setSampleIsNonSyncSample(H264NalUnitTypes.CODED_SLICE_IDR != nu.nal_unit_type);
            return sampleFlagsSampleExtension;
        }

        protected PictureOrderCountType0SampleExtension createPictureOrderCountType0SampleExtension(SliceHeader sliceHeader)
        {
            if (sliceHeader.sps.pic_order_cnt_type == 0)
            {
                return new PictureOrderCountType0SampleExtension(
                        sliceHeader, decFrameBuffer.Count > 0 ?
                        decFrameBuffer[decFrameBuffer.Count - 1].getSampleExtension< PictureOrderCountType0SampleExtension>(typeof(PictureOrderCountType0SampleExtension)) :
                            null);
                /*            decFrameBuffer.add(ssi);
                            if (decFrameBuffer.size() - 1 > max_dec_frame_buffering) { // just added one
                                drainDecPictureBuffer(false);
                            }*/
            }
            else if (sliceHeader.sps.pic_order_cnt_type == 1)
            {
                throw new Exception("pic_order_cnt_type == 1 needs to be implemented");
            }
            else if (sliceHeader.sps.pic_order_cnt_type == 2)
            {
                return null; // no ctts
            }
            throw new Exception("I don't know sliceHeader.sps.pic_order_cnt_type of " + sliceHeader.sps.pic_order_cnt_type);
        }


        protected StreamingSample createSample(List<ByteBuffer> nals, SliceHeader sliceHeader, H264NalUnitHeader nu)
        {
            //LOG.debug("Create Sample");
            configure();
            if (timescale == 0 || frametick == 0)
            {
                throw new IOException("Frame Rate needs to be configured either by hand or by SPS before samples can be created");
            }


            StreamingSample ss = new StreamingSampleImpl(
                    nals,
                    frametick);
            ss.addSampleExtension(createSampleFlagsSampleExtension(nu, sliceHeader));
            ss.addSampleExtension(createPictureOrderCountType0SampleExtension(sliceHeader));

            return ss;
        }


        public void setFrametick(int frametick)
        {
            this.frametick = frametick;
        }

        private readonly object _syncRoot = new object();

        public void configure()
        {
            lock (_syncRoot)
            {
                if (!configured)
                {
                    SeqParameterSet sps;
                    try
                    {
                        if (!spsForConfig.TryTake(out sps, 5000))
                        {
                            //LOG.warn("Can't determine frame rate as no SPS became available in time");
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        //LOG.warn(e.getMessage());
                        //LOG.warn("Can't determine frame rate as no SPS became available in time");
                        return;
                    }

                    if (sps.pic_order_cnt_type == 0 || sps.pic_order_cnt_type == 1)
                    {
                        this.addTrackExtension(new CompositionTimeTrackExtension());
                    }

                    int width = (sps.pic_width_in_mbs_minus1 + 1) * 16;
                    int mult = 2;
                    if (sps.frame_mbs_only_flag)
                    {
                        mult = 1;
                    }
                    int height = 16 * (sps.pic_height_in_map_units_minus1 + 1) * mult;
                    if (sps.frame_cropping_flag)
                    {
                        int chromaArrayType = 0;
                        if (!sps.residual_color_transform_flag)
                        {
                            chromaArrayType = sps.chroma_format_idc.getId();
                        }
                        int cropUnitX = 1;
                        int cropUnitY = mult;
                        if (chromaArrayType != 0)
                        {
                            cropUnitX = sps.chroma_format_idc.getSubWidth();
                            cropUnitY = sps.chroma_format_idc.getSubHeight() * mult;
                        }

                        width -= cropUnitX * (sps.frame_crop_left_offset + sps.frame_crop_right_offset);
                        height -= cropUnitY * (sps.frame_crop_top_offset + sps.frame_crop_bottom_offset);
                    }


                    VisualSampleEntry visualSampleEntry = new VisualSampleEntry("avc1");
                    visualSampleEntry.setDataReferenceIndex(1);
                    visualSampleEntry.setDepth(24);
                    visualSampleEntry.setFrameCount(1);
                    visualSampleEntry.setHorizresolution(72);
                    visualSampleEntry.setVertresolution(72);
                    DimensionTrackExtension dte = this.getTrackExtension< DimensionTrackExtension>(typeof(DimensionTrackExtension));
                    if (dte == null)
                    {
                        this.addTrackExtension(new DimensionTrackExtension(width, height));
                    }
                    visualSampleEntry.setWidth(width);
                    visualSampleEntry.setHeight(height);

                    visualSampleEntry.setCompressorname("AVC Coding");

                    AvcConfigurationBox avcConfigurationBox = new AvcConfigurationBox();

                    avcConfigurationBox.setSequenceParameterSets(new List<ByteBuffer>(spsIdToSpsBytes.Values));
                    avcConfigurationBox.setPictureParameterSets(new List<ByteBuffer>(ppsIdToPpsBytes.Values));
                    avcConfigurationBox.setAvcLevelIndication(sps.level_idc);
                    avcConfigurationBox.setAvcProfileIndication(sps.profile_idc);
                    avcConfigurationBox.setBitDepthLumaMinus8(sps.bit_depth_luma_minus8);
                    avcConfigurationBox.setBitDepthChromaMinus8(sps.bit_depth_chroma_minus8);
                    avcConfigurationBox.setChromaFormat(sps.chroma_format_idc.getId());
                    avcConfigurationBox.setConfigurationVersion(1);
                    avcConfigurationBox.setLengthSizeMinusOne(3);


                    avcConfigurationBox.setProfileCompatibility(
                            (sps.constraint_set_0_flag ? 128 : 0) +
                                    (sps.constraint_set_1_flag ? 64 : 0) +
                                    (sps.constraint_set_2_flag ? 32 : 0) +
                                    (sps.constraint_set_3_flag ? 16 : 0) +
                                    (sps.constraint_set_4_flag ? 8 : 0) +
                                    (int)(sps.reserved_zero_2bits & 0x3)
                    );

                    visualSampleEntry.addBox(avcConfigurationBox);
                    stsd = new SampleDescriptionBox();
                    stsd.addBox(visualSampleEntry);

                    int _timescale;
                    int _frametick;
                    if (sps.vuiParams != null)
                    {
                        _timescale = sps.vuiParams.time_scale >> 1; // Not sure why, but I found this in several places, and it works...
                        _frametick = sps.vuiParams.num_units_in_tick;
                        if (_timescale == 0 || _frametick == 0)
                        {
                            //LOG.warn("vuiParams contain invalid values: time_scale: " + _timescale + " and frame_tick: " + _frametick + ". Setting frame rate to 25fps");
                            _timescale = 0;
                            _frametick = 0;
                        }
                        if (_frametick > 0)
                        {
                            if (_timescale / _frametick > 100)
                            {
                                //LOG.warn("Framerate is " + (_timescale / _frametick) + ". That is suspicious.");
                            }
                        }
                        else
                        {
                            //LOG.warn("Frametick is " + _frametick + ". That is suspicious.");
                        }
                        if (sps.vuiParams.bitstreamRestriction != null)
                        {
                            max_dec_frame_buffering = sps.vuiParams.bitstreamRestriction.max_dec_frame_buffering;
                        }
                    }
                    else
                    {
                        //LOG.warn("Can't determine frame rate as SPS does not contain vuiParama");
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
                    if (sps.pic_order_cnt_type == 0)
                    {
                        this.addTrackExtension(new CompositionTimeTrackExtension());
                    }
                    else if (sps.pic_order_cnt_type == 1)
                    {
                        throw new Exception("Have not yet imlemented pic_order_cnt_type 1");
                    }
                    configured = true;
                }
            }
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

        public override SampleDescriptionBox getSampleDescriptionBox()
        {
            configure();
            return stsd;
        }


        public override string getHandler()
        {
            return "vide";
        }

        public override string getLanguage()
        {
            return "eng";
        }

        protected void handlePPS(ByteBuffer nal)
        {
            ((ByteBuffer)nal).position(1);
            PictureParameterSet _pictureParameterSet = null;
            try
            {
                _pictureParameterSet = PictureParameterSet.read(nal);
                currentPictureParameterSet = _pictureParameterSet;

                ByteBuffer oldPpsSameId;
                ppsIdToPpsBytes.TryGetValue(_pictureParameterSet.pic_parameter_set_id, out oldPpsSameId);

                if (oldPpsSameId != null && !oldPpsSameId.Equals(nal))
                {
                    throw new Exception("OMG - I got two SPS with same ID but different settings! (AVC3 is the solution)");
                }
                else
                {
                    ppsIdToPpsBytes[_pictureParameterSet.pic_parameter_set_id] = nal;
                    ppsIdToPps[_pictureParameterSet.pic_parameter_set_id] = _pictureParameterSet;
                }
            }
            catch (IOException e)
            {
                throw new Exception("That's surprising to get IOException when working on ByteArrayInputStream", e);
            }


        }

        protected void handleSPS(ByteBuffer data)
        {
            ((ByteBuffer)data).position(1);
            try
            {
                SeqParameterSet _seqParameterSet = SeqParameterSet.read(data);

                currentSeqParameterSet = _seqParameterSet;

                ByteBuffer oldSpsSameId;
                spsIdToSpsBytes.TryGetValue(_seqParameterSet.seq_parameter_set_id, out oldSpsSameId);
                if (oldSpsSameId != null && !oldSpsSameId.Equals(data))
                {
                    throw new Exception("OMG - I got two SPS with same ID but different settings!");
                }
                else
                {
                    spsIdToSpsBytes[_seqParameterSet.seq_parameter_set_id] = data;
                    spsIdToSps[_seqParameterSet.seq_parameter_set_id] = _seqParameterSet;
                    spsForConfig.Add(_seqParameterSet);
                }
            }
            catch (IOException e)
            {
                throw new Exception("That's surprising to get IOException when working on ByteArrayInputStream", e);
            }

        }

        public override void close()
        {

        }

        public class FirstVclNalDetector
        {

            public readonly SliceHeader sliceHeader;
            int frame_num;
            int pic_parameter_set_id;
            bool field_pic_flag;
            bool bottom_field_flag;
            int nal_ref_idc;
            int pic_order_cnt_type;
            int delta_pic_order_cnt_bottom;
            int pic_order_cnt_lsb;
            int delta_pic_order_cnt_0;
            int delta_pic_order_cnt_1;
            bool idrPicFlag;
            int idr_pic_id;
            private Dictionary<int, SeqParameterSet> spsIdToSps;
            private Dictionary<int, PictureParameterSet> ppsIdToPps;

            public FirstVclNalDetector(ByteBuffer nal, int nal_ref_idc, int nal_unit_type, Dictionary<int, SeqParameterSet> spsIdToSps, Dictionary<int, PictureParameterSet> ppsIdToPps)
            {
                this.spsIdToSps = spsIdToSps;
                this.ppsIdToPps = ppsIdToPps;

                SliceHeader sh = new SliceHeader(nal, spsIdToSps, ppsIdToPps, nal_unit_type == 5);
                this.sliceHeader = sh;
                this.frame_num = sh.frame_num;
                this.pic_parameter_set_id = sh.pic_parameter_set_id;
                this.field_pic_flag = sh.field_pic_flag;
                this.bottom_field_flag = sh.bottom_field_flag;
                this.nal_ref_idc = nal_ref_idc;
                this.pic_order_cnt_type = spsIdToSps[ppsIdToPps[sh.pic_parameter_set_id].seq_parameter_set_id].pic_order_cnt_type;
                this.delta_pic_order_cnt_bottom = sh.delta_pic_order_cnt_bottom;
                this.pic_order_cnt_lsb = sh.pic_order_cnt_lsb;
                this.delta_pic_order_cnt_0 = sh.delta_pic_order_cnt_0;
                this.delta_pic_order_cnt_1 = sh.delta_pic_order_cnt_1;
                this.idr_pic_id = sh.idr_pic_id;
            }

            public bool isFirstInNew(FirstVclNalDetector nu)
            {
                if (nu.frame_num != frame_num)
                {
                    return true;
                }
                if (nu.pic_parameter_set_id != pic_parameter_set_id)
                {
                    return true;
                }
                if (nu.field_pic_flag != field_pic_flag)
                {
                    return true;
                }
                if (nu.field_pic_flag)
                {
                    if (nu.bottom_field_flag != bottom_field_flag)
                    {
                        return true;
                    }
                }
                if (nu.nal_ref_idc != nal_ref_idc)
                {
                    return true;
                }
                if (nu.pic_order_cnt_type == 0 && pic_order_cnt_type == 0)
                {
                    if (nu.pic_order_cnt_lsb != pic_order_cnt_lsb)
                    {
                        return true;
                    }
                    if (nu.delta_pic_order_cnt_bottom != delta_pic_order_cnt_bottom)
                    {
                        return true;
                    }
                }
                if (nu.pic_order_cnt_type == 1 && pic_order_cnt_type == 1)
                {
                    if (nu.delta_pic_order_cnt_0 != delta_pic_order_cnt_0)
                    {
                        return true;
                    }
                    if (nu.delta_pic_order_cnt_1 != delta_pic_order_cnt_1)
                    {
                        return true;
                    }
                }
                if (nu.idrPicFlag != idrPicFlag)
                {
                    return true;
                }
                if (nu.idrPicFlag && idrPicFlag)
                {
                    if (nu.idr_pic_id != idr_pic_id)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}