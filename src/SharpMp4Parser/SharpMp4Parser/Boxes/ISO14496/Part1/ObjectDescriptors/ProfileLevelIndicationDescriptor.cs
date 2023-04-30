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
using System;
using System.Text;

namespace SharpMp4Parser.Boxes.ISO14496.Part1.ObjectDescriptors
{
    /**
     * class ProfileLevelIndicationIndexDescriptor () extends BaseDescriptor
     * : bit(8) ProfileLevelIndicationIndexDescrTag {
     * bit(8) profileLevelIndicationIndex;
     * }
     */
    [Descriptor(Tags = new int[] { 0x14 })]
    public class ProfileLevelIndicationDescriptor : BaseDescriptor
    {
        int profileLevelIndicationIndex;

        public ProfileLevelIndicationDescriptor()
        {
            tag = 0x14;
        }

        public override void parseDetail(ByteBuffer bb)
        {
            profileLevelIndicationIndex = IsoTypeReader.readUInt8(bb);
        }

        public override ByteBuffer serialize()
        {
            ByteBuffer output = ByteBuffer.allocate(getSize());
            IsoTypeWriter.writeUInt8(output, 0x14);
            writeSize(output, getContentSize());
            IsoTypeWriter.writeUInt8(output, profileLevelIndicationIndex);
            return output;
        }

        public override int getContentSize()
        {
            return 1;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ProfileLevelIndicationDescriptor");
            sb.Append("{profileLevelIndicationIndex=").Append(Integer.toHexString(profileLevelIndicationIndex));
            sb.Append('}');
            return sb.ToString();
        }

        public override bool Equals(Object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }

            ProfileLevelIndicationDescriptor that = (ProfileLevelIndicationDescriptor)o;

            if (profileLevelIndicationIndex != that.profileLevelIndicationIndex)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return profileLevelIndicationIndex;
        }
    }
}
