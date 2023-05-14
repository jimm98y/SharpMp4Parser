/*  
 * Copyright 2008 CoreMedia AG, Hamburg
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

using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System.IO;
using System.Linq;

namespace SharpMp4Parser.IsoParser.Boxes.SampleEntry
{
    /**
     * <h1>4cc = "{@value #TYPE1}" || "{@value #TYPE2} || "{@value #TYPE3} || "{@value #TYPE4} || "{@value #TYPE5} || "{@value #TYPE7} || "{@value #TYPE8} || "{@value #TYPE9} || "{@value #TYPE10} || "{@value #TYPE11} || "{@value #TYPE12} || "{@value #TYPE13}"</h1>
     * Contains basic information about the audio samples in this track. Format-specific information
     * is appened as boxes after the data described in ISO/IEC 14496-12 chapter 8.16.2.
     */
    public sealed class AudioSampleEntry : AbstractSampleEntry
    {
        //private static Logger LOG = LoggerFactory.getLogger(AudioSampleEntry.class);

        public const string TYPE1 = "samr";
        public const string TYPE2 = "sawb";
        public const string TYPE3 = "mp4a";
        public const string TYPE4 = "drms";
        public const string TYPE5 = "alac";
        public const string TYPE7 = "owma";
        public const string TYPE8 = "ac-3"; /* ETSI TS 102 366 1.2.1 Annex F */
        public const string TYPE9 = "ec-3"; /* ETSI TS 102 366 1.2.1 Annex F */
        public const string TYPE10 = "mlpa";
        public const string TYPE11 = "dtsl";
        public const string TYPE12 = "dtsh";
        public const string TYPE13 = "dtse";

        /**
         * Identifier for an encrypted audio track.
         *
         * @see ProtectionSchemeInformationBox
         */
        public const string TYPE_ENCRYPTED = "enca";

        private int channelCount;
        private int sampleSize;
        private long sampleRate;
        private int soundVersion;
        private int compressionId;
        private int packetSize;
        private long samplesPerPacket;
        private long bytesPerPacket;
        private long bytesPerFrame;
        private long bytesPerSample;

        private int reserved1;
        private long reserved2;
        private byte[] soundVersion2Data;


        public AudioSampleEntry(string type) : base(type)
        { }

        public void setType(string type)
        {
            this.type = type;
        }

        public int getChannelCount()
        {
            return channelCount;
        }

        public void setChannelCount(int channelCount)
        {
            this.channelCount = channelCount;
        }

        public int getSampleSize()
        {
            return sampleSize;
        }

        public void setSampleSize(int sampleSize)
        {
            this.sampleSize = sampleSize;
        }

        public long getSampleRate()
        {
            return sampleRate;
        }

        public void setSampleRate(long sampleRate)
        {
            this.sampleRate = sampleRate;
        }

        public int getSoundVersion()
        {
            return soundVersion;
        }

        public void setSoundVersion(int soundVersion)
        {
            this.soundVersion = soundVersion;
        }

        public int getCompressionId()
        {
            return compressionId;
        }

        public void setCompressionId(int compressionId)
        {
            this.compressionId = compressionId;
        }

        public int getPacketSize()
        {
            return packetSize;
        }

        public void setPacketSize(int packetSize)
        {
            this.packetSize = packetSize;
        }

        public long getSamplesPerPacket()
        {
            return samplesPerPacket;
        }

        public void setSamplesPerPacket(long samplesPerPacket)
        {
            this.samplesPerPacket = samplesPerPacket;
        }

        public long getBytesPerPacket()
        {
            return bytesPerPacket;
        }

        public void setBytesPerPacket(long bytesPerPacket)
        {
            this.bytesPerPacket = bytesPerPacket;
        }

        public long getBytesPerFrame()
        {
            return bytesPerFrame;
        }

        public void setBytesPerFrame(long bytesPerFrame)
        {
            this.bytesPerFrame = bytesPerFrame;
        }

        public long getBytesPerSample()
        {
            return bytesPerSample;
        }

        public void setBytesPerSample(long bytesPerSample)
        {
            this.bytesPerSample = bytesPerSample;
        }

        public byte[] getSoundVersion2Data()
        {
            return soundVersion2Data;
        }

        public void setSoundVersion2Data(byte[] soundVersion2Data)
        {
            this.soundVersion2Data = soundVersion2Data;
        }

        public int getReserved1()
        {
            return reserved1;
        }

        public void setReserved1(int reserved1)
        {
            this.reserved1 = reserved1;
        }

        public long getReserved2()
        {
            return reserved2;
        }

        public void setReserved2(long reserved2)
        {
            this.reserved2 = reserved2;
        }

        class CustomBox : Box
        {
            private ByteBuffer owmaSpecifics;
            private long remaining;

            public CustomBox(ByteBuffer owmaSpecifics, long remaining)
            {
                this.owmaSpecifics = owmaSpecifics;
                this.remaining = remaining;
            }

            public long getSize()
            {
                return remaining;
            }

            public string getType()
            {
                return "----";
            }

            public void getBox(ByteStream writableByteChannel)
            {
                ((Buffer)owmaSpecifics).rewind();
                writableByteChannel.write(owmaSpecifics);
            }
        }

        public override void parse(ByteStream dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            ByteBuffer content = ByteBuffer.allocate(28);
            dataSource.read(content);
            ((Buffer)content).position(6);
            dataReferenceIndex = IsoTypeReader.readUInt16(content);

            // 8 bytes already parsed
            //reserved bits - used by qt
            soundVersion = IsoTypeReader.readUInt16(content);

            //reserved
            reserved1 = IsoTypeReader.readUInt16(content);
            reserved2 = IsoTypeReader.readUInt32(content);

            channelCount = IsoTypeReader.readUInt16(content);
            sampleSize = IsoTypeReader.readUInt16(content);
            //reserved bits - used by qt
            compressionId = IsoTypeReader.readUInt16(content);
            //reserved bits - used by qt
            packetSize = IsoTypeReader.readUInt16(content);
            //sampleRate = in.readFixedPoint1616();
            sampleRate = IsoTypeReader.readUInt32(content);
            if (!type.Equals("mlpa"))
            {
                sampleRate = (long)(ulong)sampleRate >> 16;
            }

            //more qt stuff - see http://mp4v2.googlecode.com/svn-history/r388/trunk/src/atom_sound.cpp

            if (soundVersion == 1)
            {
                ByteBuffer appleStuff = ByteBuffer.allocate(16);
                dataSource.read(appleStuff);
                appleStuff.rewind();
                samplesPerPacket = IsoTypeReader.readUInt32(appleStuff);
                bytesPerPacket = IsoTypeReader.readUInt32(appleStuff);
                bytesPerFrame = IsoTypeReader.readUInt32(appleStuff);
                bytesPerSample = IsoTypeReader.readUInt32(appleStuff);
            }
            if (soundVersion == 2)
            {
                ByteBuffer appleStuff = ByteBuffer.allocate(36);
                dataSource.read(appleStuff);
                appleStuff.rewind();
                samplesPerPacket = IsoTypeReader.readUInt32(appleStuff);
                bytesPerPacket = IsoTypeReader.readUInt32(appleStuff);
                bytesPerFrame = IsoTypeReader.readUInt32(appleStuff);
                bytesPerSample = IsoTypeReader.readUInt32(appleStuff);
                soundVersion2Data = new byte[20];
                appleStuff.get(soundVersion2Data);
            }

            if ("owma".Equals(type))
            {
                //LOG.error("owma");
                long remaining = contentSize - 28
                        - (soundVersion == 1 ? 16 : 0)
                        - (soundVersion == 2 ? 36 : 0);
                ByteBuffer owmaSpecifics = ByteBuffer.allocate(CastUtils.l2i(remaining));
                dataSource.read(owmaSpecifics);

                addBox(new CustomBox(owmaSpecifics, remaining));
            }
            else
            {
                initContainer(dataSource,
                        contentSize - 28
                                - (soundVersion == 1 ? 16 : 0)
                                - (soundVersion == 2 ? 36 : 0), boxParser);
            }
        }

        public override void getBox(ByteStream writableByteChannel)
        {
            writableByteChannel.write(getHeader());
            ByteBuffer byteBuffer = ByteBuffer.allocate(28
                        + (soundVersion == 1 ? 16 : 0)
                        + (soundVersion == 2 ? 36 : 0));
            ((Buffer)byteBuffer).position(6);
            IsoTypeWriter.writeUInt16(byteBuffer, dataReferenceIndex);
            IsoTypeWriter.writeUInt16(byteBuffer, soundVersion);
            IsoTypeWriter.writeUInt16(byteBuffer, reserved1);
            IsoTypeWriter.writeUInt32(byteBuffer, reserved2);
            IsoTypeWriter.writeUInt16(byteBuffer, channelCount);
            IsoTypeWriter.writeUInt16(byteBuffer, sampleSize);
            IsoTypeWriter.writeUInt16(byteBuffer, compressionId);
            IsoTypeWriter.writeUInt16(byteBuffer, packetSize);
            //isos.writeFixedPoint1616(getSampleRate());
            if (type.Equals("mlpa"))
            {
                IsoTypeWriter.writeUInt32(byteBuffer, getSampleRate());
            }
            else
            {
                IsoTypeWriter.writeUInt32(byteBuffer, getSampleRate() << 16);
            }

            if (soundVersion == 1)
            {
                IsoTypeWriter.writeUInt32(byteBuffer, samplesPerPacket);
                IsoTypeWriter.writeUInt32(byteBuffer, bytesPerPacket);
                IsoTypeWriter.writeUInt32(byteBuffer, bytesPerFrame);
                IsoTypeWriter.writeUInt32(byteBuffer, bytesPerSample);
            }

            if (soundVersion == 2)
            {
                IsoTypeWriter.writeUInt32(byteBuffer, samplesPerPacket);
                IsoTypeWriter.writeUInt32(byteBuffer, bytesPerPacket);
                IsoTypeWriter.writeUInt32(byteBuffer, bytesPerFrame);
                IsoTypeWriter.writeUInt32(byteBuffer, bytesPerSample);
                byteBuffer.put(soundVersion2Data);
            }
            writableByteChannel.write((ByteBuffer)byteBuffer.rewind());
            writeContainer(writableByteChannel);
        }

        public override long getSize()
        {
            long s = 28
                    + (soundVersion == 1 ? 16 : 0)
                    + (soundVersion == 2 ? 36 : 0) + getContainerSize();
            s += largeBox || s + 8 >= 1L << 32 ? 16 : 8;
            return s;

        }

        public override string ToString()
        {
            return "AudioSampleEntry{" +
                    "bytesPerSample=" + bytesPerSample +
                    ", bytesPerFrame=" + bytesPerFrame +
                    ", bytesPerPacket=" + bytesPerPacket +
                    ", samplesPerPacket=" + samplesPerPacket +
                    ", packetSize=" + packetSize +
                    ", compressionId=" + compressionId +
                    ", soundVersion=" + soundVersion +
                    ", sampleRate=" + sampleRate +
                    ", sampleSize=" + sampleSize +
                    ", channelCount=" + channelCount +
                    ", boxes=" + getBoxes() +
                    '}';
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            AudioSampleEntry that = (AudioSampleEntry)o;
            ByteStream baos1 = new ByteStream();
            ByteStream baos2 = new ByteStream();
            try
            {
                getBox(Channels.newChannel(baos1));
            }
            catch (IOException)
            {
                throw;
            }
            try
            {
                that.getBox(Channels.newChannel(baos2));
            }
            catch (IOException)
            {
                throw;
            }

            return baos1.toByteArray().SequenceEqual(baos2.toByteArray());
        }

        public override int GetHashCode()
        {
            ByteStream baos1 = new ByteStream();
            try
            {
                getBox(Channels.newChannel(baos1));
            }
            catch (IOException)
            {
                throw;
            }
            return Arrays.hashCode(baos1.toByteArray());
        }
    }
}
