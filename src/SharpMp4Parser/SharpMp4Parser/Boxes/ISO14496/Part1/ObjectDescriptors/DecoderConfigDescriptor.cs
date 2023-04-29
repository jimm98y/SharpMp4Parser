/*
 * Copyright 2011 castLabs, Berlin
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

using System.Collections.Generic;
using System;
using System.Text;

namespace SharpMp4Parser.Boxes.ISO14496.Part1.ObjectDescriptors
{
    /**
     * class DecoderConfigDescriptor extends BaseDescriptor : bit(8)
     * tag=DecoderConfigDescrTag {
     * bit(8) objectTypeIndication;
     * bit(6) streamType;
     * bit(1) upStream;
     * const bit(1) reserved=1;
     * bit(24) bufferSizeDB;
     * bit(32) maxBitrate;
     * bit(32) avgBitrate;
     * DecoderSpecificInfo decSpecificInfo[0 .. 1];
     * profileLevelIndicationIndexDescriptor profileLevelIndicationIndexDescr
     * [0..255];
     * }
     */
    [Descriptor(Tags = new int[] { 0x04 })]
    public class DecoderConfigDescriptor : BaseDescriptor
    {
        //private static readonly Logger LOG = LoggerFactory.getLogger(DecoderConfigDescriptor.class);
        int objectTypeIndication;
        int streamType;
        int upStream;
        int bufferSizeDB;
        long maxBitRate;
        long avgBitRate;
        DecoderSpecificInfo decoderSpecificInfo;
        AudioSpecificConfig audioSpecificInfo;
        List<ProfileLevelIndicationDescriptor> profileLevelIndicationDescriptors = new List<ProfileLevelIndicationDescriptor>();
        byte[] configDescriptorDeadBytes;

        public DecoderConfigDescriptor()
        {
            tag = 0x04;
        }

        public override void parseDetail(ByteBuffer bb)
        {
            objectTypeIndication = IsoTypeReader.readUInt8(bb);

            int data = IsoTypeReader.readUInt8(bb);
            streamType = data >>> 2;
            upStream = (data >> 1) & 0x1;

            bufferSizeDB = IsoTypeReader.readUInt24(bb);
            maxBitRate = IsoTypeReader.readUInt32(bb);
            avgBitRate = IsoTypeReader.readUInt32(bb);


            BaseDescriptor descriptor;
            while (bb.remaining() > 2)
            { //1byte tag + at least 1byte size
                int begin = bb.position();
                descriptor = ObjectDescriptorFactory.createFrom(objectTypeIndication, bb);
                int read = bb.position() - begin;
                //LOG.trace("{} - DecoderConfigDescr1 read: {}, size: {}", descriptor, read, descriptor != null ? descriptor.getSize() : null);
                if (descriptor != null)
                {
                    int size = descriptor.getSize();
                    if (read < size)
                    {
                        //skip
                        configDescriptorDeadBytes = new byte[size - read];
                        bb.get(configDescriptorDeadBytes);
                    }
                }
                if (descriptor is DecoderSpecificInfo)
                {
                    decoderSpecificInfo = (DecoderSpecificInfo)descriptor;
                }
                else if (descriptor is AudioSpecificConfig)
                {
                    audioSpecificInfo = (AudioSpecificConfig)descriptor;
                }
                else if (descriptor is ProfileLevelIndicationDescriptor)
                {
                    profileLevelIndicationDescriptors.Add((ProfileLevelIndicationDescriptor)descriptor);
                }

            }

        }

        public int getContentSize()
        {
            int output = 13 +
                    (audioSpecificInfo == null ? 0 : audioSpecificInfo.getSize()) +
                    (decoderSpecificInfo == null ? 0 : decoderSpecificInfo.getSize());
            foreach (ProfileLevelIndicationDescriptor profileLevelIndicationDescriptor in profileLevelIndicationDescriptors)
            {
                output += profileLevelIndicationDescriptor.getSize();
            }
            return output;
        }

        public ByteBuffer serialize()
        {
            ByteBuffer output = ByteBuffer.allocate(getSize());
            IsoTypeWriter.writeUInt8(output, tag);
            writeSize(output, getContentSize());
            IsoTypeWriter.writeUInt8(output, objectTypeIndication);
            int flags = (streamType << 2) | (upStream << 1) | 1;
            IsoTypeWriter.writeUInt8(output, flags);
            IsoTypeWriter.writeUInt24(output, bufferSizeDB);
            IsoTypeWriter.writeUInt32(output, maxBitRate);
            IsoTypeWriter.writeUInt32(output, avgBitRate);
            if (decoderSpecificInfo != null)
            {
                ByteBuffer bb = decoderSpecificInfo.serialize();
                output.put(bb);
            }
            if (audioSpecificInfo != null)
            {
                ByteBuffer bb = audioSpecificInfo.serialize();
                output.put(bb);
            }
            foreach (ProfileLevelIndicationDescriptor profileLevelIndicationDescriptor in profileLevelIndicationDescriptors)
            {
                output.put(profileLevelIndicationDescriptor.serialize());
            }
            return (ByteBuffer)((Buffer)output).rewind();
        }

        public DecoderSpecificInfo getDecoderSpecificInfo()
        {
            return decoderSpecificInfo;
        }

        public void setDecoderSpecificInfo(DecoderSpecificInfo decoderSpecificInfo)
        {
            this.decoderSpecificInfo = decoderSpecificInfo;
        }

        public AudioSpecificConfig getAudioSpecificInfo()
        {
            return audioSpecificInfo;
        }

        public void setAudioSpecificInfo(AudioSpecificConfig audioSpecificInfo)
        {
            this.audioSpecificInfo = audioSpecificInfo;
        }

        public List<ProfileLevelIndicationDescriptor> getProfileLevelIndicationDescriptors()
        {
            return profileLevelIndicationDescriptors;
        }

        public int getObjectTypeIndication()
        {
            return objectTypeIndication;
        }

        public void setObjectTypeIndication(int objectTypeIndication)
        {
            this.objectTypeIndication = objectTypeIndication;
        }

        public int getStreamType()
        {
            return streamType;
        }

        public void setStreamType(int streamType)
        {
            this.streamType = streamType;
        }

        public int getUpStream()
        {
            return upStream;
        }

        public void setUpStream(int upStream)
        {
            this.upStream = upStream;
        }

        public int getBufferSizeDB()
        {
            return bufferSizeDB;
        }

        public void setBufferSizeDB(int bufferSizeDB)
        {
            this.bufferSizeDB = bufferSizeDB;
        }

        public long getMaxBitRate()
        {
            return maxBitRate;
        }

        public void setMaxBitRate(long maxBitRate)
        {
            this.maxBitRate = maxBitRate;
        }

        public long getAvgBitRate()
        {
            return avgBitRate;
        }

        public void setAvgBitRate(long avgBitRate)
        {
            this.avgBitRate = avgBitRate;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("DecoderConfigDescriptor");
            sb.Append("{objectTypeIndication=").Append(objectTypeIndication);
            sb.Append(", streamType=").Append(streamType);
            sb.Append(", upStream=").Append(upStream);
            sb.Append(", bufferSizeDB=").Append(bufferSizeDB);
            sb.Append(", maxBitRate=").Append(maxBitRate);
            sb.Append(", avgBitRate=").Append(avgBitRate);
            sb.Append(", decoderSpecificInfo=").Append(decoderSpecificInfo);
            sb.Append(", audioSpecificInfo=").Append(audioSpecificInfo);
            sb.Append(", configDescriptorDeadBytes=").Append(Hex.encodeHex(configDescriptorDeadBytes != null ? configDescriptorDeadBytes : new byte[] { }));
            sb.Append(", profileLevelIndicationDescriptors=").Append(profileLevelIndicationDescriptors == null ? "null" : profileLevelIndicationDescriptors);
            sb.Append('}');
            return sb.ToString();
        }
        /*objectTypeIndication values
          0x00 Forbidden
        0x01 Systems ISO/IEC 14496-1 a
        0x02 Systems ISO/IEC 14496-1 b
        0x03 Interaction Stream
        0x04 Systems ISO/IEC 14496-1 Extended BIFS Configuration c
        0x05 Systems ISO/IEC 14496-1 AFX d
        0x06 Font Data Stream
        0x07 Synthesized Texture Stream
        0x08 Streaming Text Stream
        0x09-0x1F reserved for ISO use
        0x20 Visual ISO/IEC 14496-2 e
        0x21 Visual ITU-T Recommendation H.264 | ISO/IEC 14496-10 f
        0x22 Parameter Sets for ITU-T Recommendation H.264 | ISO/IEC 14496-10 f
        0x23-0x3F reserved for ISO use
        0x40 Audio ISO/IEC 14496-3 g
        0x41-0x5F reserved for ISO use
        0x60 Visual ISO/IEC 13818-2 Simple Profile
        0x61 Visual ISO/IEC 13818-2 Main Profile
        0x62 Visual ISO/IEC 13818-2 SNR Profile
        0x63 Visual ISO/IEC 13818-2 Spatial Profile
        0x64 Visual ISO/IEC 13818-2 High Profile
        0x65 Visual ISO/IEC 13818-2 422 Profile
        0x66 Audio ISO/IEC 13818-7 Main Profile
        0x67 Audio ISO/IEC 13818-7 LowComplexity Profile
        0x68 Audio ISO/IEC 13818-7 Scaleable Sampling Rate Profile
        0x69 Audio ISO/IEC 13818-3
        0x6A Visual ISO/IEC 11172-2
        0x6B Audio ISO/IEC 11172-3
        0x6C Visual ISO/IEC 10918-1
        0x6D reserved for registration authority i
        0x6E Visual ISO/IEC 15444-1
        0x6F - 0x9F reserved for ISO use
        0xA0 - 0xBF reserved for registration authority i
        0xC0 - 0xE0 user private
        0xE1 reserved for registration authority i
        0xE2 - 0xFE user private
        0xFF no object type specified h
        */
        /* streamType values
          0x00 Forbidden
        0x01 ObjectDescriptorStream (see 7.2.5)
        0x02 ClockReferenceStream (see 7.3.2.5)
        0x03 SceneDescriptionStream (see ISO/IEC 14496-11)
        0x04 VisualStream
        0x05 AudioStream
        0x06 MPEG7Stream
        0x07 IPMPStream (see 7.2.3.2)
        0x08 ObjectContentInfoStream (see 7.2.4.2)
        0x09 MPEGJStream
        0x0A Interaction Stream
        0x0B IPMPToolStream (see [ISO/IEC 14496-13])
        0x0C - 0x1F reserved for ISO use
        0x20 - 0x3F user private
        */
    }
}