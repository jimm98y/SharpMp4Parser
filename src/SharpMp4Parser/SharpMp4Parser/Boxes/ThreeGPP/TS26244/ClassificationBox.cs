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
using SharpMp4Parser.Support;
using SharpMp4Parser.Tools;
using System.Text;

namespace SharpMp4Parser.Boxes.ThreeGPP.TS26244
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * Classification of the media according to 3GPP 26.244.
     */
    public class ClassificationBox : AbstractFullBox
    {
        public const string TYPE = "clsf";


        private string classificationEntity;
        private int classificationTableIndex;
        private string language;
        private string classificationInfo;

        public ClassificationBox() : base(TYPE)
        {  }

        public string getLanguage()
        {
            return language;
        }

        public void setLanguage(string language)
        {
            this.language = language;
        }

        public string getClassificationEntity()
        {
            return classificationEntity;
        }

        public void setClassificationEntity(string classificationEntity)
        {
            this.classificationEntity = classificationEntity;
        }

        public int getClassificationTableIndex()
        {
            return classificationTableIndex;
        }

        public void setClassificationTableIndex(int classificationTableIndex)
        {
            this.classificationTableIndex = classificationTableIndex;
        }

        public string getClassificationInfo()
        {
            return classificationInfo;
        }

        public void setClassificationInfo(string classificationInfo)
        {
            this.classificationInfo = classificationInfo;
        }

        protected override long getContentSize()
        {
            return 4 + 2 + 2 + Utf8.utf8StringLengthInBytes(classificationInfo) + 1;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            byte[] cE = new byte[4];
            content.get(cE);
            classificationEntity = IsoFile.bytesToFourCC(cE);
            classificationTableIndex = IsoTypeReader.readUInt16(content);
            language = IsoTypeReader.readIso639(content);
            classificationInfo = IsoTypeReader.readString(content);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            byteBuffer.put(IsoFile.fourCCtoBytes(classificationEntity));
            IsoTypeWriter.writeUInt16(byteBuffer, classificationTableIndex);
            IsoTypeWriter.writeIso639(byteBuffer, language);
            byteBuffer.put(Utf8.convert(classificationInfo));
            byteBuffer.put((byte)0);
        }


        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("ClassificationBox[language=").Append(getLanguage());
            buffer.Append("classificationEntity=").Append(getClassificationEntity());
            buffer.Append(";classificationTableIndex=").Append(getClassificationTableIndex());
            buffer.Append(";language=").Append(getLanguage());
            buffer.Append(";classificationInfo=").Append(getClassificationInfo());
            buffer.Append("]");
            return buffer.ToString();
        }
    }
}
