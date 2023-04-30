﻿/*
 * Copyright 2009 castLabs GmbH, Berlin
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

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /*
    aligned(8) class SampleAuxiliaryInformationOffsetsBox
                extends FullBox(‘saio’, version, flags)
    {
                if (flags & 1) {
                            unsigned int(32) aux_info_type;
                            unsigned int(32) aux_info_type_parameter;
                }
                unsigned int(32) entry_count;
                if ( version == 0 )
                {
                            unsigned int(32) offset[ entry_count ];
                }
                else
                {
                            unsigned int(64) offset[ entry_count ];
                }
    }
     */

    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     */
    public class SampleAuxiliaryInformationOffsetsBox : AbstractFullBox
    {
        public const string TYPE = "saio";

        private long[] offsets = new long[0];
        private string auxInfoType;
        private string auxInfoTypeParameter;

        public SampleAuxiliaryInformationOffsetsBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return 8 + (getVersion() == 0 ? 4 * offsets.Length : 8 * offsets.Length) + ((getFlags() & 1) == 1 ? 8 : 0);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            if ((getFlags() & 1) == 1)
            {
                byteBuffer.put(IsoFile.fourCCtoBytes(auxInfoType));
                byteBuffer.put(IsoFile.fourCCtoBytes(auxInfoTypeParameter));
            }

            IsoTypeWriter.writeUInt32(byteBuffer, offsets.Length);
            foreach (long offset in offsets)
            {
                if (getVersion() == 0)
                {
                    IsoTypeWriter.writeUInt32(byteBuffer, offset);
                }
                else
                {
                    IsoTypeWriter.writeUInt64(byteBuffer, offset);
                }
            }
        }

        public override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);

            if ((getFlags() & 1) == 1)
            {
                auxInfoType = IsoTypeReader.read4cc(content);
                auxInfoTypeParameter = IsoTypeReader.read4cc(content);
            }

            int entryCount = CastUtils.l2i(IsoTypeReader.readUInt32(content));
            offsets = new long[entryCount];

            for (int i = 0; i < entryCount; i++)
            {
                if (getVersion() == 0)
                {
                    offsets[i] = IsoTypeReader.readUInt32(content);
                }
                else
                {
                    offsets[i] = IsoTypeReader.readUInt64(content);
                }
            }
        }

        public string getAuxInfoType()
        {
            return auxInfoType;
        }

        public void setAuxInfoType(string auxInfoType)
        {
            this.auxInfoType = auxInfoType;
        }

        public string getAuxInfoTypeParameter()
        {
            return auxInfoTypeParameter;
        }

        public void setAuxInfoTypeParameter(string auxInfoTypeParameter)
        {
            this.auxInfoTypeParameter = auxInfoTypeParameter;
        }

        public long[] getOffsets()
        {
            return offsets;
        }

        public void setOffsets(long[] offsets)
        {
            this.offsets = offsets;
        }
    }
}