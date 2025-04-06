﻿/*  
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

using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * The sample description table gives detailed information about the coding type used, and any initialization
     * information needed for that coding. <br>
     * The information stored in the sample description box after the entry-count is both track-type specific as
     * documented here, and can also have variants within a track type (e.g. different codings may use different
     * specific information after some common fields, even within a video track).<br>
     * For video tracks, a VisualSampleEntry is used; for audio tracks, an AudioSampleEntry. Hint tracks use an
     * entry format specific to their protocol, with an appropriate name. Timed Text tracks use a TextSampleEntry
     * For hint tracks, the sample description contains appropriate declarative data for the streaming protocol being
     * used, and the format of the hint track. The definition of the sample description is specific to the protocol.
     * Multiple descriptions may be used within a track.<br>
     * The 'protocol' and 'codingname' fields are registered identifiers that uniquely identify the streaming protocol or
     * compression format decoder to be used. A given protocol or codingname may have optional or required
     * extensions to the sample description (e.g. codec initialization parameters). All such extensions shall be within
     * boxes; these boxes occur after the required fields. Unrecognized boxes shall be ignored.
     * <br>
     * Defined in ISO/IEC 14496-12
     *
     * @see VisualSampleEntry
     * @see TextSampleEntry
     * @see AudioSampleEntry
     */
    public class SampleDescriptionBox : AbstractContainerBox, FullBox
    {
        public const string TYPE = "stsd";
        private int version;
        private int flags;

        public SampleDescriptionBox() : base(TYPE)
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
            dataSource.read(versionFlagNumOfChildBoxes);
            versionFlagNumOfChildBoxes.rewind();
            version = IsoTypeReader.readUInt8(versionFlagNumOfChildBoxes);
            flags = IsoTypeReader.readUInt24(versionFlagNumOfChildBoxes);
            // number of child boxes is not required
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

        public AbstractSampleEntry getSampleEntry()
        {
            foreach (AbstractSampleEntry box in getBoxes<AbstractSampleEntry>(typeof(AbstractSampleEntry)))
            {
                return box;
            }
            return null;
        }

        public override long getSize()
        {
            long s = getContainerSize();
            long t = 8;
            return s + t + (largeBox || s + t + 8 >= 1L << 32 ? 16 : 8);
        }
    }
}
