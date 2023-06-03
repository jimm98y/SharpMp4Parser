using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        private SequenceParameterSetRbsp parsedSPS;
        private VideoParameterSet parsedVPS;
        private PictureParameterSetRbsp parsedPPS;

        protected override ByteBuffer findNextNal(LookAhead la)
        {
            try
            {
                while (!la.nextFourEquals0001())
                {
                    la.discardByte();
                }
                la.discardNext4AndMarkStart();

                while (!la.nextFourEquals0000or0001orEof(allZeroIsEndOfSequence))
                {
                    la.discardByte();
                }
                return la.getNal();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public H265TrackImpl(DataSource dataSource) : base(dataSource)
        {
            List<ByteBuffer> nals = new List<ByteBuffer>();
            LookAhead la = new LookAhead(dataSource);
            ByteBuffer nal;
            bool vclNalUnitSeenInAU = false;
            bool isIdr = true;

            visualSampleEntry = new VisualSampleEntry("hvc1");
            visualSampleEntry.setDataReferenceIndex(1);
            visualSampleEntry.setDepth(24);
            visualSampleEntry.setFrameCount(1);
            visualSampleEntry.setHorizresolution(72);
            visualSampleEntry.setVertresolution(72);
            visualSampleEntry.setWidth(640);
            visualSampleEntry.setHeight(480);
            visualSampleEntry.setCompressorname("HEVC Coding");

            while ((nal = findNextNal(la)) != null)
            {
                H265NalUnitHeader unitHeader = getNalUnitHeader(nal);
                Debug.WriteLine($"NAL Type: {unitHeader.nalUnitType}");
                //
                if (vclNalUnitSeenInAU)
                { // we need at least 1 VCL per AU
                  // This branch checks if we encountered the start of a samples/AU
                    if (isVcl(unitHeader))
                    {
                        if ((nal.get(2) & 0xff) != 0)
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
                        byte[] bPPS = Streaming.Input.AnnexBUtils.RemoveEmulationPreventionBytes(pps[pps.Count - 1].array().Skip(pps[pps.Count - 1].arrayOffset() + 2).Take(pps[pps.Count - 1].limit()).ToArray());
                        parsedPPS = new PictureParameterSetRbsp(ByteBuffer.wrap(bPPS));
                        Java.LOG.debug("Stored PPS");
                        break;
                    case H265NalUnitTypes.NAL_TYPE_VPS_NUT:
                        nal.position(0);
                        vps.Add(nal.slice());
                        ((Java.Buffer)nal).position(2); 
                        byte[] bVPS = Streaming.Input.AnnexBUtils.RemoveEmulationPreventionBytes(vps[vps.Count - 1].array().Skip(vps[vps.Count - 1].arrayOffset() + 2).Take(vps[vps.Count - 1].limit()).ToArray());
                        parsedVPS = new VideoParameterSet(ByteBuffer.wrap(bVPS));
                        Java.LOG.debug("Stored VPS"); 
                        break;
                    case H265NalUnitTypes.NAL_TYPE_SPS_NUT:
                        nal.position(0);
                        sps.Add(nal.slice());
                        ((Java.Buffer)nal).position(2); 
                        byte[] bSPS = Streaming.Input.AnnexBUtils.RemoveEmulationPreventionBytes(sps[sps.Count - 1].array().Skip(sps[sps.Count - 1].arrayOffset() + 2).Take(sps[sps.Count - 1].limit()).ToArray());
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
                        nal.position(0);
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
                            break;
                        default:
                            isIdr = false;
                            break;
                    }
                }

                vclNalUnitSeenInAU |= isVcl(unitHeader);

            }
            visualSampleEntry = fillSampleEntry();
            decodingTimes = new long[samples.Count];
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

        private VisualSampleEntry fillSampleEntry()
        {            
            HevcConfigurationBox hevcConfigurationBox = new HevcConfigurationBox();
            hevcConfigurationBox.getHevcDecoderConfigurationRecord().setGeneral_profile_idc(parsedSPS.general_profile_idc);
            hevcConfigurationBox.getHevcDecoderConfigurationRecord().setConfigurationVersion(1);
            hevcConfigurationBox.getHevcDecoderConfigurationRecord().setChromaFormat(parsedSPS.chroma_format_idc);
            hevcConfigurationBox.getHevcDecoderConfigurationRecord().setGeneral_level_idc(parsedVPS.general_level_idc);
            hevcConfigurationBox.getHevcDecoderConfigurationRecord().setGeneral_profile_compatibility_flags(parsedVPS.general_profile_compatibility_flags);
            hevcConfigurationBox.getHevcDecoderConfigurationRecord().setGeneral_constraint_indicator_flags(parsedVPS.general_profile_constraint_indicator_flags);
            hevcConfigurationBox.getHevcDecoderConfigurationRecord().setLengthSizeMinusOne(3); // 4 bytes size block inserted in between NAL units

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

            // correct order is VPS, SPS, PPS. Other order produced ffmpeg errors such as "VPS 0 does not exist" and "SPS 0 does not exist."
            hevcConfigurationBox.getArrays().AddRange(Arrays.asList(vpsArray, spsArray, ppsArray));

            visualSampleEntry.addBox(hevcConfigurationBox);

            visualSampleEntry.setWidth(parsedSPS.pic_width_in_luma_samples);
            visualSampleEntry.setHeight(parsedSPS.pic_height_in_luma_samples);

            trackMetaData.setCreationTime(DateTime.UtcNow);
            trackMetaData.setModificationTime(DateTime.UtcNow);
            trackMetaData.setLanguage("enu");
            trackMetaData.setTimescale(25); 
            trackMetaData.setWidth(parsedSPS.pic_width_in_luma_samples);
            trackMetaData.setHeight(parsedSPS.pic_height_in_luma_samples);

            return visualSampleEntry;
        }

        public void wrapUp(List<ByteBuffer> nals, ref bool vclNalUnitSeenInAU, ref bool isIdr)
        {
            samples.Add(createSampleObject(nals));
            Java.LOG.error("Create AU from " + nals.Count + " NALs");
            if (isIdr)
            {
                Java.LOG.debug("  IDR");
            }

            vclNalUnitSeenInAU = false;
            isIdr = true;
            nals.Clear();
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

        bool isVcl(H265NalUnitHeader nalUnitHeader)
        {
            return nalUnitHeader.nalUnitType >= 0 && nalUnitHeader.nalUnitType <= 31;
        }

        /**
         * Builds an MP4 sample from a list of NALs. Each NAL will be preceded by its
         * 4 byte (unit32) length.
         *
         * @param nals a list of NALs that form the sample
         * @return sample as it appears in the MP4 file
         */
        //protected override Sample createSampleObject(List<ByteBuffer> nals)
        //{
        //    ByteBuffer[] data = new ByteBuffer[nals.Count * 2];

        //    for (int i = 0; i < nals.Count; i++)
        //    {
        //        byte[] sizeInfo = new byte[4];
        //        ByteBuffer sizeBuf = ByteBuffer.wrap(sizeInfo);
        //        int size = nals[i].remaining();

        //        sizeBuf.putInt(size);

        //        var header = ByteBuffer.wrap(sizeInfo);
        //        var headerBytes = header.array().Skip(header.arrayOffset() + header.position()).Take(header.limit()).ToArray();

        //        var nalBytes = nals[i].array().Skip(nals[i].arrayOffset() + nals[i].position()).Take(nals[i].limit()).ToArray();

        //        Debug.WriteLine("Header start: {0:X2} {1:X2} {2:X2} {3:X2}", headerBytes[0], headerBytes[1], headerBytes[2], headerBytes[3]);
        //        Debug.WriteLine("Header end:   {0:X2} {1:X2} {2:X2} {3:X2}", headerBytes[headerBytes.Length - 4], headerBytes[headerBytes.Length - 3], headerBytes[headerBytes.Length - 2], headerBytes[headerBytes.Length - 1]);
        //        Debug.WriteLine("NAL start:    {0:X2} {1:X2} {2:X2} {3:X2}", nalBytes[0], nalBytes[1], nalBytes[2], nalBytes[3]);
        //        Debug.WriteLine("NAL end:      {0:X2} {1:X2} {2:X2} {3:X2}", nalBytes[nalBytes.Length - 4], nalBytes[nalBytes.Length - 3], nalBytes[nalBytes.Length - 2], nalBytes[nalBytes.Length - 1]);
        //        Debug.WriteLine("");

        //        //data[2 * i] = header;
        //        data[2 * i] = ByteBuffer.wrap(headerBytes);
        //        //data[2 * i + 1] = nals[i];
        //        data[2 * i + 1] = ByteBuffer.wrap(nalBytes);
        //    }

        //    return new SampleImpl(data, getCurrentSampleEntry());
        //}
    }
}