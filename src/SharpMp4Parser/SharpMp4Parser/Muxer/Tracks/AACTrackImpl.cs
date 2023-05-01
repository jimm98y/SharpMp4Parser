/*
 * Copyright 2012 castLabs GmbH, Berlin
 *
 * Licensed under the Apache License, Version 2.0 (the License);
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an AS IS BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part14;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Muxer.Tracks
{
    /**
     */
    public class AACTrackImpl : AbstractTrack
    {
        public static readonly Dictionary<int, int> SAMPLING_FREQUENCY_INDEX_MAP = new Dictionary<int, int>();
        static Dictionary<int, string> audioObjectTypes = new Dictionary<int, string>();

        static AACTrackImpl()
        {
            audioObjectTypes.Add(1, "AAC Main");
            audioObjectTypes.Add(2, "AAC LC (Low Complexity)");
            audioObjectTypes.Add(3, "AAC SSR (Scalable Sample Rate)");
            audioObjectTypes.Add(4, "AAC LTP (Long Term Prediction)");
            audioObjectTypes.Add(5, "SBR (Spectral Band Replication)");
            audioObjectTypes.Add(6, "AAC Scalable");
            audioObjectTypes.Add(7, "TwinVQ");
            audioObjectTypes.Add(8, "CELP (Code Excited Linear Prediction)");
            audioObjectTypes.Add(9, "HXVC (Harmonic Vector eXcitation Coding)");
            audioObjectTypes.Add(10, "Reserved");
            audioObjectTypes.Add(11, "Reserved");
            audioObjectTypes.Add(12, "TTSI (Text-To-Speech Interface)");
            audioObjectTypes.Add(13, "Main Synthesis");
            audioObjectTypes.Add(14, "Wavetable Synthesis");
            audioObjectTypes.Add(15, "General MIDI");
            audioObjectTypes.Add(16, "Algorithmic Synthesis and Audio Effects");
            audioObjectTypes.Add(17, "ER (Error Resilient) AAC LC");
            audioObjectTypes.Add(18, "Reserved");
            audioObjectTypes.Add(19, "ER AAC LTP");
            audioObjectTypes.Add(20, "ER AAC Scalable");
            audioObjectTypes.Add(21, "ER TwinVQ");
            audioObjectTypes.Add(22, "ER BSAC (Bit-Sliced Arithmetic Coding)");
            audioObjectTypes.Add(23, "ER AAC LD (Low Delay)");
            audioObjectTypes.Add(24, "ER CELP");
            audioObjectTypes.Add(25, "ER HVXC");
            audioObjectTypes.Add(26, "ER HILN (Harmonic and Individual Lines plus Noise)");
            audioObjectTypes.Add(27, "ER Parametric");
            audioObjectTypes.Add(28, "SSC (SinuSoidal Coding)");
            audioObjectTypes.Add(29, "PS (Parametric Stereo)");
            audioObjectTypes.Add(30, "MPEG Surround");
            audioObjectTypes.Add(31, "(Escape value)");
            audioObjectTypes.Add(32, "Layer-1");
            audioObjectTypes.Add(33, "Layer-2");
            audioObjectTypes.Add(34, "Layer-3");
            audioObjectTypes.Add(35, "DST (Direct Stream Transfer)");
            audioObjectTypes.Add(36, "ALS (Audio Lossless)");
            audioObjectTypes.Add(37, "SLS (Scalable LosslesS)");
            audioObjectTypes.Add(38, "SLS non-core");
            audioObjectTypes.Add(39, "ER AAC ELD (Enhanced Low Delay)");
            audioObjectTypes.Add(40, "SMR (Symbolic Music Representation) Simple");
            audioObjectTypes.Add(41, "SMR Main");
            audioObjectTypes.Add(42, "USAC (Unified Speech and Audio Coding) (no SBR)");
            audioObjectTypes.Add(43, "SAOC (Spatial Audio Object Coding)");
            audioObjectTypes.Add(44, "LD MPEG Surround");
            audioObjectTypes.Add(45, "USAC");

            SAMPLING_FREQUENCY_INDEX_MAP.Add(96000, 0);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(88200, 1);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(64000, 2);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(48000, 3);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(44100, 4);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(32000, 5);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(24000, 6);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(22050, 7);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(16000, 8);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(12000, 9);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(11025, 10);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(8000, 11);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(0x0, 96000);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(0x1, 88200);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(0x2, 64000);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(0x3, 48000);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(0x4, 44100);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(0x5, 32000);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(0x6, 24000);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(0x7, 22050);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(0x8, 16000);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(0x9, 12000);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(0xa, 11025);
            SAMPLING_FREQUENCY_INDEX_MAP.Add(0xb, 8000);
        }

        TrackMetaData trackMetaData = new TrackMetaData();
        private AudioSampleEntry audioSampleEntry;
        private long[] decTimes;
        private AdtsHeader firstHeader;

        private int bufferSizeDB;
        private long maxBitRate;
        private long avgBitRate;

        private DataSource dataSource;
        private List<Sample> samples;

        public AACTrackImpl(DataSource dataSource) : this(dataSource, "eng")
        { }

        public AACTrackImpl(DataSource dataSource, string lang) : base(dataSource.ToString())
        {
            this.dataSource = dataSource;
            samples = new List<Sample>();
            firstHeader = readSamples(dataSource);

            double packetsPerSecond = (double)firstHeader.sampleRate / 1024.0;
            double duration = samples.Count / packetsPerSecond;

            long dataSize = 0;
            Queue<int> queue = new Queue<int>();
            foreach (Sample sample in samples)
            {
                int size = (int)sample.getSize();
                dataSize += size;
                queue.Enqueue(size);
                while (queue.Count > packetsPerSecond)
                {
                    queue.Dequeue();
                }
                if (queue.Count == (int)packetsPerSecond)
                {
                    int currSize = 0;
                    foreach (int aQueue in queue)
                    {
                        currSize += aQueue;
                    }
                    double currBitrate = 8.0 * currSize / queue.Count * packetsPerSecond;
                    if (currBitrate > maxBitRate)
                    {
                        maxBitRate = (int)currBitrate;
                    }
                }
            }

            avgBitRate = (int)(8 * dataSize / duration);

            bufferSizeDB = 1536; /* TODO: Calcultate this somehow! */

            audioSampleEntry = new AudioSampleEntry("mp4a");
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
            decoderConfigDescriptor.setBufferSizeDB(bufferSizeDB);
            decoderConfigDescriptor.setMaxBitRate(maxBitRate);
            decoderConfigDescriptor.setAvgBitRate(avgBitRate);

            AudioSpecificConfig audioSpecificConfig = new AudioSpecificConfig();
            audioSpecificConfig.setOriginalAudioObjectType(2); // AAC LC
            audioSpecificConfig.setSamplingFrequencyIndex(firstHeader.sampleFrequencyIndex);
            audioSpecificConfig.setChannelConfiguration(firstHeader.channelconfig);
            decoderConfigDescriptor.setAudioSpecificInfo(audioSpecificConfig);

            descriptor.setDecoderConfigDescriptor(decoderConfigDescriptor);

            esds.setEsDescriptor(descriptor);
            audioSampleEntry.addBox(esds);

            trackMetaData.setCreationTime(new DateTime());
            trackMetaData.setModificationTime(new DateTime());
            trackMetaData.setLanguage(lang);
            trackMetaData.setVolume(1);
            trackMetaData.setTimescale(firstHeader.sampleRate); // Audio tracks always use sampleRate as timescale
            decTimes = new long[samples.Count];
            Arrays.fill(decTimes, 1024);
        }

        public void close()
        {
            // doing everything to get rid of references to memory mapped things
            dataSource.close();
        }

        public List<SampleEntry> getSampleEntries()
        {
            return new List<SampleEntry>() { audioSampleEntry };
        }

        public long[] getSampleDurations()
        {
            return decTimes;
        }

        public List<CompositionTimeToSample.Entry> getCompositionTimeEntries()
        {
            return null;
        }

        public long[] getSyncSamples()
        {
            return null;
        }

        public List<SampleDependencyTypeBox.Entry> getSampleDependencies()
        {
            return null;
        }

        public TrackMetaData getTrackMetaData()
        {
            return trackMetaData;
        }

        public string getHandler()
        {
            return "soun";
        }

        public List<Sample> getSamples()
        {
            return samples;
        }

        public SubSampleInformationBox getSubsampleInformationBox()
        {
            return null;
        }

        private AdtsHeader readADTSHeader(DataSource channel)
        {
            AdtsHeader hdr = new AdtsHeader();
            ByteBuffer bb = ByteBuffer.allocate(7);
            while (bb.position() < 7)
            {
                if (channel.read(bb) == -1)
                {
                    return null;
                }
            }

            BitReaderBuffer brb = new BitReaderBuffer((ByteBuffer)((Java.Buffer)bb).rewind());
            int syncword = brb.readBits(12); // A
            if (syncword != 0xfff)
            {
                throw new Exception("Expected Start Word 0xfff");
            }
            hdr.mpegVersion = brb.readBits(1); // B
            hdr.layer = brb.readBits(2); // C
            hdr.protectionAbsent = brb.readBits(1); // D
            hdr.profile = brb.readBits(2) + 1;  // E
                                                //System.err.println(String.format("Profile %s", audioObjectTypes.get(hdr.profile)));
            hdr.sampleFrequencyIndex = brb.readBits(4);
            hdr.sampleRate = SAMPLING_FREQUENCY_INDEX_MAP[hdr.sampleFrequencyIndex]; // F
            brb.readBits(1); // G
            hdr.channelconfig = brb.readBits(3); // H
            hdr.original = brb.readBits(1); // I
            hdr.home = brb.readBits(1); // J
            hdr.copyrightedStream = brb.readBits(1); // K
            hdr.copyrightStart = brb.readBits(1); // L
            hdr.frameLength = brb.readBits(13); // M
                                                //System.err.println(hdr.frameLength);
            hdr.bufferFullness = brb.readBits(11); // 54
            hdr.numAacFramesPerAdtsFrame = brb.readBits(2) + 1; // 56
            if (hdr.numAacFramesPerAdtsFrame != 1)
            {
                throw new Exception("This muxer can only work with 1 AAC frame per ADTS frame");
            }
            if (hdr.protectionAbsent == 0)
            {
                channel.read(ByteBuffer.allocate(2));
            }
            return hdr;
        }

        public class AdtsSample : Sample
        {
            private AudioSampleEntry audioSampleEntry;
            private DataSource dataSource;
            private long frameSize;
            private long currentPosition;

            public AdtsSample(AudioSampleEntry audioSampleEntry, DataSource dataSource, long frameSize, long currentPosition)
            {
                this.audioSampleEntry = audioSampleEntry;
                this.dataSource = dataSource;
                this.frameSize = frameSize;
                this.currentPosition = currentPosition;
            }

            public void writeTo(WritableByteChannel channel)
            {
                dataSource.transferTo(currentPosition, frameSize, channel);
            }

            public long getSize()
            {
                return frameSize;
            }

            public ByteBuffer asByteBuffer()
            {
                try
                {
                    return dataSource.map(currentPosition, frameSize);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            public SampleEntry getSampleEntry()
            {
                return audioSampleEntry;
            }
        }

        private AdtsHeader readSamples(DataSource channel)
        {
            AdtsHeader first = null;
            AdtsHeader hdr;

            while ((hdr = readADTSHeader(channel)) != null)
            {
                if (first == null)
                {
                    first = hdr;
                }

                long currentPosition = channel.position();
                long frameSize = hdr.frameLength - hdr.getSize();
                samples.Add(new AdtsSample(audioSampleEntry, dataSource, frameSize, currentPosition));

                channel.position(channel.position() + hdr.frameLength - hdr.getSize());
            }
            return first;
        }

        public override string ToString()
        {
            return "AACTrackImpl{" +
                    "sampleRate=" + firstHeader.sampleRate +
                    ", channelconfig=" + firstHeader.channelconfig +
                    '}';
        }

        public class AdtsHeader
        {
            public int sampleFrequencyIndex;
            public int mpegVersion;
            public int layer;
            public int protectionAbsent;
            public int profile;
            public int sampleRate;
            public int channelconfig;
            public int original;
            public int home;
            public int copyrightedStream;
            public int copyrightStart;
            public int frameLength;
            public int bufferFullness;
            public int numAacFramesPerAdtsFrame;

            public int getSize()
            {
                return 7 + (protectionAbsent == 0 ? 2 : 0);
            }
        }
    }
}