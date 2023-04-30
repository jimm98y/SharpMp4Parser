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

using SharpMp4Parser.Java;
using SharpMp4Parser.Support;
using SharpMp4Parser.Tools;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * This box within a Media Box declares the process by which the media-data in the track is presented,
     * and thus, the nature of the media in a track.
     * This Box when present in a Meta Box, declares the structure or format of the 'meta' box contents.
     * See ISO/IEC 14496-12 for details.
     *
     * @see MetaBox
     * @see MediaBox
     */
    public class HandlerBox : AbstractFullBox
    {
        public const string TYPE = "hdlr";
        public static readonly ReadOnlyDictionary<string, string> readableTypes;

        static HandlerBox()
        {
            Dictionary<string, string> hm = new Dictionary<string, string>
            {
                { "odsm", "ObjectDescriptorStream - defined in ISO/IEC JTC1/SC29/WG11 - CODING OF MOVING PICTURES AND AUDIO" },
                { "crsm", "ClockReferenceStream - defined in ISO/IEC JTC1/SC29/WG11 - CODING OF MOVING PICTURES AND AUDIO" },
                { "sdsm", "SceneDescriptionStream - defined in ISO/IEC JTC1/SC29/WG11 - CODING OF MOVING PICTURES AND AUDIO" },
                { "m7sm", "MPEG7Stream - defined in ISO/IEC JTC1/SC29/WG11 - CODING OF MOVING PICTURES AND AUDIO" },
                { "ocsm", "ObjectContentInfoStream - defined in ISO/IEC JTC1/SC29/WG11 - CODING OF MOVING PICTURES AND AUDIO" },
                { "ipsm", "IPMP Stream - defined in ISO/IEC JTC1/SC29/WG11 - CODING OF MOVING PICTURES AND AUDIO" },
                { "mjsm", "MPEG-J Stream - defined in ISO/IEC JTC1/SC29/WG11 - CODING OF MOVING PICTURES AND AUDIO" },
                { "mdir", "Apple Meta Data iTunes Reader" },
                { "mp7b", "MPEG-7 binary XML" },
                { "mp7t", "MPEG-7 XML" },
                { "vide", "Video Track" },
                { "soun", "Sound Track" },
                { "hint", "Hint Track" },
                { "appl", "Apple specific" },
                { "meta", "Timed Metadata track - defined in ISO/IEC JTC1/SC29/WG11 - CODING OF MOVING PICTURES AND AUDIO" }
            };
            readableTypes = new ReadOnlyDictionary<string, string>(hm);
        }

        private string handlerType;
        private string name = null;
        private long a, b, c;
        private bool zeroTerm = true;

        private long shouldBeZeroButAppleWritesHereSomeValue;

        public HandlerBox() : base(TYPE)
        { }

        public string getHandlerType()
        {
            return handlerType;
        }

        public void setHandlerType(string handlerType)
        {
            this.handlerType = handlerType;
        }

        public string getName()
        {
            return name;
        }

        /**
         * You are required to add a '\0' string termination by yourself.
         *
         * @param name the new human readable name
         */
        public void setName(string name)
        {
            this.name = name;
        }

        public string getHumanReadableTrackType()
        {
            return readableTypes[handlerType] != null ? readableTypes[handlerType] : "Unknown Handler Type";
        }

        protected override long getContentSize()
        {
            if (zeroTerm)
            {
                return 25 + Utf8.utf8StringLengthInBytes(name);
            }
            else
            {
                return 24 + Utf8.utf8StringLengthInBytes(name);
            }

        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            shouldBeZeroButAppleWritesHereSomeValue = IsoTypeReader.readUInt32(content);
            handlerType = IsoTypeReader.read4cc(content);
            a = IsoTypeReader.readUInt32(content);
            b = IsoTypeReader.readUInt32(content);
            c = IsoTypeReader.readUInt32(content);
            if (content.remaining() > 0)
            {
                name = IsoTypeReader.readString(content, content.remaining());
                if (name.EndsWith("\0"))
                {
                    name = name.Substring(0, name.Length - 1);
                    zeroTerm = true;
                }
                else
                {
                    zeroTerm = false;
                }
            }
            else
            {
                zeroTerm = false; //No string at all, not even zero term char
            }
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt32(byteBuffer, shouldBeZeroButAppleWritesHereSomeValue);
            byteBuffer.put(IsoFile.fourCCtoBytes(handlerType));
            IsoTypeWriter.writeUInt32(byteBuffer, a);
            IsoTypeWriter.writeUInt32(byteBuffer, b);
            IsoTypeWriter.writeUInt32(byteBuffer, c);
            if (name != null)
            {
                byteBuffer.put(Utf8.convert(name));
            }
            if (zeroTerm)
            {
                byteBuffer.put((byte)0);
            }
        }

        public override string ToString()
        {
            return "HandlerBox[handlerType=" + getHandlerType() + ";name=" + getName() + "]";
        }
    }
}
