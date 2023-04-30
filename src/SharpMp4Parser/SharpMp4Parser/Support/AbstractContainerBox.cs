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

using SharpMp4Parser.Java;
using SharpMp4Parser.Tools;
using System.Text;

namespace SharpMp4Parser.Support
{
    /**
     * Abstract base class suitable for most boxes acting purely as container for other boxes.
     */
    public class AbstractContainerBox : BasicContainer, ParsableBox
    {
        protected string type;
        protected bool largeBox;
        Container parent;

        public AbstractContainerBox(string type)
        {
            this.type = type;
        }


        public void setParent(Container parent)
        {
            this.parent = parent;
        }

        public virtual long getSize()
        {
            long s = getContainerSize();
            return s + ((largeBox || (s + 8) >= (1L << 32)) ? 16 : 8);
        }

        public string getType()
        {
            return type;
        }

        protected virtual ByteBuffer getHeader()
        {
            ByteBuffer header;
            if (largeBox || getSize() >= (1L << 32))
            {
                header = ByteBuffer.wrap(new byte[] { 0, 0, 0, 1, Encoding.UTF8.GetBytes(type)[0], Encoding.UTF8.GetBytes(type)[1], Encoding.UTF8.GetBytes(type)[2], Encoding.UTF8.GetBytes(type)[3], 0, 0, 0, 0, 0, 0, 0, 0 });
                ((Java.Buffer)header).position(8);
                IsoTypeWriter.writeUInt64(header, getSize());
            }
            else
            {
                header = ByteBuffer.wrap(new byte[] { 0, 0, 0, 0, Encoding.UTF8.GetBytes(type)[0], Encoding.UTF8.GetBytes(type)[1], Encoding.UTF8.GetBytes(type)[2], Encoding.UTF8.GetBytes(type)[3] });
                IsoTypeWriter.writeUInt32(header, getSize());
            }
            ((Java.Buffer)header).rewind();
            return header;
        }

        public virtual void parse(ReadableByteChannel dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            this.largeBox = header.remaining() == 16; // sometime people use large boxes without requiring them
            initContainer(dataSource, contentSize, boxParser);
        }


        public virtual void getBox(WritableByteChannel writableByteChannel)
        {
            writableByteChannel.write(getHeader());
            writeContainer(writableByteChannel);
        }
    }
}
