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
     * The Item Protection Box provides an array of item protection information, for use by the Item Information Box.
     *
     * @see ItemProtectionBox
     */
    public class ItemProtectionBox : AbstractContainerBox, FullBox
    {
        public const string TYPE = "ipro";
        private int version;
        private int flags;

        public ItemProtectionBox() : base(TYPE)
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

        public SchemeInformationBox getItemProtectionScheme()
        {
            if (getBoxes<SchemeInformationBox>(typeof(SchemeInformationBox)).Count != 0)
            {
                return getBoxes<SchemeInformationBox>(typeof(SchemeInformationBox))[0];
            }
            else
            {
                return null;
            }
        }

        public override void parse(ByteStream dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {

            ByteBuffer versionFlagNumOfChildBoxes = ByteBuffer.allocate(6);
            dataSource.read(versionFlagNumOfChildBoxes);
            versionFlagNumOfChildBoxes.rewind();
            version = IsoTypeReader.readUInt8(versionFlagNumOfChildBoxes);
            flags = IsoTypeReader.readUInt24(versionFlagNumOfChildBoxes);
            // number of child boxes is not required
            initContainer(dataSource, contentSize - 6, boxParser);
        }

        public override void getBox(ByteStream writableByteChannel)
        {
            writableByteChannel.write(getHeader());
            ByteBuffer versionFlagNumOfChildBoxes = ByteBuffer.allocate(6);
            IsoTypeWriter.writeUInt8(versionFlagNumOfChildBoxes, version);
            IsoTypeWriter.writeUInt24(versionFlagNumOfChildBoxes, flags);
            IsoTypeWriter.writeUInt16(versionFlagNumOfChildBoxes, getBoxes().Count);
            writableByteChannel.write((ByteBuffer)versionFlagNumOfChildBoxes.rewind());
            writeContainer(writableByteChannel);
        }

        public override long getSize()
        {
            long s = getContainerSize();
            long t = 6; // bytes to container start
            return s + t + (largeBox || s + t >= 1L << 32 ? 16 : 8);
        }
    }
}