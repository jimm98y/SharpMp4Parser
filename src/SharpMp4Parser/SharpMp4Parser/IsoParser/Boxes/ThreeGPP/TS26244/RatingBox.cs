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
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System.Text;

namespace SharpMp4Parser.IsoParser.Boxes.ThreeGPP.TS26244
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * Contained a the <code>UserDataBox</code> and containing information about the media's rating. E.g.
     * PG13or FSK16.
     */
    public class RatingBox : AbstractFullBox
    {
        public const string TYPE = "rtng";

        private string ratingEntity;
        private string ratingCriteria;
        private string language;
        private string ratingInfo;

        public RatingBox() : base(TYPE)
        { }

        public string getLanguage()
        {
            return language;
        }

        public void setLanguage(string language)
        {
            this.language = language;
        }

        /**
         * Gets a four-character code that indicates the rating entity grading the asset, e.g., 'BBFC'. The values of this
         * field should follow common names of worldwide movie rating systems, such as those mentioned in
         * [http://www.movie-ratings.net/, October 2002].
         *
         * @return the rating organization
         */
        public string getRatingEntity()
        {
            return ratingEntity;
        }

        public void setRatingEntity(string ratingEntity)
        {
            this.ratingEntity = ratingEntity;
        }

        /**
         * Gets the four-character code that indicates which rating criteria are being used for the corresponding rating
         * entity, e.g., 'PG13'.
         *
         * @return the actual rating
         */
        public string getRatingCriteria()
        {
            return ratingCriteria;
        }

        public void setRatingCriteria(string ratingCriteria)
        {
            this.ratingCriteria = ratingCriteria;
        }

        public string getRatingInfo()
        {
            return ratingInfo;
        }

        public void setRatingInfo(string ratingInfo)
        {
            this.ratingInfo = ratingInfo;
        }

        protected override long getContentSize()
        {
            return 15 + Utf8.utf8StringLengthInBytes(ratingInfo);
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            ratingEntity = IsoTypeReader.read4cc(content);
            ratingCriteria = IsoTypeReader.read4cc(content);
            language = IsoTypeReader.readIso639(content);
            ratingInfo = IsoTypeReader.readString(content);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            byteBuffer.put(IsoFile.fourCCtoBytes(ratingEntity));
            byteBuffer.put(IsoFile.fourCCtoBytes(ratingCriteria));
            IsoTypeWriter.writeIso639(byteBuffer, language);
            byteBuffer.put(Utf8.convert(ratingInfo));
            byteBuffer.put(0);
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("RatingBox[language=").Append(getLanguage());
            buffer.Append("ratingEntity=").Append(getRatingEntity());
            buffer.Append(";ratingCriteria=").Append(getRatingCriteria());
            buffer.Append(";language=").Append(getLanguage());
            buffer.Append(";ratingInfo=").Append(getRatingInfo());
            buffer.Append("]");
            return buffer.ToString();
        }
    }
}
