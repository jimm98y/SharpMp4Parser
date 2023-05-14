using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part14;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.Java;
using SharpMp4Parser.Streaming.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SharpMp4Parser.Streaming.Input.AAC
{
    public class AdtsAacStreamingTrack : AbstractStreamingTrack /*, Callable<Void> */
    {
        private static Dictionary<int, int> samplingFrequencyIndexMap = new Dictionary<int, int>();
        //private static Logger LOG = LoggerFactory.getLogger(AdtsAacStreamingTrack.class.getName());

        static AdtsAacStreamingTrack()
        {
            samplingFrequencyIndexMap.Add(96000, 0);
            samplingFrequencyIndexMap.Add(88200, 1);
            samplingFrequencyIndexMap.Add(64000, 2);
            samplingFrequencyIndexMap.Add(48000, 3);
            samplingFrequencyIndexMap.Add(44100, 4);
            samplingFrequencyIndexMap.Add(32000, 5);
            samplingFrequencyIndexMap.Add(24000, 6);
            samplingFrequencyIndexMap.Add(22050, 7);
            samplingFrequencyIndexMap.Add(16000, 8);
            samplingFrequencyIndexMap.Add(12000, 9);
            samplingFrequencyIndexMap.Add(11025, 10);
            samplingFrequencyIndexMap.Add(8000, 11);
            samplingFrequencyIndexMap.Add(0x0, 96000);
            samplingFrequencyIndexMap.Add(0x1, 88200);
            samplingFrequencyIndexMap.Add(0x2, 64000);
            samplingFrequencyIndexMap.Add(0x3, 48000);
            samplingFrequencyIndexMap.Add(0x4, 44100);
            samplingFrequencyIndexMap.Add(0x5, 32000);
            samplingFrequencyIndexMap.Add(0x6, 24000);
            samplingFrequencyIndexMap.Add(0x7, 22050);
            samplingFrequencyIndexMap.Add(0x8, 16000);
            samplingFrequencyIndexMap.Add(0x9, 12000);
            samplingFrequencyIndexMap.Add(0xa, 11025);
            samplingFrequencyIndexMap.Add(0xb, 8000);
        }

        SemaphoreSlim gotFirstSample = new SemaphoreSlim(1);
        SampleDescriptionBox stsd = null;
        private ByteStream input;
        private bool closed;
        private AdtsHeader firstHeader;
        private string lang = "und";
        private long avgBitrate;
        private long maxBitrate;


        public AdtsAacStreamingTrack(ByteStream input, long avgBitrate, long maxBitrate)
        {
            this.avgBitrate = avgBitrate;
            this.maxBitrate = maxBitrate;
            Debug.Assert(input != null);
            this.input = input;
            DefaultSampleFlagsTrackExtension defaultSampleFlagsTrackExtension = new DefaultSampleFlagsTrackExtension();
            defaultSampleFlagsTrackExtension.setIsLeading(2);
            defaultSampleFlagsTrackExtension.setSampleDependsOn(2);
            defaultSampleFlagsTrackExtension.setSampleIsDependedOn(2);
            defaultSampleFlagsTrackExtension.setSampleHasRedundancy(2);
            defaultSampleFlagsTrackExtension.setSampleIsNonSyncSample(false);
            this.addTrackExtension(defaultSampleFlagsTrackExtension);
        }

        private static AdtsHeader readADTSHeader(ByteStream fis)
        {
            AdtsHeader hdr = new AdtsHeader();
            int x = fis.read(); // A
            int syncword = x << 4;
            x = fis.read();
            if (x == -1)
            {
                return null;
            }
            syncword += (x >> 4);
            if (syncword != 0xfff)
            {
                throw new IOException("Expected Start Word 0xfff");
            }
            hdr.mpegVersion = (x & 0x8) >> 3;
            hdr.layer = (x & 0x6) >> 1;
            ; // C
            hdr.protectionAbsent = (x & 0x1);  // D

            x = fis.read();


            hdr.profile = ((x & 0xc0) >> 6) + 1;  // E
                                                  //System.err.println(String.format("Profile %s", audioObjectTypes.get(hdr.profile)));
            hdr.sampleFrequencyIndex = (x & 0x3c) >> 2;
            Debug.Assert(hdr.sampleFrequencyIndex != 15);
            hdr.sampleRate = samplingFrequencyIndexMap[hdr.sampleFrequencyIndex]; // F
            hdr.channelconfig = (x & 1) << 2; // H

            x = fis.read();
            hdr.channelconfig += (x & 0xc0) >> 6;

            hdr.original = (x & 0x20) >> 5; // I
            hdr.home = (x & 0x10) >> 4; // J
            hdr.copyrightedStream = (x & 0x8) >> 3; // K
            hdr.copyrightStart = (x & 0x4) >> 2; // L
            hdr.frameLength = (x & 0x3) << 9;  // M

            x = fis.read();
            hdr.frameLength += (x << 3);

            x = fis.read();
            hdr.frameLength += (x & 0xe0) >> 5;

            hdr.bufferFullness = (x & 0x1f) << 6;

            x = fis.read();
            hdr.bufferFullness += (x & 0xfc) >> 2;
            hdr.numAacFramesPerAdtsFrame = ((x & 0x3)) + 1;


            if (hdr.numAacFramesPerAdtsFrame != 1)
            {
                throw new IOException("This muxer can only work with 1 AAC frame per ADTS frame");
            }
            if (hdr.protectionAbsent == 0)
            {
                int crc1 = fis.read();
                int crc2 = fis.read();
            }
            return hdr;
        }

        public bool isClosed()
        {
            return closed;
        }

        private readonly object _syncRoot = new object();

        public override SampleDescriptionBox getSampleDescriptionBox()
        {
            lock (_syncRoot)
            {
                waitForFirstSample();
                if (stsd == null)
                {
                    stsd = new SampleDescriptionBox();
                    AudioSampleEntry audioSampleEntry = new AudioSampleEntry("mp4a");
                    if (firstHeader.channelconfig == 7)
                    {
                        audioSampleEntry.setChannelCount(8);
                    }
                    else
                    {
                        audioSampleEntry.setChannelCount(firstHeader.channelconfig);
                    }
                    audioSampleEntry.setSampleRate(firstHeader.sampleRate);
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
                    audioSpecificConfig.setSamplingFrequencyIndex(firstHeader.sampleFrequencyIndex);
                    audioSpecificConfig.setChannelConfiguration(firstHeader.channelconfig);
                    decoderConfigDescriptor.setAudioSpecificInfo(audioSpecificConfig);

                    descriptor.setDecoderConfigDescriptor(decoderConfigDescriptor);

                    esds.setEsDescriptor(descriptor);

                    audioSampleEntry.addBox(esds);
                    stsd.addBox(audioSampleEntry);

                }
                return stsd;
            }
        }

        void waitForFirstSample()
        {
            try
            {
                gotFirstSample.Wait();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override long getTimescale()
        {
            waitForFirstSample();
            return firstHeader.sampleRate;
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
            closed = true;
            input.close();
        }

        public void call()
        {
            AdtsHeader header;
            int i = 1;
            try
            {
                while ((header = readADTSHeader(input)) != null)
                {
                    if (firstHeader == null)
                    {
                        firstHeader = header;
                        gotFirstSample.Release();
                    }
                    byte[] frame = new byte[header.frameLength - header.getSize()];
                    int n = 0;
                    while (n < frame.Length)
                    {
                        int count = input.read(frame, n, frame.Length - n);
                        if (count < 0)
                            throw new EndOfStreamException();
                        n += count;
                    }
                    //System.err.println("Sample " + i++);
                    sampleSink.acceptSample(new StreamingSampleImpl(ByteBuffer.wrap(frame), 1024), this);
                }
            }
            catch (EndOfStreamException)
            {
                //LOG.info("Done reading ADTS AAC file.");
            }
        }

        public override string ToString()
        {
            TrackIdTrackExtension trackIdTrackExtension = this.getTrackExtension< TrackIdTrackExtension>(typeof(TrackIdTrackExtension));
            if (trackIdTrackExtension != null)
            {
                return "AdtsAacStreamingTrack{trackId=" + trackIdTrackExtension.getTrackId() + "}";
            }
            else
            {
                return "AdtsAacStreamingTrack{}";
            }
        }
    }
}
