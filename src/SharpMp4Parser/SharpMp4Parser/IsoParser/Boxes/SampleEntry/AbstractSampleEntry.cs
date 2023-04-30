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

using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.SampleEntry
{
    /**
     * Abstract base class for all sample entries.
     *
     * @see AudioSampleEntry
     * @see VisualSampleEntry
     * @see TextSampleEntry
     */
    public abstract class AbstractSampleEntry : AbstractContainerBox, SampleEntry
    {
        protected int dataReferenceIndex = 1;

        protected AbstractSampleEntry(string type) : base(type)
        { }

        public int getDataReferenceIndex()
        {
            return dataReferenceIndex;
        }

        public void setDataReferenceIndex(int dataReferenceIndex)
        {
            this.dataReferenceIndex = dataReferenceIndex;
        }

        public override abstract void parse(ReadableByteChannel dataSource, ByteBuffer header, long contentSize, BoxParser boxParser);

        public override abstract void getBox(WritableByteChannel writableByteChannel);
    }
}
