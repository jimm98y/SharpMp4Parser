using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part14;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;
using System.IO;

namespace SharpMp4Parser.Muxer.Tracks
{
    /**
     * MPEG V1 Layer 3 Audio. Does not support IDv3 or any other tags. Only raw stream of MP3 frames.
     * See <a href="http://mpgedit.org/mpgedit/mpeg_format/mpeghdr.htm">http://mpgedit.org/mpgedit/mpeg_format/mpeghdr.htm</a>
     * for stream format description.
     *
     * @author Roman Elizarov
     */
    public class MP3TrackImpl : AbstractTrack
    {
        private const int MPEG_V1 = 0x3; // only support V1
        private const int MPEG_L3 = 1; // only support L3
        private static readonly int[] SAMPLE_RATE = { 44100, 48000, 32000, 0 };
        private static readonly int[] BIT_RATE = { 0, 32000, 40000, 48000, 56000, 64000, 80000, 96000, 112000, 128000, 160000, 192000, 224000, 256000, 320000, 0 };
        private const int SAMPLES_PER_FRAME = 1152; // Samples per L3 frame

        private const int ES_OBJECT_TYPE_INDICATION = 0x6b;
        private const int ES_STREAM_TYPE = 5;
        private readonly DataSource dataSource;

        TrackMetaData trackMetaData = new TrackMetaData();
        MP3Header firstHeader;

        long maxBitRate;
        long avgBitRate;

        private List<Sample> samples;
        private long[] durations;
        AudioSampleEntry audioSampleEntry;

        public MP3TrackImpl(DataSource channel) : this(channel, "eng")
        { }

        public MP3TrackImpl(DataSource dataSource, string lang) : base(dataSource.toString())
        {
            this.dataSource = dataSource;
            samples = new List<Sample>();
            firstHeader = readSamples(dataSource);

            double packetsPerSecond = (double)firstHeader.sampleRate / SAMPLES_PER_FRAME;
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
                    double currBitRate = 8.0 * currSize / queue.Count * packetsPerSecond;
                    if (currBitRate > maxBitRate)
                    {
                        maxBitRate = (int)currBitRate;
                    }
                }
            }

            avgBitRate = (int)(8 * dataSize / duration);

            audioSampleEntry = new AudioSampleEntry("mp4a");
            audioSampleEntry.setChannelCount(firstHeader.channelCount);
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
            decoderConfigDescriptor.setObjectTypeIndication(ES_OBJECT_TYPE_INDICATION);
            decoderConfigDescriptor.setStreamType(ES_STREAM_TYPE);
            decoderConfigDescriptor.setMaxBitRate(maxBitRate);
            decoderConfigDescriptor.setAvgBitRate(avgBitRate);
            descriptor.setDecoderConfigDescriptor(decoderConfigDescriptor);

            ByteBuffer data = descriptor.serialize();
            esds.setData(data);
            audioSampleEntry.addBox(esds);

            trackMetaData.setCreationTime(new DateTime());
            trackMetaData.setModificationTime(new DateTime());
            trackMetaData.setLanguage(lang);
            trackMetaData.setVolume(1);
            trackMetaData.setTimescale(firstHeader.sampleRate); // Audio tracks always use sampleRate as timescale
            durations = new long[samples.Count];
            Arrays.fill(durations, SAMPLES_PER_FRAME);
        }

        public void close()
        {
            dataSource.close();
        }

        public List<SampleEntry> getSampleEntries()
        {
            return new List<SampleEntry>() { audioSampleEntry };
        }

        public long[] getSampleDurations()
        {
            return durations;
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

        private MP3Header readSamples(DataSource channel)
        {
            MP3Header first = null;
            while (true)
            {
                long pos = channel.position();
                MP3Header hdr;
                if ((hdr = readMP3Header(channel)) == null)
                    break;
                if (first == null)
                    first = hdr;
                channel.position(pos);
                ByteBuffer data = ByteBuffer.allocate(hdr.getFrameLength());
                channel.read(data);
                ((Java.Buffer)data).rewind();
                samples.Add(new SampleImpl(data, audioSampleEntry));
            }
            return first;
        }

        private MP3Header readMP3Header(DataSource channel)
        {
            MP3Header hdr = new MP3Header();
            ByteBuffer bb = ByteBuffer.allocate(4);
            while (bb.position() < 4)
            {
                if (channel.read(bb) == -1)
                {
                    return null;
                }
            }
            if (bb.get(0) == 0x54 && bb.get(1) == 0x41 && bb.get(2) == 0x47)
            {
                // encounter id3 tag. That's the end of the file.
                return null;
            }

            BitReaderBuffer brb = new BitReaderBuffer((ByteBuffer)((Java.Buffer)bb).rewind());
            int sync = brb.readBits(11); // A
            if (sync != 0x7ff)
                throw new IOException("Expected Start Word 0x7ff");
            hdr.mpegVersion = brb.readBits(2); // B

            if (hdr.mpegVersion != MPEG_V1)
                throw new IOException("Expected MPEG Version 1 (ISO/IEC 11172-3)");

            hdr.layer = brb.readBits(2); // C

            if (hdr.layer != MPEG_L3)
                throw new IOException("Expected Layer III");

            hdr.protectionAbsent = brb.readBits(1); // D

            hdr.bitRateIndex = brb.readBits(4); // E
            hdr.bitRate = BIT_RATE[hdr.bitRateIndex];
            if (hdr.bitRate == 0)
                throw new IOException("Unexpected (free/bad) bit rate");

            hdr.sampleFrequencyIndex = brb.readBits(2);
            hdr.sampleRate = SAMPLE_RATE[hdr.sampleFrequencyIndex]; // F
            if (hdr.sampleRate == 0)
                throw new IOException("Unexpected (reserved) sample rate frequency");

            hdr.padding = brb.readBits(1); // G padding
            brb.readBits(1); // H private

            hdr.channelMode = brb.readBits(2); // H
            hdr.channelCount = hdr.channelMode == 3 ? 1 : 2;
            return hdr;
        }

        public override string toString()
        {
            return "MP3TrackImpl";
        }

        public class MP3Header
        {
            public int mpegVersion;
            public int layer;
            public int protectionAbsent;

            public int bitRateIndex;
            public int bitRate;

            public int sampleFrequencyIndex;
            public int sampleRate;

            public int padding;

            public int channelMode;
            public int channelCount;

            public int getFrameLength()
            {
                return 144 * bitRate / sampleRate + padding;
            }
        }
    }
}
