﻿/*
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

using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part14
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * ES Descriptor Box.
     */
    public class ESDescriptorBox : AbstractDescriptorBox
    {
        public const string TYPE = "esds";


        public ESDescriptorBox() : base(TYPE)
        { }

        public ESDescriptor getEsDescriptor()
        {
            return (ESDescriptor)getDescriptor();
        }

        public void setEsDescriptor(ESDescriptor esDescriptor)
        {
            setDescriptor(esDescriptor);
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            ESDescriptorBox that = (ESDescriptorBox)o;

            if (data != null ? !data.Equals(that.data) : that.data != null) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return data != null ? data.hashCode() : 0;
        }

        protected override long getContentSize()
        {
            ESDescriptor esd = getEsDescriptor();
            if (esd != null)
            {
                return 4 + esd.getSize();
            }
            else
            {
                return 4 + data.remaining();
            }
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            ESDescriptor esd = getEsDescriptor();
            if (esd != null)
            {
                byteBuffer.put(esd.serialize().rewind());
            }
            else
            {
                byteBuffer.put(data.duplicate());
            }
        }
    }
}
