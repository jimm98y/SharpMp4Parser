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
using System.Collections.Generic;
using System.Text;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * This box identifies the specifications to which this file complies. <br>
     * Each brand is a printable four-character code, registered with ISO, that
     * identifies a precise specification.
     */
    public class SegmentTypeBox : AbstractBox
    {
        public const string TYPE = "styp";

        private string majorBrand;
        private long minorVersion;
        private List<string> compatibleBrands = new List<string>();

        public SegmentTypeBox() : base(TYPE)
        { }

        public SegmentTypeBox(string majorBrand, long minorVersion, List<string> compatibleBrands) : base(TYPE)
        {
            this.majorBrand = majorBrand;
            this.minorVersion = minorVersion;
            this.compatibleBrands = compatibleBrands;
        }

        protected override long getContentSize()
        {
            return 8 + compatibleBrands.Count * 4;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            majorBrand = IsoTypeReader.read4cc(content);
            minorVersion = IsoTypeReader.readUInt32(content);
            int compatibleBrandsCount = content.remaining() / 4;
            compatibleBrands = new List<string>();
            for (int i = 0; i < compatibleBrandsCount; i++)
            {
                compatibleBrands.Add(IsoTypeReader.read4cc(content));
            }
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            byteBuffer.put(IsoFile.fourCCtoBytes(majorBrand));
            IsoTypeWriter.writeUInt32(byteBuffer, minorVersion);
            foreach (string compatibleBrand in compatibleBrands)
            {
                byteBuffer.put(IsoFile.fourCCtoBytes(compatibleBrand));
            }
        }

        /**
         * Gets the brand identifier.
         *
         * @return the brand identifier
         */
        public string getMajorBrand()
        {
            return majorBrand;
        }

        /**
         * Sets the major brand of the file used to determine an appropriate reader.
         *
         * @param majorBrand the new major brand
         */
        public void setMajorBrand(string majorBrand)
        {
            this.majorBrand = majorBrand;
        }

        /**
         * Gets an informative integer for the minor version of the major brand.
         *
         * @return an informative integer
         * @see SegmentTypeBox#getMajorBrand()
         */
        public long getMinorVersion()
        {
            return minorVersion;
        }

        /**
         * Sets the "informative integer for the minor version of the major brand".
         *
         * @param minorVersion the version number of the major brand
         */
        public void setMinorVersion(long minorVersion)
        {
            this.minorVersion = minorVersion;
        }

        /**
         * Gets an array of 4-cc brands.
         *
         * @return the compatible brands
         */
        public List<string> getCompatibleBrands()
        {
            return compatibleBrands;
        }

        public void setCompatibleBrands(List<string> compatibleBrands)
        {
            this.compatibleBrands = compatibleBrands;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("SegmentTypeBox[");
            result.Append("majorBrand=").Append(getMajorBrand());
            result.Append(";");
            result.Append("minorVersion=").Append(getMinorVersion());
            foreach (string compatibleBrand in compatibleBrands)
            {
                result.Append(";");
                result.Append("compatibleBrand=").Append(compatibleBrand);
            }
            result.Append("]");
            return result.ToString();
        }
    }
}