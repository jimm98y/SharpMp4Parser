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
    /*
    class ES_Descriptor extends BaseDescriptor : bit(8) tag=ES_DescrTag {
    bit(16) ES_ID;
    bit(1) streamDependenceFlag;
    bit(1) URL_Flag;
    bit(1) OCRstreamFlag;
    bit(5) streamPriority;
    if (streamDependenceFlag)
    bit(16) dependsOn_ES_ID;
    if (URL_Flag) {
    bit(8) URLlength;
    bit(8) URLstring[URLlength];
    }
    if (OCRstreamFlag)
    bit(16) OCR_ES_Id;
    DecoderConfigDescriptor decConfigDescr;
    if (ODProfileLevelIndication==0x01) //no SL extension.
    {
    SLConfigDescriptor slConfigDescr;
    }
    else // SL extension is possible.
    {
    SLConfigDescriptor slConfigDescr;
    }
    IPI_DescrPointer ipiPtr[0 .. 1];
    IP_IdentificationDataSet ipIDS[0 .. 255];
    IPMP_DescriptorPointer ipmpDescrPtr[0 .. 255];
    LanguageDescriptor langDescr[0 .. 255];
    QoS_Descriptor qosDescr[0 .. 1];
    RegistrationDescriptor regDescr[0 .. 1];
    ExtensionDescriptor extDescr[0 .. 255];
    }
     */
    [Descriptor(Tags = new int[] { 0x03 })]
    public class ESDescriptor : BaseDescriptor
    {
        //private static Logger LOG = LoggerFactory.getLogger(ESDescriptor.class);
        int esId;
        int streamDependenceFlag;
        int URLFlag;
        int oCRstreamFlag;
        int streamPriority;
        int URLLength = 0;
        String URLString;
        int remoteODFlag;
        int dependsOnEsId;
        int oCREsId;
        DecoderConfigDescriptor decoderConfigDescriptor;
        SLConfigDescriptor slConfigDescriptor;
        List<BaseDescriptor> otherDescriptors = new List<BaseDescriptor>();

        public ESDescriptor()
        {
            tag = 0x3;
        }

        public override void parseDetail(ByteBuffer bb)
        {
            esId = IsoTypeReader.readUInt16(bb);

            int data = IsoTypeReader.readUInt8(bb);
            streamDependenceFlag = data >>> 7;
            URLFlag = (data >>> 6) & 0x1;
            oCRstreamFlag = (data >>> 5) & 0x1;
            streamPriority = data & 0x1f;

            if (streamDependenceFlag == 1) {
                dependsOnEsId = IsoTypeReader.readUInt16(bb);
            }
            if (URLFlag == 1) {
                URLLength = IsoTypeReader.readUInt8(bb);
                URLString = IsoTypeReader.readString(bb, URLLength);
            }
            if (oCRstreamFlag == 1) {
                oCREsId = IsoTypeReader.readUInt16(bb);
            }

            while (bb.remaining() > 1) {
                BaseDescriptor descriptor = ObjectDescriptorFactory.createFrom(-1, bb);
                if (descriptor is DecoderConfigDescriptor)
                {
                    decoderConfigDescriptor = (DecoderConfigDescriptor)descriptor;
                }
                else if (descriptor is SLConfigDescriptor) 
                {
                    slConfigDescriptor = (SLConfigDescriptor)descriptor;
                } 
                else
                {
                    otherDescriptors.Add(descriptor);
                }
            }
        }

        public int getContentSize()
        {
            int output = 3;
            if (streamDependenceFlag > 0)
            {
                output += 2;
            }
            if (URLFlag > 0)
            {
                output += 1 + URLLength;
            }
            if (oCRstreamFlag > 0)
            {
                output += 2;
            }

            output += decoderConfigDescriptor.getSize();
            output += slConfigDescriptor.getSize();

            if (otherDescriptors.size() > 0)
            {
                throw new NotImplementedException(" Doesn't handle other descriptors yet");
            }

            return output;
        }

        public ByteBuffer serialize()
        {
            byte[] aaa = new byte[getSize()];
            ByteBuffer output = ByteBuffer.wrap(aaa); // Usually is around 30 bytes, so 200 should be enough...
            IsoTypeWriter.writeUInt8(output, 3);
            writeSize(output, getContentSize());
            IsoTypeWriter.writeUInt16(output, esId);
            int flags = (streamDependenceFlag << 7) | (URLFlag << 6) | (oCRstreamFlag << 5) | (streamPriority & 0x1f);
            IsoTypeWriter.writeUInt8(output, flags);
            if (streamDependenceFlag > 0)
            {
                IsoTypeWriter.writeUInt16(output, dependsOnEsId);
            }
            if (URLFlag > 0)
            {
                IsoTypeWriter.writeUInt8(output, URLLength);
                IsoTypeWriter.writeUtf8String(output, URLString);
            }
            if (oCRstreamFlag > 0)
            {
                IsoTypeWriter.writeUInt16(output, oCREsId);
            }

            ByteBuffer dec = decoderConfigDescriptor.serialize();
            ByteBuffer sl = slConfigDescriptor.serialize();
            output.put(dec.array());
            output.put(sl.array());

            // Doesn't handle other descriptors yet

            return output;
        }


        public DecoderConfigDescriptor getDecoderConfigDescriptor()
        {
            return decoderConfigDescriptor;
        }

        public void setDecoderConfigDescriptor(DecoderConfigDescriptor decoderConfigDescriptor)
        {
            this.decoderConfigDescriptor = decoderConfigDescriptor;
        }

        public SLConfigDescriptor getSlConfigDescriptor()
        {
            return slConfigDescriptor;
        }

        public void setSlConfigDescriptor(SLConfigDescriptor slConfigDescriptor)
        {
            this.slConfigDescriptor = slConfigDescriptor;
        }

        public List<BaseDescriptor> getOtherDescriptors()
        {
            return otherDescriptors;
        }

        public int getoCREsId()
        {
            return oCREsId;
        }

        public void setoCREsId(int oCREsId)
        {
            this.oCREsId = oCREsId;
        }

        public int getEsId()
        {
            return esId;
        }

        public void setEsId(int esId)
        {
            this.esId = esId;
        }

        public int getStreamDependenceFlag()
        {
            return streamDependenceFlag;
        }

        public void setStreamDependenceFlag(int streamDependenceFlag)
        {
            this.streamDependenceFlag = streamDependenceFlag;
        }

        public int getURLFlag()
        {
            return URLFlag;
        }

        public void setURLFlag(int URLFlag)
        {
            this.URLFlag = URLFlag;
        }

        public int getoCRstreamFlag()
        {
            return oCRstreamFlag;
        }

        public void setoCRstreamFlag(int oCRstreamFlag)
        {
            this.oCRstreamFlag = oCRstreamFlag;
        }

        public int getStreamPriority()
        {
            return streamPriority;
        }

        public void setStreamPriority(int streamPriority)
        {
            this.streamPriority = streamPriority;
        }

        public int getURLLength()
        {
            return URLLength;
        }

        public void setURLLength(int URLLength)
        {
            this.URLLength = URLLength;
        }

        public string getURLString()
        {
            return URLString;
        }

        public void setURLString(string URLString)
        {
            this.URLString = URLString;
        }

        public int getRemoteODFlag()
        {
            return remoteODFlag;
        }

        public void setRemoteODFlag(int remoteODFlag)
        {
            this.remoteODFlag = remoteODFlag;
        }

        public int getDependsOnEsId()
        {
            return dependsOnEsId;
        }

        public void setDependsOnEsId(int dependsOnEsId)
        {
            this.dependsOnEsId = dependsOnEsId;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ESDescriptor");
            sb.Append("{esId=").Append(esId);
            sb.Append(", streamDependenceFlag=").Append(streamDependenceFlag);
            sb.Append(", URLFlag=").Append(URLFlag);
            sb.Append(", oCRstreamFlag=").Append(oCRstreamFlag);
            sb.Append(", streamPriority=").Append(streamPriority);
            sb.Append(", URLLength=").Append(URLLength);
            sb.Append(", URLString='").Append(URLString).Append('\'');
            sb.Append(", remoteODFlag=").Append(remoteODFlag);
            sb.Append(", dependsOnEsId=").Append(dependsOnEsId);
            sb.Append(", oCREsId=").Append(oCREsId);
            sb.Append(", decoderConfigDescriptor=").Append(decoderConfigDescriptor);
            sb.Append(", slConfigDescriptor=").Append(slConfigDescriptor);
            sb.Append('}');
            return sb.ToString();
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || getClass() != o.getClass()) return false;

            ESDescriptor that = (ESDescriptor)o;

            if (URLFlag != that.URLFlag) return false;
            if (URLLength != that.URLLength) return false;
            if (dependsOnEsId != that.dependsOnEsId) return false;
            if (esId != that.esId) return false;
            if (oCREsId != that.oCREsId) return false;
            if (oCRstreamFlag != that.oCRstreamFlag) return false;
            if (remoteODFlag != that.remoteODFlag) return false;
            if (streamDependenceFlag != that.streamDependenceFlag) return false;
            if (streamPriority != that.streamPriority) return false;
            if (URLString != null ? !URLString.Equals(that.URLString) : that.URLString != null) return false;
            if (decoderConfigDescriptor != null ? !decoderConfigDescriptor.Equals(that.decoderConfigDescriptor) : that.decoderConfigDescriptor != null)
                return false;
            if (otherDescriptors != null ? !otherDescriptors.Equals(that.otherDescriptors) : that.otherDescriptors != null)
                return false;
            if (slConfigDescriptor != null ? !slConfigDescriptor.Equals(that.slConfigDescriptor) : that.slConfigDescriptor != null)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            int result = esId;
            result = 31 * result + streamDependenceFlag;
            result = 31 * result + URLFlag;
            result = 31 * result + oCRstreamFlag;
            result = 31 * result + streamPriority;
            result = 31 * result + URLLength;
            result = 31 * result + (URLString != null ? URLString.GetHashCode() : 0);
            result = 31 * result + remoteODFlag;
            result = 31 * result + dependsOnEsId;
            result = 31 * result + oCREsId;
            result = 31 * result + (decoderConfigDescriptor != null ? decoderConfigDescriptor.GetHashCode() : 0);
            result = 31 * result + (slConfigDescriptor != null ? slConfigDescriptor.GetHashCode() : 0);
            result = 31 * result + (otherDescriptors != null ? otherDescriptors.GetHashCode() : 0);
            return result;
        }
    }
}