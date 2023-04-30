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

using SharpMp4Parser.Java;
using SharpMp4Parser.Tools;
using System.Collections.Generic;
using System.Text;

namespace SharpMp4Parser.Boxes.ISO14496.Part1.ObjectDescriptors
{
    /*
    class InitialObjectDescriptor extends ObjectDescriptorBase : bit(8)
    tag=InitialObjectDescrTag {
    bit(10) ObjectDescriptorID;
    bit(1) URL_Flag;
    bit(1) includeInlineProfileLevelFlag;
    const bit(4) reserved=0b1111;
    if (URL_Flag) {
    bit(8) URLlength;
    bit(8) URLstring[URLlength];
    } else {
    bit(8) ODProfileLevelIndication;
    bit(8) sceneProfileLevelIndication;
    bit(8) audioProfileLevelIndication;
    bit(8) visualProfileLevelIndication;
    bit(8) graphicsProfileLevelIndication;
    ES_Descriptor esDescr[1 .. 255];
    OCI_Descriptor ociDescr[0 .. 255];
    IPMP_DescriptorPointer ipmpDescrPtr[0 .. 255];
    IPMP_Descriptor ipmpDescr [0 .. 255];
    IPMP_ToolListDescriptor toolListDescr[0 .. 1];
    }
    ExtensionDescriptor extDescr[0 .. 255];
    }
    */
    //@Descriptor(tags = {0x02, 0x10})
    public abstract class InitialObjectDescriptor : ObjectDescriptorBase
    {
        int urlFlag;
        int includeInlineProfileLevelFlag;
        int urlLength;
        string urlString;
        int oDProfileLevelIndication;
        int sceneProfileLevelIndication;
        int audioProfileLevelIndication;
        int visualProfileLevelIndication;
        int graphicsProfileLevelIndication;
        List<ESDescriptor> esDescriptors = new List<ESDescriptor>();
        List<ExtensionDescriptor> extensionDescriptors = new List<ExtensionDescriptor>();
        List<BaseDescriptor> unknownDescriptors = new List<BaseDescriptor>();
        private int objectDescriptorId;

        public override void parseDetail(ByteBuffer bb)
        {
            int data = IsoTypeReader.readUInt16(bb);
            objectDescriptorId = (data & 0xFFC0) >> 6;

            urlFlag = (data & 0x3F) >> 5;
            includeInlineProfileLevelFlag = (data & 0x1F) >> 4;

            int sizeLeft = getSize() - 2;
            if (urlFlag == 1)
            {
                urlLength = IsoTypeReader.readUInt8(bb);
                urlString = IsoTypeReader.readString(bb, urlLength);
                sizeLeft = sizeLeft - (1 + urlLength);
            }
            else
            {
                oDProfileLevelIndication = IsoTypeReader.readUInt8(bb);
                sceneProfileLevelIndication = IsoTypeReader.readUInt8(bb);
                audioProfileLevelIndication = IsoTypeReader.readUInt8(bb);
                visualProfileLevelIndication = IsoTypeReader.readUInt8(bb);
                graphicsProfileLevelIndication = IsoTypeReader.readUInt8(bb);

                sizeLeft = sizeLeft - 5;

                if (sizeLeft > 2)
                {
                    BaseDescriptor descriptor = ObjectDescriptorFactory.createFrom(-1, bb);
                    sizeLeft = sizeLeft - descriptor.getSize();
                    if (descriptor is ESDescriptor)
                    {
                        esDescriptors.Add((ESDescriptor)descriptor);
                    }
                    else
                    {
                        unknownDescriptors.Add(descriptor);
                    }
                }
            }

            if (sizeLeft > 2)
            {
                BaseDescriptor descriptor = ObjectDescriptorFactory.createFrom(-1, bb);
                if (descriptor is ExtensionDescriptor)
                {
                    extensionDescriptors.Add((ExtensionDescriptor)descriptor);
                }
                else
                {
                    unknownDescriptors.Add(descriptor);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("InitialObjectDescriptor");
            sb.Append("{objectDescriptorId=").Append(objectDescriptorId);
            sb.Append(", urlFlag=").Append(urlFlag);
            sb.Append(", includeInlineProfileLevelFlag=").Append(includeInlineProfileLevelFlag);
            sb.Append(", urlLength=").Append(urlLength);
            sb.Append(", urlString='").Append(urlString).Append('\'');
            sb.Append(", oDProfileLevelIndication=").Append(oDProfileLevelIndication);
            sb.Append(", sceneProfileLevelIndication=").Append(sceneProfileLevelIndication);
            sb.Append(", audioProfileLevelIndication=").Append(audioProfileLevelIndication);
            sb.Append(", visualProfileLevelIndication=").Append(visualProfileLevelIndication);
            sb.Append(", graphicsProfileLevelIndication=").Append(graphicsProfileLevelIndication);
            sb.Append(", esDescriptors=").Append(esDescriptors);
            sb.Append(", extensionDescriptors=").Append(extensionDescriptors);
            sb.Append(", unknownDescriptors=").Append(unknownDescriptors);
            sb.Append('}');
            return sb.ToString();
        }
    }
}