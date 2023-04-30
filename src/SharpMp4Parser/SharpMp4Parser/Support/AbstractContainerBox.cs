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

using System.ComponentModel;
using System;

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

        public long getSize()
        {
            long s = getContainerSize();
            return s + ((largeBox || (s + 8) >= (1L << 32)) ? 16 : 8);
        }

        public string getType()
        {
            return type;
        }

        protected ByteBuffer getHeader()
        {
            ByteBuffer header;
            if (largeBox || getSize() >= (1L << 32))
            {
                header = ByteBuffer.wrap(new byte[] { 0, 0, 0, 1, type.getBytes()[0], type.getBytes()[1], type.getBytes()[2], type.getBytes()[3], 0, 0, 0, 0, 0, 0, 0, 0 });
                ((Buffer)header).position(8);
                IsoTypeWriter.writeUInt64(header, getSize());
            }
            else
            {
                header = ByteBuffer.wrap(new byte[] { 0, 0, 0, 0, type.getBytes()[0], type.getBytes()[1], type.getBytes()[2], type.getBytes()[3] });
                IsoTypeWriter.writeUInt32(header, getSize());
            }
            ((Buffer)header).rewind();
            return header;
        }

        public void parse(ReadableByteChannel dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            this.largeBox = header.remaining() == 16; // sometime people use large boxes without requiring them
            initContainer(dataSource, contentSize, boxParser);
        }


        public void getBox(WritableByteChannel writableByteChannel)
        {
            writableByteChannel.write(getHeader());
            writeContainer(writableByteChannel);
        }
    }
}
