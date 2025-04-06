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

using System.Collections.Generic;
using System;
using SharpMp4Parser.Java;
using System.Text;
using SharpMp4Parser.IsoParser.Tools;

namespace SharpMp4Parser.IsoParser.Support
{
    /**
     * Abstract base class for a full tools box only containing ither boxes.
     */
    public abstract class FullContainerBox : AbstractContainerBox, FullBox
    {
        //private static Logger LOG = LoggerFactory.getLogger(FullContainerBox.class);
        private int version;
        private int flags;

        public FullContainerBox(string type) : base(type)
        { }

        public int getVersion()
        {
            return version;
        }

        public void setVersion(int version)
        {
            this.version = version;
        }

        public int getFlags()
        {
            return flags;
        }

        public void setFlags(int flags)
        {
            this.flags = flags;
        }

        public override List<T> getBoxes<T>(Type clazz)
        {
            return getBoxes<T>(clazz, false);
        }

        public override void parse(ByteStream dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            ByteBuffer versionAndFlags = ByteBuffer.allocate(4);
            dataSource.read(versionAndFlags);
            parseVersionAndFlags((ByteBuffer)versionAndFlags.rewind());
            base.parse(dataSource, header, contentSize, boxParser);
        }

        public override void getBox(ByteStream writableByteChannel)
        {
            base.getBox(writableByteChannel);
        }

        public override string ToString()
        {
            return GetType().Name + "[childBoxes]";
        }

        /**
         * Parses the version/flags header and returns the remaining box size.
         *
         * @param content the <code>ByteBuffer</code> that contains the version &amp; flag
         * @return number of bytes read
         */
        protected long parseVersionAndFlags(ByteBuffer content)
        {
            version = IsoTypeReader.readUInt8(content);
            flags = IsoTypeReader.readUInt24(content);
            return 4;
        }

        protected void writeVersionAndFlags(ByteBuffer bb)
        {
            IsoTypeWriter.writeUInt8(bb, version);
            IsoTypeWriter.writeUInt24(bb, flags);
        }

        protected override ByteBuffer getHeader()
        {
            ByteBuffer header;
            if (largeBox || getSize() >= 1L << 32)
            {
                header = ByteBuffer.wrap(new byte[] { 0, 0, 0, 1, Encoding.UTF8.GetBytes(type)[0], Encoding.UTF8.GetBytes(type)[1], Encoding.UTF8.GetBytes(type)[2], Encoding.UTF8.GetBytes(type)[3], 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                ((Java.Buffer)header).position(8);
                IsoTypeWriter.writeUInt64(header, getSize());
                writeVersionAndFlags(header);
            }
            else
            {
                header = ByteBuffer.wrap(new byte[] { 0, 0, 0, 0, Encoding.UTF8.GetBytes(type)[0], Encoding.UTF8.GetBytes(type)[1], Encoding.UTF8.GetBytes(type)[2], Encoding.UTF8.GetBytes(type)[3], 0, 0, 0, 0 });
                IsoTypeWriter.writeUInt32(header, getSize());
                ((Java.Buffer)header).position(8);
                writeVersionAndFlags(header);
            }
            header.rewind();
            return header;
        }
    }
}
