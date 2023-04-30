using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpMp4Parser.IsoParser.Boxes.Microsoft.ContentProtection
{
    /**
     * Specifications &gt; Microsoft PlayReady Format Specification &gt; 2. PlayReady Media Format &gt; 2.7. ASF GUIDs
     * <p>
     * ASF_Protection_System_Identifier_Object
     * 9A04F079-9840-4286-AB92E65BE0885F95</p>
     * <p>
     * ASF_Content_Protection_System_Microsoft_PlayReady
     * F4637010-03C3-42CD-B932B48ADF3A6A54    </p>
     * <p>
     * ASF_StreamType_PlayReady_Encrypted_Command_Media
     * 8683973A-6639-463A-ABD764F1CE3EEAE0</p>
     * <p>
     * Specifications &gt; Microsoft PlayReady Format Specification &gt; 2. PlayReady Media Format &gt; 2.5. Data Objects &gt; 2.5.1. Payload TrackExtension for AES in Counter Mode</p>
     * <p>
     * The sample Id is used as the IV in CTR mode. Block offset, starting at 0 and incremented by 1 after every 16 bytes, from the beginning of the sample is used as the Counter.</p>
     * <p>
     * The sample ID for each sample (media object) is stored as an ASF payload extension system with the ID of ASF_Payload_Extension_Encryption_SampleID = {6698B84E-0AFA-4330-AEB2-1C0A98D7A44D}. The payload extension can be stored as a fixed size extension of 8 bytes.</p>
     * <p>
     * The sample ID is always stored in big-endian byte order.</p>
     */
    public class PlayReadyHeader : ProtectionSpecificHeader
    {
        public static readonly Uuid PROTECTION_SYSTEM_ID = Uuid.Parse("9A04F079-9840-4286-AB92-E65BE0885F95");

        static PlayReadyHeader()
        {
            uuidRegistry.Add(PROTECTION_SYSTEM_ID, typeof(PlayReadyHeader));
        }

        private long length;
        private List<PlayReadyRecord> records;

        public override Uuid getSystemId()
        {
            return PROTECTION_SYSTEM_ID;
        }

        public override void parse(ByteBuffer byteBuffer)
        {
            /*
            Length DWORD 32

            PlayReady Record Count WORD 16

            PlayReady Records See Text Varies
            */

            length = IsoTypeReader.readUInt32BE(byteBuffer);
            int recordCount = IsoTypeReader.readUInt16BE(byteBuffer);

            records = PlayReadyRecord.createFor(byteBuffer, recordCount);
        }

        public override ByteBuffer getData()
        {

            int size = 4 + 2;
            foreach (PlayReadyRecord record in records)
            {
                size += 2 + 2;
                size += record.getValue().rewind().limit();
            }
            ByteBuffer byteBuffer = ByteBuffer.allocate(size);

            IsoTypeWriter.writeUInt32BE(byteBuffer, size);
            IsoTypeWriter.writeUInt16BE(byteBuffer, records.Count);
            foreach (PlayReadyRecord record in records)
            {
                IsoTypeWriter.writeUInt16BE(byteBuffer, record.type);
                IsoTypeWriter.writeUInt16BE(byteBuffer, record.getValue().limit());
                ByteBuffer tmp4debug = record.getValue();
                byteBuffer.put(tmp4debug);
            }

            return byteBuffer;
        }

        public List<PlayReadyRecord> getRecords()
        {
            return records.ToList();
        }

        public void setRecords(List<PlayReadyRecord> records)
        {
            this.records = records;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("PlayReadyHeader");
            sb.Append("{length=").Append(length);
            sb.Append(", recordCount=").Append(records.Count);
            sb.Append(", records=").Append(records);
            sb.Append('}');
            return sb.ToString();
        }

        public abstract class PlayReadyRecord
        {
            public int type;


            public PlayReadyRecord(int type)
            {
                this.type = type;
            }

            public static List<PlayReadyRecord> createFor(ByteBuffer byteBuffer, int recordCount)
            {
                List<PlayReadyRecord> records = new List<PlayReadyRecord>(recordCount);

                for (int i = 0; i < recordCount; i++)
                {
                    PlayReadyRecord record;
                    int type = IsoTypeReader.readUInt16BE(byteBuffer);
                    int length = IsoTypeReader.readUInt16BE(byteBuffer);
                    switch (type)
                    {
                        case 0x1:
                            record = new RMHeader();
                            break;
                        case 0x2:
                            record = new DefaulPlayReadyRecord(0x02);
                            break;
                        case 0x3:
                            record = new EmeddedLicenseStore();
                            break;
                        default:
                            record = new DefaulPlayReadyRecord(type);
                            break;
                    }
                    record.parse(byteBuffer.slice().limit(length));
                    ((Java.Buffer)byteBuffer).position(byteBuffer.position() + length);
                    records.Add(record);
                }

                return records;
            }

            public abstract void parse(ByteBuffer bytes);

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("PlayReadyRecord");
                sb.Append("{type=").Append(type);
                sb.Append(", length=").Append(getValue().limit());
                //            sb.append(", value=").append(Hex.encodeHex(getValue())).append('\'');
                sb.Append('}');
                return sb.ToString();
            }

            public abstract ByteBuffer getValue();

            public sealed class RMHeader : PlayReadyRecord
            {
                string header;

                public RMHeader() : base(0x01)
                { }

                public override void parse(ByteBuffer bytes)
                {
                    try
                    {
                        byte[] str = new byte[bytes.slice().limit()];
                        bytes.get(str);
                        header = Encoding.GetEncoding("UTF-16LE").GetString(str);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                public override ByteBuffer getValue()
                {
                    byte[] headerBytes;
                    try
                    {
                        headerBytes = Encoding.GetEncoding("UTF-16LE").GetBytes(header);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    return ByteBuffer.wrap(headerBytes);
                }

                public string getHeader()
                {
                    return header;
                }

                public void setHeader(string header)
                {
                    this.header = header;
                }

                public override string ToString()
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("RMHeader");
                    sb.Append("{length=").Append(getValue().limit());
                    sb.Append(", header='").Append(header).Append('\'');
                    sb.Append('}');
                    return sb.ToString();
                }
            }

            public sealed class EmeddedLicenseStore : PlayReadyRecord
            {
                ByteBuffer value;

                public EmeddedLicenseStore() : base(0x03)
                { }

                public override void parse(ByteBuffer bytes)
                {
                    value = bytes.duplicate();
                }

                public override ByteBuffer getValue()
                {
                    return value;
                }

                public override string ToString()
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("EmeddedLicenseStore");
                    sb.Append("{length=").Append(getValue().limit());
                    //sb.append(", value='").append(Hex.encodeHex(getValue())).append('\'');
                    sb.Append('}');
                    return sb.ToString();
                }
            }

            public sealed class DefaulPlayReadyRecord : PlayReadyRecord
            {
                ByteBuffer value;

                public DefaulPlayReadyRecord(int type) : base(type)
                {

                }

                public override void parse(ByteBuffer bytes)
                {
                    value = bytes.duplicate();
                }

                public override ByteBuffer getValue()
                {
                    return value;
                }
            }
        }
    }
}
