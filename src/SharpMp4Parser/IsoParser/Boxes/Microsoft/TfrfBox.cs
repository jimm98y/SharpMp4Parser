﻿using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System.Collections.Generic;
using System.Text;

namespace SharpMp4Parser.IsoParser.Boxes.Microsoft
{
    /**
      * <h1>4cc = "uuid", d4807ef2-ca39-4695-8e54-26cb9e46a79f</h1>
      * The syntax of the fields defined in this section, specified in ABNF [RFC5234], is as follows:
      * TfrfBox = TfrfBoxLength TfrfBoxType [TfrfBoxLongLength] TfrfBoxUUID TfrfBoxFields
      * TfrfBoxChildren
      * TfrfBoxType = "u" "u" "i" "d"
      * TfrfBoxLength = BoxLength
      * TfrfBoxLongLength = LongBoxLength
      * TfrfBoxUUID = %xD4 %x80 %x7E %xF2 %xCA %x39 %x46 %x95
      * %x8E %x54 %x26 %xCB %x9E %x46 %xA7 %x9F
      * TfrfBoxFields = TfrfBoxVersion
      * TfrfBoxFlags
      * FragmentCount
      * (1* TfrfBoxDataFields32) / (1* TfrfBoxDataFields64)
      * TfrfBoxVersion = %x00 / %x01
      * TfrfBoxFlags = 24*24 RESERVED_BIT
      * FragmentCount = UINT8
      * TfrfBoxDataFields32 = FragmentAbsoluteTime32
      * FragmentDuration32
      * TfrfBoxDataFields64 = FragmentAbsoluteTime64
      * FragmentDuration64
      * FragmentAbsoluteTime64 = UNSIGNED_INT32
      * FragmentDuration64 = UNSIGNED_INT32
      * FragmentAbsoluteTime64 = UNSIGNED_INT64
      * FragmentDuration64 = UNSIGNED_INT64
      * TfrfBoxChildren = *( VendorExtensionUUIDBox )
      */
    public class TfrfBox : AbstractFullBox
    {
        public List<Entry> entries = new List<Entry>();

        public TfrfBox() : base("uuid")
        { }

        public override byte[] getUserType()
        {
            return new byte[]{ 0xd4,  0x80,  0x7e,  0xf2,  0xca,  0x39,  0x46,
                 0x95,  0x8e,  0x54, 0x26,  0xcb,  0x9e,  0x46,  0xa7,  0x9f};
        }

        protected override long getContentSize()
        {
            return 5 + entries.Count * (getVersion() == 0x01 ? 16 : 8);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt8(byteBuffer, entries.Count);

            foreach (Entry entry in entries)
            {
                if (getVersion() == 0x01)
                {
                    IsoTypeWriter.writeUInt64(byteBuffer, entry.fragmentAbsoluteTime);
                    IsoTypeWriter.writeUInt64(byteBuffer, entry.fragmentAbsoluteDuration);
                }
                else
                {
                    IsoTypeWriter.writeUInt32(byteBuffer, entry.fragmentAbsoluteTime);
                    IsoTypeWriter.writeUInt32(byteBuffer, entry.fragmentAbsoluteDuration);
                }
            }
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            int fragmentCount = IsoTypeReader.readUInt8(content);

            for (int i = 0; i < fragmentCount; i++)
            {
                Entry entry = new Entry();
                if (getVersion() == 0x01)
                {
                    entry.fragmentAbsoluteTime = IsoTypeReader.readUInt64(content);
                    entry.fragmentAbsoluteDuration = IsoTypeReader.readUInt64(content);
                }
                else
                {
                    entry.fragmentAbsoluteTime = IsoTypeReader.readUInt32(content);
                    entry.fragmentAbsoluteDuration = IsoTypeReader.readUInt32(content);
                }
                entries.Add(entry);
            }
        }


        public long getFragmentCount()
        {
            return entries.Count;
        }

        public List<Entry> getEntries()
        {
            return entries;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("TfrfBox");
            sb.Append("{entries=").Append(entries);
            sb.Append('}');
            return sb.ToString();
        }

        public class Entry
        {
            public long fragmentAbsoluteTime;
            public long fragmentAbsoluteDuration;

            public long getFragmentAbsoluteTime()
            {
                return fragmentAbsoluteTime;
            }

            public long getFragmentAbsoluteDuration()
            {
                return fragmentAbsoluteDuration;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Entry");
                sb.Append("{fragmentAbsoluteTime=").Append(fragmentAbsoluteTime);
                sb.Append(", fragmentAbsoluteDuration=").Append(fragmentAbsoluteDuration);
                sb.Append('}');
                return sb.ToString();
            }
        }
    }
}
