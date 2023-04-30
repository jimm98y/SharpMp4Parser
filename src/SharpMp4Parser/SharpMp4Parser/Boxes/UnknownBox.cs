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

using System;

namespace SharpMp4Parser.Boxes
{
    /**
     * A box unknown to the ISO Parser. If there is no specific Box implementation for a Box this <code>UnknownBox</code>
     * will just hold the box's data.
     */
    public class UnknownBox : AbstractBox
    {
        ByteBuffer data;

        public UnknownBox(string type) : base(type)
        { }

        protected override long getContentSize()
        {
            return data.limit();
        }

        public override void _parseDetails(ByteBuffer content)
        {
            data = content;
            ((Buffer)content).position(content.position() + content.remaining());
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            ((Buffer)data).rewind();
            byteBuffer.put(data);
        }

        public ByteBuffer getData()
        {
            return data;
        }

        public void setData(ByteBuffer data)
        {
            this.data = data;
        }

        public override string ToString()
        {
            return "UnknownBox{" +
                    "type=" + type +
                    '}';
        }
    }
}
