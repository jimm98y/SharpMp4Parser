using SharpMp4Parser.Java;
using SharpMp4Parser.Support;
using SharpMp4Parser.Tools;
using System.Collections.Generic;

namespace SharpMp4Parser.Boxes.Dece
{
    /**
     * <pre>
     * aligned(8) class ContentInformationBox
     * extends FullBox(‘cinf’, version=0, flags=0)
     * {
     *  string          mimeSubtypeName;
     *  string          profile-level-idc;
     *  string          codecs;
     *  unsigned int(8) protection;
     *  string          languages;
     *  unsigned int(8) brand_entry_count;
     *  for( int i=0; i &lt; brand_entry_count; i++)
     *  {
     *   string iso_brand;
     *   string version
     *  }
     *  unsigned int(8) id_entry_count;
     *  for( int i=0; i &lt; id_entry_count; i++)
     *  {
     *   string namespace;
     *   string asset_id;
     *  }
     * }
     * </pre>
     */
    public class ContentInformationBox : AbstractFullBox
    {
        public const string TYPE = "cinf";

        string mimeSubtypeName;
        string profileLevelIdc;
        string codecs;
        string protection;
        string languages;

        Dictionary<string, string> brandEntries = new Dictionary<string, string>();
        Dictionary<string, string> idEntries = new Dictionary<string, string>();

        public ContentInformationBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            long size = 4;
            size += Utf8.utf8StringLengthInBytes(mimeSubtypeName) + 1;
            size += Utf8.utf8StringLengthInBytes(profileLevelIdc) + 1;
            size += Utf8.utf8StringLengthInBytes(codecs) + 1;
            size += Utf8.utf8StringLengthInBytes(protection) + 1;
            size += Utf8.utf8StringLengthInBytes(languages) + 1;
            size += 1;
            foreach (var brandEntry in brandEntries)
            {
                size += Utf8.utf8StringLengthInBytes(brandEntry.Key) + 1;
                size += Utf8.utf8StringLengthInBytes(brandEntry.Value) + 1;
            }
            size += 1;
            foreach (var idEntry in idEntries)
            {
                size += Utf8.utf8StringLengthInBytes(idEntry.Key) + 1;
                size += Utf8.utf8StringLengthInBytes(idEntry.Value) + 1;

            }
            return size;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeZeroTermUtf8String(byteBuffer, mimeSubtypeName);
            IsoTypeWriter.writeZeroTermUtf8String(byteBuffer, profileLevelIdc);
            IsoTypeWriter.writeZeroTermUtf8String(byteBuffer, codecs);
            IsoTypeWriter.writeZeroTermUtf8String(byteBuffer, protection);
            IsoTypeWriter.writeZeroTermUtf8String(byteBuffer, languages);
            IsoTypeWriter.writeUInt8(byteBuffer, brandEntries.Count);
            foreach (var brandEntry in brandEntries)
            {
                IsoTypeWriter.writeZeroTermUtf8String(byteBuffer, brandEntry.Key);
                IsoTypeWriter.writeZeroTermUtf8String(byteBuffer, brandEntry.Value);
            }
            IsoTypeWriter.writeUInt8(byteBuffer, idEntries.Count);
            foreach (var idEntry in idEntries)
            {
                IsoTypeWriter.writeZeroTermUtf8String(byteBuffer, idEntry.Key);
                IsoTypeWriter.writeZeroTermUtf8String(byteBuffer, idEntry.Value);
            }
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            mimeSubtypeName = IsoTypeReader.readString(content);
            profileLevelIdc = IsoTypeReader.readString(content);
            codecs = IsoTypeReader.readString(content);
            protection = IsoTypeReader.readString(content);
            languages = IsoTypeReader.readString(content);
            int brandEntryCount = IsoTypeReader.readUInt8(content);
            while (brandEntryCount-- > 0)
            {
                brandEntries.Add(IsoTypeReader.readString(content), IsoTypeReader.readString(content));
            }
            int idEntryCount = IsoTypeReader.readUInt8(content);
            while (idEntryCount-- > 0)
            {
                idEntries.Add(IsoTypeReader.readString(content), IsoTypeReader.readString(content));
            }
        }

        public string getMimeSubtypeName()
        {
            return mimeSubtypeName;
        }

        public void setMimeSubtypeName(string mimeSubtypeName)
        {
            this.mimeSubtypeName = mimeSubtypeName;
        }

        public string getProfileLevelIdc()
        {
            return profileLevelIdc;
        }

        public void setProfileLevelIdc(string profileLevelIdc)
        {
            this.profileLevelIdc = profileLevelIdc;
        }

        public string getCodecs()
        {
            return codecs;
        }

        public void setCodecs(string codecs)
        {
            this.codecs = codecs;
        }

        public string getProtection()
        {
            return protection;
        }

        public void setProtection(string protection)
        {
            this.protection = protection;
        }

        public string getLanguages()
        {
            return languages;
        }

        public void setLanguages(string languages)
        {
            this.languages = languages;
        }

        public Dictionary<string, string> getBrandEntries()
        {
            return brandEntries;
        }

        public void setBrandEntries(Dictionary<string, string> brandEntries)
        {
            this.brandEntries = brandEntries;
        }

        public Dictionary<string, string> getIdEntries()
        {
            return idEntries;
        }

        public void setIdEntries(Dictionary<string, string> idEntries)
        {
            this.idEntries = idEntries;
        }

        public sealed class BrandEntry
        {
            string iso_brand;
            string version;

            public BrandEntry(string iso_brand, string version)
            {
                this.iso_brand = iso_brand;
                this.version = version;
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || this.GetType() != o.GetType()) return false;

                BrandEntry that = (BrandEntry)o;

                if (iso_brand != null ? !iso_brand.Equals(that.iso_brand) : that.iso_brand != null) return false;
                if (version != null ? !version.Equals(that.version) : that.version != null) return false;

                return true;
            }

            public override int GetHashCode()
            {
                int result = iso_brand != null ? iso_brand.GetHashCode() : 0;
                result = 31 * result + (version != null ? version.GetHashCode() : 0);
                return result;
            }
        }
    }
}
