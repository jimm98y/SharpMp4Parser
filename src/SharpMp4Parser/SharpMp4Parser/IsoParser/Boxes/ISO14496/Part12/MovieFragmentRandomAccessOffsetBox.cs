﻿/*
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

using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * aligned(8) class MovieFragmentRandomAccessOffsetBox
     * extends FullBox('mfro', version, 0) {
     * unsigned int(32) size;
     * }
     */
    public class MovieFragmentRandomAccessOffsetBox : AbstractFullBox
    {
        public const string TYPE = "mfro";
        private long mfraSize;

        public MovieFragmentRandomAccessOffsetBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return 8;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            mfraSize = IsoTypeReader.readUInt32(content);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt32(byteBuffer, mfraSize);
        }

        public long getMfraSize()
        {
            return mfraSize;
        }

        public void setMfraSize(long mfraSize)
        {
            this.mfraSize = mfraSize;
        }
    }
}