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

namespace SharpMp4Parser.IsoParser.Boxes.Apple
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     */
    public class AppleDataReferenceBox : AbstractFullBox
    {
        public const string TYPE = "rdrf";
        private int dataReferenceSize;
        private string dataReferenceType;
        private string dataReference;

        public AppleDataReferenceBox() : base(TYPE)
        { }


        protected override long getContentSize()
        {
            return 12 + dataReferenceSize;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            dataReferenceType = IsoTypeReader.read4cc(content);
            dataReferenceSize = CastUtils.l2i(IsoTypeReader.readUInt32(content));
            dataReference = IsoTypeReader.readString(content, dataReferenceSize);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            byteBuffer.put(IsoFile.fourCCtoBytes(dataReferenceType));
            IsoTypeWriter.writeUInt32(byteBuffer, dataReferenceSize);
            byteBuffer.put(Utf8.convert(dataReference));
        }

        public long getDataReferenceSize()
        {
            return dataReferenceSize;
        }

        public string getDataReferenceType()
        {
            return dataReferenceType;
        }

        public string getDataReference()
        {
            return dataReference;
        }
    }
}
