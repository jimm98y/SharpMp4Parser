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

using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;
using System.Text;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors
{
    /**
     * abstract class ExtensionDescriptor extends BaseDescriptor
     * : bit(8) tag = ExtensionProfileLevelDescrTag, ExtDescrTagStartRange ..
     * ExtDescrTagEndRange {
     * // empty. To be filled by classes extending this class.
     * }
     */
    [Descriptor(Tags = new int[] { 0x13 })]
    public class ExtensionProfileLevelDescriptor : BaseDescriptor
    {
        byte[] bytes;

        public ExtensionProfileLevelDescriptor()
        {
            tag = 0x13;
        }

        public override void parseDetail(ByteBuffer bb)
        {
            if (getSize() > 0)
            {
                bytes = new byte[getSize()];
                bb.get(bytes);
            }
        }

        public override ByteBuffer serialize()
        {
            throw new NotImplementedException("Not Implemented");
        }

        public override int getContentSize()
        {
            throw new NotImplementedException("Not Implemented");
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ExtensionDescriptor");
            sb.Append("{bytes=").Append(bytes == null ? "null" : Hex.encodeHex(bytes));
            sb.Append('}');
            return sb.ToString();
        }
    }
}