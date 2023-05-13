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

using System.Text;
using System;
using SharpMp4Parser.Java;
using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12
{
    /**
      * <h1>4cc = "{@value #TYPE}"</h1>
      * This box defines overall information which is media-independent, and relevant to the entire presentation
      * considered as a whole.
      */
    public class MediaHeaderBox : AbstractFullBox
    {
        public const string TYPE = "mdhd";
        //private static Logger LOG = LoggerFactory.getLogger(MediaHeaderBox.class);
        private DateTime creationTime = new DateTime();
        private DateTime modificationTime = new DateTime();
        private long timescale;
        private long duration;
        private string language = "eng";

        public MediaHeaderBox() : base(TYPE)
        { }

        public DateTime getCreationTime()
        {
            if(!isParsed)
            {
                parseDetails();
            }

            return creationTime;
        }

        public void setCreationTime(DateTime creationTime)
        {
            this.creationTime = creationTime;
        }

        public DateTime getModificationTime()
        {
            if (!isParsed)
            {
                parseDetails();
            }

            return modificationTime;
        }

        public void setModificationTime(DateTime modificationTime)
        {
            this.modificationTime = modificationTime;
        }

        public long getTimescale()
        {
            if (!isParsed)
            {
                parseDetails();
            }

            return timescale;
        }

        public void setTimescale(long timescale)
        {
            this.timescale = timescale;
        }

        public long getDuration()
        {
            if (!isParsed)
            {
                parseDetails();
            }

            return duration;
        }

        public void setDuration(long duration)
        {
            this.duration = duration;
        }

        public string getLanguage()
        {
            if (!isParsed)
            {
                parseDetails();
            }

            return language;
        }

        public void setLanguage(string language)
        {
            this.language = language;
        }

        protected override long getContentSize()
        {
            long contentSize = 4;
            if (getVersion() == 1)
            {
                contentSize += 8 + 8 + 4 + 8;
            }
            else
            {
                contentSize += 4 + 4 + 4 + 4;
            }
            contentSize += 2;
            contentSize += 2;
            return contentSize;

        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            if (getVersion() == 1)
            {
                creationTime = DateHelper.convert(IsoTypeReader.readUInt64(content));
                modificationTime = DateHelper.convert(IsoTypeReader.readUInt64(content));
                timescale = IsoTypeReader.readUInt32(content);
                duration = content.getLong();
            }
            else
            {
                creationTime = DateHelper.convert(IsoTypeReader.readUInt32(content));
                modificationTime = DateHelper.convert(IsoTypeReader.readUInt32(content));
                timescale = IsoTypeReader.readUInt32(content);
                duration = content.getInt();
            }
            if (duration < -1)
            {
                //LOG.warn("mdhd duration is not in expected range");
            }


            language = IsoTypeReader.readIso639(content);
            IsoTypeReader.readUInt16(content);
        }


        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("MediaHeaderBox[");
            result.Append("creationTime=").Append(getCreationTime());
            result.Append(";");
            result.Append("modificationTime=").Append(getModificationTime());
            result.Append(";");
            result.Append("timescale=").Append(getTimescale());
            result.Append(";");
            result.Append("duration=").Append(getDuration());
            result.Append(";");
            result.Append("language=").Append(getLanguage());
            result.Append("]");
            return result.ToString();
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            if (getVersion() == 1)
            {
                IsoTypeWriter.writeUInt64(byteBuffer, DateHelper.convert(creationTime));
                IsoTypeWriter.writeUInt64(byteBuffer, DateHelper.convert(modificationTime));
                IsoTypeWriter.writeUInt32(byteBuffer, timescale);
                byteBuffer.putLong(duration);
            }
            else
            {
                IsoTypeWriter.writeUInt32(byteBuffer, DateHelper.convert(creationTime));
                IsoTypeWriter.writeUInt32(byteBuffer, DateHelper.convert(modificationTime));
                IsoTypeWriter.writeUInt32(byteBuffer, timescale);
                byteBuffer.putInt((int)duration);
            }
            IsoTypeWriter.writeIso639(byteBuffer, language);
            IsoTypeWriter.writeUInt16(byteBuffer, 0);
        }
    }
}