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

using System.Diagnostics;

namespace SharpMp4Parser.Boxes.Dece
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * <pre>
     * AssetInformationBox as defined the DECE Common File Format Spec.
     * aligned(8) class AssetInformationBox
     * extends FullBox(‘ainf’, version=1, flags)
     * {
     *  string 				mimeSubtypeName;
     *  string				codecs;
     *  unsigned int(8) 	encrypted;
     *  unsigned int(8) 	entry_count;
     *  for( int i=0; i &lt; entry_count; i++)
     *  {
     *   string	namespace;
     *   string	profile-level-idc;
     *   string	asset_id;
     *  }
     * }
     * </pre>
     */
    public class AssetInformationBox : AbstractFullBox
    {
        public const string TYPE = "ainf";

        string apid = "";
        string profileVersion = "0000";

        public AssetInformationBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return Utf8.utf8StringLengthInBytes(apid) + 9;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            if (getVersion() == 0)
            {
                byteBuffer.put(Utf8.convert(profileVersion), 0, 4);
                byteBuffer.put(Utf8.convert(apid));
                byteBuffer.put((byte)0);
            }
            else
            {
                throw new RuntimeException("Unknown ainf version " + getVersion());
            }
        }

        public override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            profileVersion = IsoTypeReader.readString(content, 4);
            apid = IsoTypeReader.readString(content);
        }

        public string getApid()
        {
            return apid;
        }

        public void setApid(string apid)
        {
            this.apid = apid;
        }

        public string getProfileVersion()
        {
            return profileVersion;
        }

        public void setProfileVersion(string profileVersion)
        {
            Debug.Assert(profileVersion != null && profileVersion.length() == 4);
            this.profileVersion = profileVersion;
        }

        public bool isHidden()
        {
            return (getFlags() & 1) == 1;
        }

        public void setHidden(bool hidden)
        {
            int flags = getFlags();
            if (isHidden() ^ hidden)
            {
                if (hidden)
                {
                    setFlags(flags | 1);
                }
                else
                {
                    setFlags(flags & 0xFFFFFE);
                }
            }
        }

        public sealed class Entry
        {
            public string ns;
            public string profileLevelIdc;
            public string assetId;

            public Entry(string ns, string profileLevelIdc, string assetId)
            {
                this.ns = ns;
                this.profileLevelIdc = profileLevelIdc;
                this.assetId = assetId;
            }

            public override string ToString()
            {
                return "{" +
                        "namespace='" + ns + '\'' +
                            ", profileLevelIdc='" + profileLevelIdc + '\'' +
                            ", assetId='" + assetId + '\'' +
                            '}';
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || getClass() != o.getClass()) return false;

                Entry entry = (Entry)o;

                if (!assetId.Equals(entry.assetId)) return false;
                if (!ns.Equals(entry.ns)) return false;
                if (!profileLevelIdc.Equals(entry.profileLevelIdc)) return false;

                return true;
            }

            public override int GetHashCode()
            {
                int result = ns.GetHashCode();
                result = 31 * result + profileLevelIdc.GetHashCode();
                result = 31 * result + assetId.GetHashCode();
                return result;
            }

            public int getSize()
            {
                return 3 + Utf8.utf8StringLengthInBytes(ns) +
                            Utf8.utf8StringLengthInBytes(profileLevelIdc) + Utf8.utf8StringLengthInBytes(assetId);
            }
        }
    }
}