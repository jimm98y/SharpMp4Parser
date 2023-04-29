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

using System;
using System.Text;

namespace SharpMp4Parser.Boxes.ISO14496.Part1.ObjectDescriptors
{
    public class UnknownDescriptor : BaseDescriptor
    {
        //private static Logger LOG = LoggerFactory.getLogger(UnknownDescriptor.class);
        private ByteBuffer data;

        public override void parseDetail(ByteBuffer bb)
        {
            data = bb.slice();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UnknownDescriptor");
            sb.Append("{tag=").Append(tag);
            sb.Append(", sizeOfInstance=").Append(sizeOfInstance);
            sb.Append(", data=").Append(data);
            sb.Append('}');
            return sb.ToString();
        }

        public override ByteBuffer serialize()
        {
            throw new Exception("sdjlhfl");
        }

        public override int getContentSize()
        {
            throw new Exception("sdjlhfl");
        }
    }
}
