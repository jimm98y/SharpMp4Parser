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

using System.IO;
using System;
using SharpMp4Parser.Java;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors;
using SharpMp4Parser.IsoParser.Support;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part14
{
    /**
      * ES Descriptor Box.
      */
    public class AbstractDescriptorBox : AbstractFullBox
    {
        //private static Logger LOG = LoggerFactory.getLogger(AbstractDescriptorBox.class.getName());

        protected BaseDescriptor descriptor;
        protected ByteBuffer data;

        public AbstractDescriptorBox(string type) : base(type)
        { }

        public ByteBuffer getData()
        {
            return data;
        }

        public void setData(ByteBuffer data)
        {
            this.data = data;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            data.rewind(); // has been fforwarded by parsing
            byteBuffer.put(data);
        }

        protected override long getContentSize()
        {
            return 4 + data.limit();
        }

        public BaseDescriptor getDescriptor()
        {
            return descriptor;
        }

        public void setDescriptor(BaseDescriptor descriptor)
        {
            this.descriptor = descriptor;
        }

        public string getDescriptorAsString()
        {
            return descriptor.ToString();
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            data = content.slice();
            ((Java.Buffer)content).position(content.position() + content.remaining());
            try
            {
                data.rewind();
                descriptor = ObjectDescriptorFactory.createFrom(-1, (ByteBuffer)data.duplicate());
            }
            catch (IOException e)
            {
                Java.LOG.warn("Error parsing ObjectDescriptor", e);
                //that's why we copied it ;)
            }
            catch (IndexOutOfRangeException ex)
            {
                Java.LOG.warn("Error parsing ObjectDescriptor", ex);
                //that's why we copied it ;)
            }
        }
    }
}
