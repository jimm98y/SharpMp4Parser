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
using System;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * A common base structure to contain general metadata. See ISO/IEC 14496-12 Ch. 8.44.1.
     */
    public class MetaBox : AbstractContainerBox
    {
        public const string TYPE = "meta";

        private int version;
        private int flags;
        private bool quickTimeFormat;

        public MetaBox() : base(TYPE)
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

        public override void parse(ByteStream dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            // Read first 20 bytes to determine whether the file is formatted according to QuickTime File Format.
            RewindableByteStreamBase rewindableDataSource = new RewindableByteStreamBase(dataSource, 20);
            ByteBuffer bb = ByteBuffer.allocate(20);
            int bytesRead = rewindableDataSource.read(bb);
            if (bytesRead == 20)
            {
                // If the second and the fifth 32-bit integers encode 'hdlr' and 'mdta' respectively then the MetaBox is
                // formatted according to QuickTime File Format.
                // See https://developer.apple.com/library/content/documentation/QuickTime/QTFF/Metadata/Metadata.html
                ((Java.Buffer)bb).position(4);
                string second4cc = IsoTypeReader.read4cc(bb);
                ((Java.Buffer)bb).position(16);
                string fifth4cc = IsoTypeReader.read4cc(bb);
                if ("hdlr".Equals(second4cc) && "mdta".Equals(fifth4cc))
                {
                    quickTimeFormat = true;
                }
            }
            rewindableDataSource.rewind();

            if (!quickTimeFormat)
            {
                bb = ByteBuffer.allocate(4);
                rewindableDataSource.read(bb);
                parseVersionAndFlags((ByteBuffer)bb.rewind());
            }

            int bytesUsed = quickTimeFormat ? 0 : 4;
            initContainer(rewindableDataSource, contentSize - bytesUsed, boxParser);
        }

        public override void getBox(ByteStream writableByteChannel)
        {
            writableByteChannel.write(getHeader());
            if (!quickTimeFormat)
            {
                ByteBuffer bb = ByteBuffer.allocate(4);
                writeVersionAndFlags(bb);
                writableByteChannel.write((ByteBuffer)bb.rewind());
            }
            writeContainer(writableByteChannel);
        }

        public override long getSize()
        {
            long s = getContainerSize();
            long t = quickTimeFormat ? 0 : 4; // bytes to container start
            return s + t + (largeBox || s + t >= 1L << 32 ? 16 : 8);

        }
    }
}
