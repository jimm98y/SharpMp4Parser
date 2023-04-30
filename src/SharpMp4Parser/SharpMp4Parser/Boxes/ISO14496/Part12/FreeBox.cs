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

using System.Collections.Generic;
using System.ComponentModel;
using System;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * A free box. Just a placeholder to enable editing without rewriting the whole file.
     */
    public class FreeBox : ParsableBox
    {
        public const string TYPE = "free";
        ByteBuffer data;
        List<ParsableBox> replacers = new List<ParsableBox>();
        private Container parent;
        private long offset;

        public FreeBox()
        {
            this.data = ByteBuffer.wrap(new byte[0]);
        }

        public FreeBox(int size)
        {
            this.data = ByteBuffer.allocate(size);
        }

        public ByteBuffer getData()
        {
            if (data != null)
            {
                return (ByteBuffer)((Buffer)data.duplicate()).rewind();
            }
            else
            {
                return null;
            }
        }

        public void setData(ByteBuffer data)
        {
            this.data = data;
        }

        public void getBox(WritableByteChannel os)
        {
            foreach (ParsableBox replacer in replacers)
            {
                replacer.getBox(os);
            }
            ByteBuffer header = ByteBuffer.allocate(8);
            IsoTypeWriter.writeUInt32(header, 8 + data.limit());
            header.put(TYPE.getBytes());
            ((Buffer)header).rewind();
            os.write(header);
            ((Buffer)header).rewind();
            ((Buffer)data).rewind();
            os.write(data);
            ((Buffer)data).rewind();
        }

        public long getSize()
        {
            long size = 8;
            foreach (ParsableBox replacer in replacers)
            {
                size += replacer.getSize();
            }
            size += data.limit();
            return size;
        }

        public string getType()
        {
            return TYPE;
        }

        public void parse(ReadableByteChannel dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            data = ByteBuffer.allocate(CastUtils.l2i(contentSize));

            int bytesRead = 0;
            int b;
            while (((b = dataSource.read(data)) + bytesRead) < contentSize)
            {
                bytesRead += b;
            }
        }

        public void addAndReplace(ParsableBox parsableBox)
        {
            ((Buffer)data).position(CastUtils.l2i(parsableBox.getSize()));
            data = data.slice();
            replacers.Add(parsableBox);
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || getClass() != o.getClass()) return false;

            FreeBox freeBox = (FreeBox)o;

            if (getData() != null ? !getData().equals(freeBox.getData()) : freeBox.getData() != null) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return data != null ? data.hashCode() : 0;
        }
    }
}