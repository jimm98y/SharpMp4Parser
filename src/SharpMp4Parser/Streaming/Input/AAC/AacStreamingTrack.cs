﻿using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part14;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.Java;
using SharpMp4Parser.Streaming.Extensions;

namespace SharpMp4Parser.Streaming.Input.AAC
{
    /**
     * Created by Jimm98y on 5/20/2023.
     */
    public class AacStreamingTrack : AbstractStreamingTrack
    {
        SampleDescriptionBox stsd = null;
        //private bool closed;
        private string lang = "und";
        private long avgBitrate;
        private long maxBitrate;
        private int channelCount;
        private int sampleRate;
        private int sampleFrequencyIndex;

        public AacStreamingTrack(long avgBitrate, long maxBitrate, int channelCount, int sampleRate, int frequencyIndex)
        {
            this.avgBitrate = avgBitrate;
            this.maxBitrate = maxBitrate;
            this.channelCount = channelCount;
            this.sampleRate = sampleRate;
            this.sampleFrequencyIndex = frequencyIndex;

            DefaultSampleFlagsTrackExtension defaultSampleFlagsTrackExtension = new DefaultSampleFlagsTrackExtension();
            defaultSampleFlagsTrackExtension.setIsLeading(2);
            defaultSampleFlagsTrackExtension.setSampleDependsOn(2);
            defaultSampleFlagsTrackExtension.setSampleIsDependedOn(2);
            defaultSampleFlagsTrackExtension.setSampleHasRedundancy(2);
            defaultSampleFlagsTrackExtension.setSampleIsNonSyncSample(false);
            this.addTrackExtension(defaultSampleFlagsTrackExtension);
        }

        public override SampleDescriptionBox getSampleDescriptionBox()
        {
            if (stsd == null)
            {
                stsd = new SampleDescriptionBox();
                AudioSampleEntry audioSampleEntry = new AudioSampleEntry("mp4a");
                    
                audioSampleEntry.setChannelCount(this.channelCount);
                audioSampleEntry.setSampleRate(this.sampleRate);
                audioSampleEntry.setDataReferenceIndex(1);
                audioSampleEntry.setSampleSize(16);


                ESDescriptorBox esds = new ESDescriptorBox();
                ESDescriptor descriptor = new ESDescriptor();
                descriptor.setEsId(0);

                SLConfigDescriptor slConfigDescriptor = new SLConfigDescriptor();
                slConfigDescriptor.setPredefined(2);
                descriptor.setSlConfigDescriptor(slConfigDescriptor);

                DecoderConfigDescriptor decoderConfigDescriptor = new DecoderConfigDescriptor();
                decoderConfigDescriptor.setObjectTypeIndication(0x40);
                decoderConfigDescriptor.setStreamType(5);
                decoderConfigDescriptor.setBufferSizeDB(1536);
                decoderConfigDescriptor.setMaxBitRate(maxBitrate);
                decoderConfigDescriptor.setAvgBitRate(avgBitrate);

                AudioSpecificConfig audioSpecificConfig = new AudioSpecificConfig();
                audioSpecificConfig.setOriginalAudioObjectType(2); // AAC LC
                audioSpecificConfig.setSamplingFrequencyIndex(this.sampleFrequencyIndex);
                audioSpecificConfig.setChannelConfiguration(this.channelCount);
                decoderConfigDescriptor.setAudioSpecificInfo(audioSpecificConfig);

                descriptor.setDecoderConfigDescriptor(decoderConfigDescriptor);

                esds.setEsDescriptor(descriptor);

                audioSampleEntry.addBox(esds);
                stsd.addBox(audioSampleEntry);
            }

            return stsd;
        }

        public override long getTimescale()
        {
            return this.sampleRate;
        }

        public override string getHandler()
        {
            return "soun";
        }

        public override string getLanguage()
        {
            return lang;
        }

        public void setLanguage(string lang)
        {
            this.lang = lang;
        }

        public override void close()
        {
            //closed = true;
        }

        public void ProcessFrame(byte[] frame)
        {
            sampleSink.acceptSample(new StreamingSampleImpl(ByteBuffer.wrap(frame), 1024), this);
        }

        public override string ToString()
        {
            TrackIdTrackExtension trackIdTrackExtension = this.getTrackExtension<TrackIdTrackExtension>(typeof(TrackIdTrackExtension));
            if (trackIdTrackExtension != null)
            {
                return "AacStreamingTrack{trackId=" + trackIdTrackExtension.getTrackId() + "}";
            }
            else
            {
                return "AacStreamingTrack{}";
            }
        }
    }
}
