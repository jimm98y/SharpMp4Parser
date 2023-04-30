/*
 * Copyright 2009 castLabs GmbH, Berlin
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

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     */
    public class TrackFragmentBaseMediaDecodeTimeBox : AbstractFullBox
    {
        public const string TYPE = "tfdt";

        private long baseMediaDecodeTime;

        public TrackFragmentBaseMediaDecodeTimeBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return getVersion() == 0 ? 8 : 12;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            if (getVersion() == 1)
            {
                IsoTypeWriter.writeUInt64(byteBuffer, baseMediaDecodeTime);
            }
            else
            {
                IsoTypeWriter.writeUInt32(byteBuffer, baseMediaDecodeTime);
            }
        }

        public override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            if (getVersion() == 1)
            {
                baseMediaDecodeTime = IsoTypeReader.readUInt64(content);
            }
            else
            {
                baseMediaDecodeTime = IsoTypeReader.readUInt32(content);
            }
        }

        public long getBaseMediaDecodeTime()
        {
            return baseMediaDecodeTime;
        }

        public void setBaseMediaDecodeTime(long baseMediaDecodeTime)
        {
            this.baseMediaDecodeTime = baseMediaDecodeTime;
        }

        public override string ToString()
        {
            return "TrackFragmentBaseMediaDecodeTimeBox{" +
                    "baseMediaDecodeTime=" + baseMediaDecodeTime +
                    '}';
        }
    }
}
