using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using SharpMp4Parser.Muxer.Tracks.H264.Parsing.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace SharpMp4Parser.Muxer.Tracks.H264
{
    /**
     * The <code>H264TrackImpl</code> creates a <code>Track</code> from an H.264
     * Annex B file.
     */
    public class H264TrackImpl : AbstractH26XTrack
    {
        //private static Logger LOG = LoggerFactory.getLogger(H264TrackImpl.class.getName());

        Dictionary<int, ByteBuffer> spsIdToSpsBytes = new Dictionary<int, ByteBuffer>();
        Dictionary<int, SeqParameterSet> spsIdToSps = new Dictionary<int, SeqParameterSet>();
        Dictionary<int, ByteBuffer> ppsIdToPpsBytes = new Dictionary<int, ByteBuffer>();
        Dictionary<int, PictureParameterSet> ppsIdToPps = new Dictionary<int, PictureParameterSet>();

        SeqParameterSet firstSeqParameterSet = null;
        PictureParameterSet firstPictureParameterSet = null;
        SeqParameterSet currentSeqParameterSet = null;
        PictureParameterSet currentPictureParameterSet = null;
        RangeStartMap<int, ByteBuffer> seqParameterRangeMap = new RangeStartMap<int, ByteBuffer>();
        RangeStartMap<int, ByteBuffer> pictureParameterRangeMap = new RangeStartMap<int, ByteBuffer>();
        int frameNrInGop = 0;
        int[] pictureOrderCounts = new int[0];
        int prevPicOrderCntLsb = 0;
        int prevPicOrderCntMsb = 0;
        long psize = 0;
        long pcount = 0;
        long bsize = 0;
        long bcount = 0;
        long isize = 0;
        long icount = 0;
        private List<Sample> samples;
        private int width;
        private int height;
        private long timescale;
        private int frametick;
        private SEIMessage seiMessage;
        private bool determineFrameRate = true;
        private string lang = "eng";

        VisualSampleEntry visualSampleEntry;

        protected override SampleEntry getCurrentSampleEntry()
        {
            return visualSampleEntry;
        }

        /**
         * Creates a new <code>Track</code> object from a raw H264 source (<code>DataSource dataSource1</code>).
         * Whenever the timescale and frametick are set to negative value (e.g. -1) the H264TrackImpl
         * tries to detect the frame rate.
         * Typically values for <code>timescale</code> and <code>frametick</code> are:
         * <ul>
         * <li>23.976 FPS: timescale = 24000; frametick = 1001</li>
         * <li>25 FPS: timescale = 25; frametick = 1</li>
         * <li>29.97 FPS: timescale = 30000; frametick = 1001</li>
         * <li>30 FPS: timescale = 30; frametick = 1</li>
         * </ul>
         *
         * @param dataSource the source file of the H264 samples
         * @param lang       language of the movie (in doubt: use "eng")
         * @param timescale  number of time units (ticks) in one second
         * @param frametick  number of time units (ticks) that pass while showing exactly one frame
         * @throws IOException in case of problems whiel reading from the <code>DataSource</code>
         */
        public H264TrackImpl(DataSource dataSource, string lang, long timescale, int frametick) : base(dataSource)
        {
            this.lang = lang;
            this.timescale = timescale; //e.g. 23976
            this.frametick = frametick;
            if ((timescale > 0) && (frametick > 0))
            {
                this.determineFrameRate = false;
            }

            parse(new LookAhead(dataSource));
        }


        public H264TrackImpl(DataSource dataSource, string lang) : this(dataSource, lang, -1, -1)
        { }

        public H264TrackImpl(DataSource dataSource) : this(dataSource, "eng")
        { }

        //public static void main(string[] args)
        //{
        //    new H264TrackImpl(new FileDataSourceImpl("C:\\dev\\mp4parser\\tos.264"));
        //}

        public static H264NalUnitHeader getNalUnitHeader(ByteBuffer nal)
        {
            H264NalUnitHeader nalUnitHeader = new H264NalUnitHeader();
            int type = nal.get(0);
            nalUnitHeader.nal_ref_idc = (type >> 5) & 3;
            nalUnitHeader.nal_unit_type = type & 0x1f;

            return nalUnitHeader;
        }



        private void parse(LookAhead la)
        {

            visualSampleEntry = new VisualSampleEntry("avc1");
            visualSampleEntry.setDataReferenceIndex(1);
            visualSampleEntry.setDepth(24);
            visualSampleEntry.setFrameCount(1);
            visualSampleEntry.setHorizresolution(72);
            visualSampleEntry.setVertresolution(72);
            visualSampleEntry.setWidth(width);
            visualSampleEntry.setHeight(height);
            visualSampleEntry.setCompressorname("AVC Coding");

            samples = new List<Sample>();
            if (!readSamples(la))
            {
                throw new IOException();
            }

            if (!readVariables())
            {
                throw new IOException();
            }


            AvcConfigurationBox avcConfigurationBox = new AvcConfigurationBox();

            avcConfigurationBox.setSequenceParameterSets(new List<ByteBuffer>(spsIdToSpsBytes.Values));
            avcConfigurationBox.setPictureParameterSets(new List<ByteBuffer>(ppsIdToPpsBytes.Values));
            avcConfigurationBox.setAvcLevelIndication(firstSeqParameterSet.level_idc);
            avcConfigurationBox.setAvcProfileIndication(firstSeqParameterSet.profile_idc);
            avcConfigurationBox.setBitDepthLumaMinus8(firstSeqParameterSet.bit_depth_luma_minus8);
            avcConfigurationBox.setBitDepthChromaMinus8(firstSeqParameterSet.bit_depth_chroma_minus8);
            avcConfigurationBox.setChromaFormat(firstSeqParameterSet.chroma_format_idc.getId());
            avcConfigurationBox.setConfigurationVersion(1);
            avcConfigurationBox.setLengthSizeMinusOne(3);


            avcConfigurationBox.setProfileCompatibility(
                    (firstSeqParameterSet.constraint_set_0_flag ? 128 : 0) +
                            (firstSeqParameterSet.constraint_set_1_flag ? 64 : 0) +
                            (firstSeqParameterSet.constraint_set_2_flag ? 32 : 0) +
                            (firstSeqParameterSet.constraint_set_3_flag ? 16 : 0) +
                            (firstSeqParameterSet.constraint_set_4_flag ? 8 : 0) +
                            (int)(firstSeqParameterSet.reserved_zero_2bits & 0x3)
            );

            visualSampleEntry.addBox(avcConfigurationBox);

            trackMetaData.setCreationTime(new DateTime());
            trackMetaData.setModificationTime(new DateTime());
            trackMetaData.setLanguage(lang);
            trackMetaData.setTimescale(timescale);
            trackMetaData.setWidth(width);
            trackMetaData.setHeight(height);
        }

        public override List<SampleEntry> getSampleEntries()
        {
            return new List<SampleEntry>() { visualSampleEntry };
        }

        public override string getHandler()
        {
            return "vide";
        }

        public override IList<Sample> getSamples()
        {
            return samples;
        }

        private bool readVariables()
        {
            width = (firstSeqParameterSet.pic_width_in_mbs_minus1 + 1) * 16;
            int mult = 2;
            if (firstSeqParameterSet.frame_mbs_only_flag)
            {
                mult = 1;
            }
            height = 16 * (firstSeqParameterSet.pic_height_in_map_units_minus1 + 1) * mult;
            if (firstSeqParameterSet.frame_cropping_flag)
            {
                int chromaArrayType = 0;
                if (!firstSeqParameterSet.residual_color_transform_flag)
                {
                    chromaArrayType = firstSeqParameterSet.chroma_format_idc.getId();
                }
                int cropUnitX = 1;
                int cropUnitY = mult;
                if (chromaArrayType != 0)
                {
                    cropUnitX = firstSeqParameterSet.chroma_format_idc.getSubWidth();
                    cropUnitY = firstSeqParameterSet.chroma_format_idc.getSubHeight() * mult;
                }

                width -= cropUnitX * (firstSeqParameterSet.frame_crop_left_offset + firstSeqParameterSet.frame_crop_right_offset);
                height -= cropUnitY * (firstSeqParameterSet.frame_crop_top_offset + firstSeqParameterSet.frame_crop_bottom_offset);
            }
            return true;
        }

        public class FirstVclNalDetector
        {

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

            public FirstVclNalDetector(ByteBuffer nal, int nal_ref_idc, int nal_unit_type)
            {
                InputStream bs = cleanBuffer(new ByteBufferBackedInputStream(nal));
                SliceHeader sh = new SliceHeader(bs, spsIdToSps, ppsIdToPps, nal_unit_type == 5);
                this.frame_num = sh.frame_num;
                this.pic_parameter_set_id = sh.pic_parameter_set_id;
                this.field_pic_flag = sh.field_pic_flag;
                this.bottom_field_flag = sh.bottom_field_flag;
                this.nal_ref_idc = nal_ref_idc;
                this.pic_order_cnt_type = spsIdToSps.get(ppsIdToPps.get(sh.pic_parameter_set_id).seq_parameter_set_id).pic_order_cnt_type;
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

        private bool readSamples(LookAhead la)
        {
            List<ByteBuffer> buffered = new List<ByteBuffer>();

            ByteBuffer nal;

            FirstVclNalDetector fvnd = null;

            bool nal_loop = true;
            while (nal_loop && (nal = findNextNal(la)) != null)
            {
                H264NalUnitHeader nalUnitHeader = getNalUnitHeader(nal);

                switch (nalUnitHeader.nal_unit_type)
                {
                    case H264NalUnitTypes.CODED_SLICE_NON_IDR:
                    case H264NalUnitTypes.CODED_SLICE_DATA_PART_A:
                    case H264NalUnitTypes.CODED_SLICE_DATA_PART_B:
                    case H264NalUnitTypes.CODED_SLICE_DATA_PART_C:
                    case H264NalUnitTypes.CODED_SLICE_IDR:
                        FirstVclNalDetector current = new FirstVclNalDetector(nal,
                                nalUnitHeader.nal_ref_idc, nalUnitHeader.nal_unit_type);
                        if (fvnd != null && fvnd.isFirstInNew(current))
                        {
                            //LOG.debug("Wrapping up cause of first vcl nal is found");
                            createSample(buffered);
                        }
                        fvnd = current;
                        //System.err.println("" + nalUnitHeader.nal_unit_type);
                        buffered.Add((ByteBuffer)((Java.Buffer)nal).rewind());
                        //log.finer("NAL Unit Type: " + nalUnitHeader.nal_unit_type + " " + fvnd.frame_num);
                        break;

                    case H264NalUnitTypes.SEI:
                        if (fvnd != null)
                        {
                            //LOG.debug("Wrapping up cause of SEI after vcl marks new sample");
                            createSample(buffered);
                            fvnd = null;
                        }
                        seiMessage = new SEIMessage(cleanBuffer(new ByteBufferBackedInputStream(nal)), currentSeqParameterSet);
                        //System.err.println("" + nalUnitHeader.nal_unit_type);
                        buffered.Add(nal);
                        break;

                    case H264NalUnitTypes.AU_UNIT_DELIMITER:
                        if (fvnd != null)
                        {
                            //LOG.debug("Wrapping up cause of AU after vcl marks new sample");
                            createSample(buffered);
                            fvnd = null;
                        }
                        //System.err.println("" + nalUnitHeader.nal_unit_type);
                        buffered.Add(nal);
                        break;
                    case H264NalUnitTypes.SEQ_PARAMETER_SET:
                        if (fvnd != null)
                        {
                            //LOG.debug("Wrapping up cause of SPS after vcl marks new sample");
                            createSample(buffered);
                            fvnd = null;
                        }
                        handleSPS((ByteBuffer)((Java.Buffer)nal).rewind());
                        break;
                    case 8:
                        if (fvnd != null)
                        {
                            //LOG.debug("Wrapping up cause of PPS after vcl marks new sample");
                            createSample(buffered);
                            fvnd = null;
                        }
                        handlePPS((ByteBuffer)((Java.Buffer)nal).rewind());
                        break;
                    case H264NalUnitTypes.END_OF_SEQUENCE:
                    case H264NalUnitTypes.END_OF_STREAM:
                        nal_loop = false;
                        break;

                    case H264NalUnitTypes.SEQ_PARAMETER_SET_EXT:
                        throw new Exception("Sequence parameter set extension is not yet handled. Needs TLC.");

                    default:
                        //  buffered.add(nal);
                        //LOG.warn("Unknown NAL unit type: " + nalUnitHeader.nal_unit_type);
                        break;

                }


            }
            if (buffered.Count > 0)
            {
                createSample(buffered);
            }
            calcCtts();

            decodingTimes = new long[samples.Count];
            Arrays.fill(decodingTimes, frametick);

            return true;
        }

        public void calcCtts()
        {

            int pTime = 0;
            int lastPoc = -1;
            for (int j = 0; j < pictureOrderCounts.Length; j++)
            {
                int minIndex = 0;
                int minValue = int.MaxValue;
                for (int i = Math.Max(0, j - 128); i < Math.Min(pictureOrderCounts.Length, j + 128); i++)
                {
                    if (pictureOrderCounts[i] > lastPoc && pictureOrderCounts[i] < minValue)
                    {
                        minIndex = i;
                        minValue = pictureOrderCounts[i];
                    }
                }
                lastPoc = pictureOrderCounts[minIndex];
                pictureOrderCounts[minIndex] = pTime++;
            }
            for (int i = 0; i < pictureOrderCounts.Length; i++)
            {
                ctts.add(new CompositionTimeToSample.Entry(1, pictureOrderCounts[i] - i));
            }

            pictureOrderCounts = new int[0];
        }

        long getSize(List<ByteBuffer> buffered)
        {
            long i = 0;
            foreach (ByteBuffer byteBuffer in buffered)
            {
                i += byteBuffer.remaining();
            }
            return i;
        }

        private void createSample(List<ByteBuffer> buffered)
        {

            SampleDependencyTypeBox.Entry sampleDependency = new SampleDependencyTypeBox.Entry(0);

            bool IdrPicFlag = false;
            H264NalUnitHeader nu = null;
            ByteBuffer slice = null;
            foreach (ByteBuffer nal in buffered)
            {
                H264NalUnitHeader _nu = getNalUnitHeader(nal);

                switch (_nu.nal_unit_type)
                {
                    case H264NalUnitTypes.CODED_SLICE_IDR:
                        IdrPicFlag = true;
                        nu = _nu;
                        slice = nal;
                        break; ;
                    case H264NalUnitTypes.CODED_SLICE_NON_IDR:
                    case H264NalUnitTypes.CODED_SLICE_DATA_PART_A:
                    case H264NalUnitTypes.CODED_SLICE_DATA_PART_B:
                    case H264NalUnitTypes.CODED_SLICE_DATA_PART_C:
                        nu = _nu;
                        slice = nal;
                        break;
                }
            }
            if (nu == null)
            {
                //LOG.warn("Sample without Slice");
                return;
            }
            Debug.Assert(slice != null);

            if (IdrPicFlag)
            {
                calcCtts();

            }
            // cleans the buffer we just added
            InputStream bs = cleanBuffer(new ByteBufferBackedInputStream(slice));
            SliceHeader sh = new SliceHeader(bs, spsIdToSps, ppsIdToPps, IdrPicFlag);

            if ((sh.slice_type == SliceHeader.SliceType.I) || (sh.slice_type == SliceHeader.SliceType.SI))
            {
                isize += getSize(buffered);
                icount++;
            }
            else if ((sh.slice_type == SliceHeader.SliceType.P) || (sh.slice_type == SliceHeader.SliceType.SP))
            {
                psize += getSize(buffered);
                pcount++;
            }
            else if ((sh.slice_type == SliceHeader.SliceType.B))
            {
                bsize += getSize(buffered);
                bcount++;
            }
            else
            {
                throw new Exception("_sdjlfd");
            }

            if (nu.nal_ref_idc == 0)
            {
                sampleDependency.setSampleIsDependedOn(2);
            }
            else
            {
                sampleDependency.setSampleIsDependedOn(1);
            }
            if ((sh.slice_type == SliceHeader.SliceType.I) || (sh.slice_type == SliceHeader.SliceType.SI))
            {
                sampleDependency.setSampleDependsOn(2);
            }
            else
            {
                sampleDependency.setSampleDependsOn(1);
            }
            Sample bb = createSampleObject(buffered);
            //                    LOG.fine("Adding sample with size " + bb.capacity() + " and header " + sh);
            buffered.Clear();

            if (seiMessage == null || seiMessage.n_frames == 0)
            {
                frameNrInGop = 0;
            }

            if (sh.sps.pic_order_cnt_type == 0)
            {
                int max_pic_order_count_lsb = (1 << (sh.sps.log2_max_pic_order_cnt_lsb_minus4 + 4));
                // System.out.print(" pic_order_cnt_lsb " + pic_order_cnt_lsb + " " + max_pic_order_count);
                int picOrderCountLsb = sh.pic_order_cnt_lsb;
                int picOrderCntMsb;
                if ((picOrderCountLsb < prevPicOrderCntLsb) &&
                        ((prevPicOrderCntLsb - picOrderCountLsb) >= (max_pic_order_count_lsb / 2)))
                {
                    picOrderCntMsb = prevPicOrderCntMsb + max_pic_order_count_lsb;
                }
                else if ((picOrderCountLsb > prevPicOrderCntLsb) &&
                        ((picOrderCountLsb - prevPicOrderCntLsb) > (max_pic_order_count_lsb / 2)))
                {
                    picOrderCntMsb = prevPicOrderCntMsb - max_pic_order_count_lsb;
                }
                else
                {
                    picOrderCntMsb = prevPicOrderCntMsb;
                }

                pictureOrderCounts = Mp4Arrays.copyOfAndAppend(pictureOrderCounts, picOrderCntMsb + picOrderCountLsb);
                prevPicOrderCntLsb = picOrderCountLsb;
                prevPicOrderCntMsb = picOrderCntMsb;


            }
            else if (sh.sps.pic_order_cnt_type == 1)
            {
                /*if (seiMessage != null && seiMessage.clock_timestamp_flag) {
                    offset = seiMessage.n_frames - frameNrInGop;
                } else if (seiMessage != null && seiMessage.removal_delay_flag) {
                    offset = seiMessage.dpb_removal_delay / 2;
                }

                if (seiMessage == null) {
                    LOG.warning("CTS timing in ctts box is most likely not OK");
                }*/
                throw new Exception("pic_order_cnt_type == 1 needs to be implemented");
            }
            else if (sh.sps.pic_order_cnt_type == 2)
            {
                pictureOrderCounts = Mp4Arrays.copyOfAndAppend(pictureOrderCounts, samples.Count);
            }

            sdtp.Add(sampleDependency);
            frameNrInGop++;

            samples.Add(bb);
            if (IdrPicFlag)
            { // IDR Picture
                stss.Add(samples.Count);
            }
        }


        private void handlePPS(ByteBuffer data)
        {
            InputStream input = new ByteBufferBackedInputStream(data);
            input.read();

            PictureParameterSet _pictureParameterSet = PictureParameterSet.read(input);
            if (firstPictureParameterSet == null)
            {
                firstPictureParameterSet = _pictureParameterSet;
            }

            currentPictureParameterSet = _pictureParameterSet;


            ByteBuffer oldPpsSameId = ppsIdToPpsBytes[_pictureParameterSet.pic_parameter_set_id];

            ((Java.Buffer)data).rewind();
            if (oldPpsSameId != null && !oldPpsSameId.Equals(data))
            {
                throw new Exception("OMG - I got two SPS with same ID but different settings! (AVC3 is the solution)");
            }
            else
            {
                if (oldPpsSameId == null)
                {
                    pictureParameterRangeMap.Add(samples.Count, data);
                }
                ppsIdToPpsBytes.Add(_pictureParameterSet.pic_parameter_set_id, data);
                ppsIdToPps.Add(_pictureParameterSet.pic_parameter_set_id, _pictureParameterSet);
            }
        }

        private void handleSPS(ByteBuffer data)
        {
            InputStream spsInputStream = cleanBuffer(new ByteBufferBackedInputStream(data));
            spsInputStream.read();
            SeqParameterSet _seqParameterSet = SeqParameterSet.read(spsInputStream);
            if (firstSeqParameterSet == null)
            {
                firstSeqParameterSet = _seqParameterSet;
                configureFramerate();
            }
            currentSeqParameterSet = _seqParameterSet;

            ((Java.Buffer)data).rewind();
            ByteBuffer oldSpsSameId = spsIdToSpsBytes[_seqParameterSet.seq_parameter_set_id];
            if (oldSpsSameId != null && !oldSpsSameId.Equals(data))
            {
                throw new Exception("OMG - I got two SPS with same ID but different settings!");
            }
            else
            {
                if (oldSpsSameId != null)
                {
                    seqParameterRangeMap.Add(samples.Count, data);
                }
                spsIdToSpsBytes.Add(_seqParameterSet.seq_parameter_set_id, data);
                spsIdToSps.Add(_seqParameterSet.seq_parameter_set_id, _seqParameterSet);
            }
        }

        private void configureFramerate()
        {
            if (determineFrameRate)
            {
                if (firstSeqParameterSet.vuiParams != null)
                {
                    timescale = firstSeqParameterSet.vuiParams.time_scale >> 1; // Not sure why, but I found this in several places, and it works...
                    frametick = firstSeqParameterSet.vuiParams.num_units_in_tick;
                    if (timescale == 0 || frametick == 0)
                    {
                        //LOG.warn("vuiParams contain invalid values: time_scale: " + timescale + " and frame_tick: " + frametick + ". Setting frame rate to 25fps");
                        timescale = 90000;
                        frametick = 3600;
                    }

                    if (timescale / frametick > 100)
                    {
                        //LOG.warn("Framerate is " + (timescale / frametick) + ". That is suspicious.");
                    }
                }
                else
                {
                    //LOG.warn("Can't determine frame rate. Guessing 25 fps");
                    timescale = 90000;
                    frametick = 3600;
                }
            }
        }

        public class ByteBufferBackedInputStream : ByteArrayInputStream
        {
            private readonly ByteBuffer buf;

            public ByteBufferBackedInputStream(ByteBuffer buf)
            {
                // make a coy of the buffer
                this.buf = buf.duplicate();
            }

            public override int read()
            {
                if (!buf.hasRemaining())
                {
                    return -1;
                }
                return buf.get() & 0xFF;
            }

            public int read(byte[] bytes, int off, int len)
            {
                if (!buf.hasRemaining())
                {
                    return -1;
                }

                len = Math.Min(len, buf.remaining());
                buf.get(bytes, off, len);
                return len;
            }
        }
    }
}
