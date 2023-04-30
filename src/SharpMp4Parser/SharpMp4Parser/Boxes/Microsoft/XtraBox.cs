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

using System;
using System.Text;

namespace SharpMp4Parser.Boxes.Microsoft
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * Windows Media Xtra Box.
     * <br>
     * I can't find definitive documentation on this from Microsoft so it's cobbled together from
     * various sources. Mostly ExifTool for Perl.
     * <br>
     * Various references:
     * https://msdn.microsoft.com/en-us/library/windows/desktop/dd743066(v=vs.85).aspx
     * https://metacpan.org/source/EXIFTOOL/Image-ExifTool-9.76/lib/Image/ExifTool/Microsoft.pm
     * http://www.ventismedia.com/mantis/view.php?id=12017
     * http://www.hydrogenaudio.org/forums/index.php?showtopic=75123&amp;st=250
     * http://www.mediamonkey.com/forum/viewtopic.php?f=1&amp;t=76321
     * https://code.google.com/p/mp4v2/issues/detail?id=113
     *
     * @author marwatk
     */
    public class XtraBox : AbstractBox
    {
        //private static Logger LOG = LoggerFactory.getLogger(XtraBox.class);
        public const string TYPE = "Xtra";

        public const int MP4_XTRA_BT_UNICODE = 8;
        public const int MP4_XTRA_BT_INT64 = 19;
        public const int MP4_XTRA_BT_FILETIME = 21;
        public const int MP4_XTRA_BT_GUID = 72;
        //http://stackoverflow.com/questions/5398557/java-library-for-dealing-with-win32-filetime
        private const long FILETIME_EPOCH_DIFF = 11644473600000L;
        private const long FILETIME_ONE_MILLISECOND = 10 * 1000;
        Vector<XtraTag> tags = new Vector<XtraTag>();
        ByteBuffer data;
        private bool successfulParse = false;

        public XtraBox() : base("Xtra")
        { }

        public XtraBox(string type) : base(type)
        { }

        private static long filetimeToMillis(long filetime)
        {
            return (filetime / FILETIME_ONE_MILLISECOND) - FILETIME_EPOCH_DIFF;
        }

        private static long millisToFiletime(long millis)
        {
            return (millis + FILETIME_EPOCH_DIFF) * FILETIME_ONE_MILLISECOND;
        }

        private static void writeAsciiString(ByteBuffer dest, string s)
        {
            try
            {
                dest.Add(s.getBytes("US-ASCII"));
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private static string readAsciiString(ByteBuffer content, int length)
        {
            byte[] s = new byte[length];
            content.get(s);
            try
            {
                return new string(s, "US-ASCII");
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private static string readUtf16String(ByteBuffer content, int length)
        {
            char[] s = new char[(length / 2) - 1];
            for (int i = 0; i < (length / 2) - 1; i++)
            {
                s[i] = content.getChar();
            }
            content.getChar(); //Discard terminating null
            return new string(s);
        }

        private static void writeUtf16String(ByteBuffer dest, string s)
        {
            char[] ar = s.toCharArray();
            for (int i = 0; i < ar.Length; i++)
            { //Probably not the best way to do this but it preserves the byte order
                dest.putChar(ar[i]);
            }
            dest.putChar((char)0); //Terminating null
        }

        protected override long getContentSize()
        {
            if (successfulParse)
            {
                return detailSize();
            }
            else
            {
                return data.limit();
            }
        }

        private int detailSize()
        {
            int size = 0;
            for (int i = 0; i < tags.size(); i++)
            {
                size += tags.elementAt(i).getContentSize();
            }
            return size;

        }

        public override string ToString()
        {
            if (!this.isParsed())
            {
                this.parseDetails();
            }
            StringBuilder b = new StringBuilder();
            b.Append("XtraBox[");
            foreach (XtraTag tag in tags)
            {
                foreach (XtraValue value in tag.values)
                {
                    b.Append(tag.tagName);
                    b.Append("=");
                    b.Append(value.toString());
                    b.Append(";");
                }
            }
            b.Append("]");
            return b.ToString();
        }

        public override void _parseDetails(ByteBuffer content)
        {
            int boxSize = content.remaining();
            data = content.slice(); //Keep this in case we fail to parse
            successfulParse = false;
            try
            {
                tags.clear();
                while (content.remaining() > 0)
                {
                    XtraTag tag = new XtraTag();
                    tag.parse(content);
                    tags.addElement(tag);
                }
                int calcSize = detailSize();
                if (boxSize != calcSize)
                {
                    throw new Exception("Improperly handled Xtra tag: Calculated sizes don't match ( " + boxSize + "/" + calcSize + ")");
                }
                successfulParse = true;
            }
            catch (Exception e)
            {
                successfulParse = false;
                //LOG.error("Malformed Xtra Tag detected: {}", e.ToString());
                ((Buffer)content).position(content.position() + content.remaining());
            }
            finally
            {
                content.order(ByteOrder.BIG_ENDIAN); //Just in case we bailed out mid-parse we don't want to leave the byte order in MS land
            }
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            if (successfulParse)
            {
                for (int i = 0; i < tags.size(); i++)
                {
                    tags.elementAt(i).getContent(byteBuffer);
                }
            }
            else
            {
                ((Buffer)data).rewind();
                byteBuffer.put(data);
            }
        }

        /**
         * Returns a list of the tag names present in this Xtra Box
         *
         * @return Possibly empty (zero length) array of tag names present
         */
        public string[] getAllTagNames()
        {
            string[] names = new string[tags.size()];
            for (int i = 0; i < tags.size(); i++)
            {
                XtraTag tag = tags.elementAt(i);
                names[i] = tag.tagName;
            }
            return names;
        }

        /**
         * Returns the first String value found for this tag
         *
         * @param name Tag name
         * @return First String value found
         */
        public string getFirstStringValue(string name)
        {
            object[] objs = getValues(name);
            foreach (object obj in objs)
            {
                if (obj is string) {
                    return (string)obj;
                }
            }
            return null;
        }

        /**
         * Returns the first Date value found for this tag
         *
         * @param name Tag name
         * @return First Date value found
         */
        public DateTime? getFirstDateValue(string name)
        {
            object[] objs = getValues(name);
            foreach (object obj in objs)
            {
                if (obj is DateTime)
                {
                    return (DateTime)obj;
                }
            }
            return null;
        }

        /**
         * Returns the first Long value found for this tag
         *
         * @param name Tag name
         * @return First long value found
         */
        public long? getFirstLongValue(string name)
        {
            object[] objs = getValues(name);
            foreach (object obj in objs)
            {
                if (obj is long)
                {
                    return (long)obj;
                }
            }
            return null;
        }

        /**
         * Returns an array of values for this tag. Empty array when tag is not present
         *
         * @param name Tag name to retrieve
         * @return Possibly empty array of values (possible types are String, Long, Date and byte[] )
         */
        public object[] getValues(string name)
        {
            XtraTag tag = getTagByName(name);
            object[] values;
            if (tag != null)
            {
                values = new object[tag.values.size()];
                for (int i = 0; i < tag.values.size(); i++)
                {
                    values[i] = tag.values.elementAt(i).getValueAsObject();
                }
            }
            else
            {
                values = new object[0];
            }
            return values;
        }

        /**
         * Removes specified tag (all values for that tag will be removed)
         *
         * @param name Tag to remove
         */
        public void removeTag(string name)
        {
            XtraTag tag = getTagByName(name);
            if (tag != null)
            {
                tags.remove(tag);
            }
        }

        /**
         * Removes and recreates tag using specified String values
         *
         * @param name   Tag name to replace
         * @param values New String values
         */
        public void setTagValues(string name, string[] values)
        {
            removeTag(name);
            XtraTag tag = new XtraTag(name);
            for (int i = 0; i < values.length; i++)
            {
                tag.values.addElement(new XtraValue(values[i]));
            }
            tags.addElement(tag);
        }

        /**
         * Removes and recreates tag using specified String value
         *
         * @param name  Tag name to replace
         * @param value New String value
         */
        public void setTagValue(string name, string value)
        {
            setTagValues(name, new string[] { value });
        }

        /**
         * Removes and recreates tag using specified Date value
         *
         * @param name Tag name to replace
         * @param date New Date value
         */
        public void setTagValue(string name, DateTime date)
        {
            removeTag(name);
            XtraTag tag = new XtraTag(name);
            tag.values.addElement(new XtraValue(date));
            tags.addElement(tag);
        }

        /**
         * Removes and recreates tag using specified Long value
         *
         * @param name  Tag name to replace
         * @param value New Long value
         */
        public void setTagValue(string name, long value)
        {
            removeTag(name);
            XtraTag tag = new XtraTag(name);
            tag.values.addElement(new XtraValue(value));
            tags.addElement(tag);
        }

        private XtraTag getTagByName(string name)
        {
            foreach (XtraTag tag in tags) {
                if (tag.tagName.equals(name))
                {
                    return tag;
                }
            }
            return null;
        }

        private sealed class XtraTag
        {
            private int inputSize; //For debugging only

            private string tagName;
            private Vector<XtraValue> values;

            private XtraTag()
            {
                values = new Vector<XtraValue>();
            }

            private XtraTag(string name) : this()
            {
                tagName = name;
            }

            private void parse(ByteBuffer content)
            {
                inputSize = content.getInt();
                int tagLength = content.getInt();
                tagName = readAsciiString(content, tagLength);
                int count = content.getInt();

                for (int i = 0; i < count; i++)
                {
                    XtraValue val = new XtraValue();
                    val.parse(content);
                    values.addElement(val);
                }
                if (inputSize != getContentSize())
                {
                    throw new Exception("Improperly handled Xtra tag: Sizes don't match ( " + inputSize + "/" + getContentSize() + ") on " + tagName);
                }
            }

            private void getContent(ByteBuffer b)
            {
                b.putInt(getContentSize());
                b.putInt(tagName.length());
                writeAsciiString(b, tagName);
                b.putInt(values.size());
                for (int i = 0; i < values.size(); i++)
                {
                    values.elementAt(i).getContent(b);
                }
            }

            private int getContentSize()
            {
                //Size: 4
                //TagLength: 4
                //Tag: tagLength;
                //Count: 4
                //Values: count * values.getContentSize();
                int size = 12 + tagName.length();
                for (int i = 0; i < values.size(); i++)
                {
                    size += values.elementAt(i).getContentSize();
                }
                return size;
            }

            public string toString()
            {
                StringBuilder b = new StringBuilder();
                b.Append(tagName);
                b.Append(" [");
                b.Append(inputSize);
                b.Append("/");
                b.Append(values.size());
                b.Append("]:\n");
                for (int i = 0; i < values.size(); i++)
                {
                    b.Append("  ");
                    b.Append(values.elementAt(i).toString());
                    b.Append("\n");
                }
                return b.ToString();
            }

        }

        private sealed class XtraValue
        {
            public int type;

            public string stringValue;
            public long longValue;
            public byte[] nonParsedValue;
            public DateTime fileTimeValue;

            private XtraValue()
            { }

            private XtraValue(string val)
            {
                type = MP4_XTRA_BT_UNICODE;
                stringValue = val;
            }

            private XtraValue(long longVal)
            {
                type = MP4_XTRA_BT_INT64;
                longValue = longVal;
            }

            private XtraValue(DateTime time)
            {
                type = MP4_XTRA_BT_FILETIME;
                fileTimeValue = time;
            }

            private object getValueAsObject()
            {
                switch (type)
                {
                    case MP4_XTRA_BT_UNICODE:
                        return stringValue;
                    case MP4_XTRA_BT_INT64:
                        return (long)(longValue);
                    case MP4_XTRA_BT_FILETIME:
                        return fileTimeValue;
                    case MP4_XTRA_BT_GUID:
                    default:
                        return nonParsedValue;
                }
            }

            private void parse(ByteBuffer content)
            {
                int length = content.getInt() - 6; //length + type are included in length
                type = content.getShort();
                content.order(ByteOrder.LITTLE_ENDIAN);
                switch (type)
                {
                    case MP4_XTRA_BT_UNICODE:
                        stringValue = readUtf16String(content, length);
                        break;
                    case MP4_XTRA_BT_INT64:
                        longValue = content.getLong();
                        break;
                    case MP4_XTRA_BT_FILETIME:
                        fileTimeValue = new DateTime(filetimeToMillis(content.getLong()));
                        break;
                    case MP4_XTRA_BT_GUID:
                    default:
                        nonParsedValue = new byte[length];
                        content.get(nonParsedValue);
                        break;

                }
                content.order(ByteOrder.BIG_ENDIAN);

            }

            private void getContent(ByteBuffer b)
            {
                try
                {
                    int length = getContentSize();
                    b.putInt(length);
                    b.putShort((short)type);
                    b.order(ByteOrder.LITTLE_ENDIAN);
                    switch (type)
                    {
                        case MP4_XTRA_BT_UNICODE:
                            writeUtf16String(b, stringValue);
                            break;
                        case MP4_XTRA_BT_INT64:
                            b.putLong(longValue);
                            break;
                        case MP4_XTRA_BT_FILETIME:
                            b.putLong(millisToFiletime(fileTimeValue.getTime()));
                            break;
                        case MP4_XTRA_BT_GUID:
                        default:
                            b.put(nonParsedValue);
                            break;
                    }
                }
                finally
                {
                    b.order(ByteOrder.BIG_ENDIAN);
                }
            }

            private int getContentSize()
            {
                //Length: 4 bytes
                //Type: 2 bytes
                //Content: length bytes
                int size = 6;

                switch (type)
                {
                    case MP4_XTRA_BT_UNICODE:
                        size += (stringValue.length() * 2) + 2; //Plus 2 for trailing null
                        break;
                    case MP4_XTRA_BT_INT64:
                    case MP4_XTRA_BT_FILETIME:
                        size += 8;
                        break;
                    case MP4_XTRA_BT_GUID:
                    default:
                        size += nonParsedValue.Length;
                        break;
                }
                return size;
            }

            public override string ToString()
            {
                switch (type)
                {
                    case MP4_XTRA_BT_UNICODE:
                        return "[string]" + stringValue;
                    case MP4_XTRA_BT_INT64:
                        return "[long]" + longValue;
                    case MP4_XTRA_BT_FILETIME:
                        return "[filetime]" + fileTimeValue.ToString();
                    case MP4_XTRA_BT_GUID:
                    default:
                        return "[GUID](nonParsed)";

                }
            }
        }
    }
}
