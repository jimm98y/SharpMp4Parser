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

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12
{

    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * The data reference object contains a table of data references (normally URLs) that declare the location(s) of
     * the media data used within the presentation. The data reference index in the sample description ties entries in
     * this table to the samples in the track. A track may be split over several sources in this way.
     * If the flag is set indicating that the data is in the same file as this box, then no string (not even an empty one)
     * shall be supplied in the entry field.
     * The DataEntryBox within the DataReferenceBox shall be either a DataEntryUrnBox or a DataEntryUrlBox.
     *
     * @see DataEntryUrlBox
     * @see DataEntryUrnBox
     */
    public class DataReferenceBox : AbstractContainerBox, FullBox
    {

        public const string TYPE = "dref";
        private int version;
        private int flags;

        public DataReferenceBox() : base(TYPE)
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

        public override void parse(ByteStream dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            ByteBuffer versionFlagNumOfChildBoxes = ByteBuffer.allocate(8);
            int required = versionFlagNumOfChildBoxes.limit();
            while (required > 0)
            {
                int read = dataSource.read(versionFlagNumOfChildBoxes);
                required -= read;
            }
            versionFlagNumOfChildBoxes.rewind();
            version = IsoTypeReader.readUInt8(versionFlagNumOfChildBoxes);
            flags = IsoTypeReader.readUInt24(versionFlagNumOfChildBoxes);
            // number of child boxes is not required - ignore
            initContainer(dataSource, contentSize - 8, boxParser);
        }

        public override void getBox(ByteStream writableByteChannel)
        {
            writableByteChannel.write(getHeader());
            ByteBuffer versionFlagNumOfChildBoxes = ByteBuffer.allocate(8);
            IsoTypeWriter.writeUInt8(versionFlagNumOfChildBoxes, version);
            IsoTypeWriter.writeUInt24(versionFlagNumOfChildBoxes, flags);
            IsoTypeWriter.writeUInt32(versionFlagNumOfChildBoxes, getBoxes().Count);
            writableByteChannel.write((ByteBuffer)versionFlagNumOfChildBoxes.rewind());
            writeContainer(writableByteChannel);
        }

        public override long getSize()
        {
            long s = getContainerSize();
            long t = 8;
            return s + t + (largeBox || s + t + 8 >= 1L << 32 ? 16 : 8);
        }
    }
}
