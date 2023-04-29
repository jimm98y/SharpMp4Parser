/*
 * Copyright 2011 castLabs, Berlin
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

namespace SharpMp4Parser.Boxes.Dece
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     */
    public class BaseLocationBox : AbstractFullBox
    {
        public const string TYPE = "bloc";

        string baseLocation = "";
        string purchaseLocation = "";

        public BaseLocationBox() : base(TYPE)
        { }

        public BaseLocationBox(string baseLocation, string purchaseLocation) : base(TYPE)
        {
            this.baseLocation = baseLocation;
            this.purchaseLocation = purchaseLocation;
        }

        public string getBaseLocation()
        {
            return baseLocation;
        }

        public void setBaseLocation(string baseLocation)
        {
            this.baseLocation = baseLocation;
        }

        public string getPurchaseLocation()
        {
            return purchaseLocation;
        }

        public void setPurchaseLocation(string purchaseLocation)
        {
            this.purchaseLocation = purchaseLocation;
        }

        protected override long getContentSize()
        {
            return 1028;
        }

        public override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            baseLocation = IsoTypeReader.readString(content);
            content.get(new byte[256 - Utf8.utf8StringLengthInBytes(baseLocation) - 1]);
            purchaseLocation = IsoTypeReader.readString(content);
            content.get(new byte[256 - Utf8.utf8StringLengthInBytes(purchaseLocation) - 1]);
            content.get(new byte[512]);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            byteBuffer.put(Utf8.convert(baseLocation));
            byteBuffer.put(new byte[256 - Utf8.utf8StringLengthInBytes(baseLocation)]); // string plus term zero
            byteBuffer.put(Utf8.convert(purchaseLocation));
            byteBuffer.put(new byte[256 - Utf8.utf8StringLengthInBytes(purchaseLocation)]); // string plus term zero
            byteBuffer.put(new byte[512]);
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || getClass() != o.getClass()) return false;

            BaseLocationBox that = (BaseLocationBox)o;

            if (baseLocation != null ? !baseLocation.equals(that.baseLocation) : that.baseLocation != null) return false;
            if (purchaseLocation != null ? !purchaseLocation.equals(that.purchaseLocation) : that.purchaseLocation != null)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            int result = baseLocation != null ? baseLocation.GetHashCode() : 0;
            result = 31 * result + (purchaseLocation != null ? purchaseLocation.GetHashCode() : 0);
            return result;
        }

        public override string ToString()
        {
            return "BaseLocationBox{" +
                    "baseLocation='" + baseLocation + '\'' +
                    ", purchaseLocation='" + purchaseLocation + '\'' +
                    '}';
        }
    }
}
