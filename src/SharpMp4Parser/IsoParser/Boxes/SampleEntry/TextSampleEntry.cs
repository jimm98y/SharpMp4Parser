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

using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;
using System.Linq;

namespace SharpMp4Parser.IsoParser.Boxes.SampleEntry
{
    /**
     * <h1>4cc = "{@value #TYPE1}"</h1>
     * Entry type for timed text samples defined in the timed text specification (ISO/IEC 14496-17).
     */
    public class TextSampleEntry : AbstractSampleEntry
    {
        public const string TYPE1 = "tx3g";

        public const string TYPE_ENCRYPTED = "enct";

        /*  class TextSampleEntry() extends AbstractSampleEntry ('tx3g') {
            unsigned int(32)  displayFlags;
            signed int(8)     horizontal-justification;
            signed int(8)     vertical-justification;
            unsigned int(8)   background-color-rgba[4];
            BoxRecord         default-text-box;
            StyleRecord       default-style;
            FontTableBox      font-table;
          }
          */

        private long displayFlags; // 32 bits
        private int horizontalJustification; // 8 bit
        private int verticalJustification;  // 8 bit
        private int[] backgroundColorRgba = new int[4]; // 4 bytes
        private BoxRecord boxRecord = new BoxRecord();
        private StyleRecord styleRecord = new StyleRecord();
         

        public TextSampleEntry() : base(TYPE1)
        { }

        public TextSampleEntry(string type) : base(type)
        { }

        public void setType(string type)
        {
            this.type = type;
        }

        public override void parse(ByteStream dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            ByteBuffer content = ByteBuffer.allocate(38);
            dataSource.read(content);
            ((Java.Buffer)content).position(6);
            dataReferenceIndex = IsoTypeReader.readUInt16(content);
            displayFlags = IsoTypeReader.readUInt32(content);
            horizontalJustification = IsoTypeReader.readUInt8(content);
            verticalJustification = IsoTypeReader.readUInt8(content);
            backgroundColorRgba = new int[4];
            backgroundColorRgba[0] = IsoTypeReader.readUInt8(content);
            backgroundColorRgba[1] = IsoTypeReader.readUInt8(content);
            backgroundColorRgba[2] = IsoTypeReader.readUInt8(content);
            backgroundColorRgba[3] = IsoTypeReader.readUInt8(content);
            boxRecord = new BoxRecord();
            boxRecord.parse(content);

            styleRecord = new StyleRecord();
            styleRecord.parse(content);
            initContainer(dataSource, contentSize - 38, boxParser);
        }

        public override void getBox(ByteStream writableByteChannel)
        {
            writableByteChannel.write(getHeader());
            ByteBuffer byteBuffer = ByteBuffer.allocate(38);
            ((Java.Buffer)byteBuffer).position(6);
            IsoTypeWriter.writeUInt16(byteBuffer, dataReferenceIndex);
            IsoTypeWriter.writeUInt32(byteBuffer, displayFlags);
            IsoTypeWriter.writeUInt8(byteBuffer, horizontalJustification);
            IsoTypeWriter.writeUInt8(byteBuffer, verticalJustification);
            IsoTypeWriter.writeUInt8(byteBuffer, backgroundColorRgba[0]);
            IsoTypeWriter.writeUInt8(byteBuffer, backgroundColorRgba[1]);
            IsoTypeWriter.writeUInt8(byteBuffer, backgroundColorRgba[2]);
            IsoTypeWriter.writeUInt8(byteBuffer, backgroundColorRgba[3]);
            boxRecord.getContent(byteBuffer);
            styleRecord.getContent(byteBuffer);
            writableByteChannel.write((ByteBuffer)byteBuffer.rewind());
            writeContainer(writableByteChannel);
        }


        public override string ToString()
        {
            return "TextSampleEntry";
        }

        public BoxRecord getBoxRecord()
        {
            return boxRecord;
        }

        public void setBoxRecord(BoxRecord boxRecord)
        {
            this.boxRecord = boxRecord;
        }

        public StyleRecord getStyleRecord()
        {
            return styleRecord;
        }

        public void setStyleRecord(StyleRecord styleRecord)
        {
            this.styleRecord = styleRecord;
        }

        public bool isScrollIn()
        {
            return (displayFlags & 0x00000020) == 0x00000020;
        }

        public void setScrollIn(bool scrollIn)
        {
            if (scrollIn)
            {
                displayFlags |= 0x00000020;
            }
            else
            {
                displayFlags &= ~0x00000020;
            }
        }

        public bool isScrollOut()
        {
            return (displayFlags & 0x00000040) == 0x00000040;
        }

        public void setScrollOut(bool scrollOutIn)
        {
            if (scrollOutIn)
            {
                displayFlags |= 0x00000040;
            }
            else
            {
                displayFlags &= ~0x00000040;
            }
        }

        public bool isScrollDirection()
        {
            return (displayFlags & 0x00000180) == 0x00000180;
        }

        public void setScrollDirection(bool scrollOutIn)
        {
            if (scrollOutIn)
            {
                displayFlags |= 0x00000180;
            }
            else
            {
                displayFlags &= ~0x00000180;
            }
        }

        public bool isContinuousKaraoke()
        {
            return (displayFlags & 0x00000800) == 0x00000800;
        }

        public void setContinuousKaraoke(bool continuousKaraoke)
        {
            if (continuousKaraoke)
            {
                displayFlags |= 0x00000800;
            }
            else
            {
                displayFlags &= ~0x00000800;
            }
        }

        public bool isWriteTextVertically()
        {
            return (displayFlags & 0x00020000) == 0x00020000;
        }

        public void setWriteTextVertically(bool writeTextVertically)
        {
            if (writeTextVertically)
            {
                displayFlags |= 0x00020000;
            }
            else
            {
                displayFlags &= ~0x00020000;
            }
        }


        public bool isFillTextRegion()
        {
            return (displayFlags & 0x00040000) == 0x00040000;
        }

        public void setFillTextRegion(bool fillTextRegion)
        {
            if (fillTextRegion)
            {
                displayFlags |= 0x00040000;
            }
            else
            {
                displayFlags &= ~0x00040000;
            }
        }


        public int getHorizontalJustification()
        {
            return horizontalJustification;
        }

        public void setHorizontalJustification(int horizontalJustification)
        {
            this.horizontalJustification = horizontalJustification;
        }

        public int getVerticalJustification()
        {
            return verticalJustification;
        }

        public void setVerticalJustification(int verticalJustification)
        {
            this.verticalJustification = verticalJustification;
        }

        public int[] getBackgroundColorRgba()
        {
            return backgroundColorRgba;
        }

        public void setBackgroundColorRgba(int[] backgroundColorRgba)
        {
            this.backgroundColorRgba = backgroundColorRgba;
        }

        public override long getSize()
        {
            long s = getContainerSize();
            long t = 38; // bytes to container start
            return s + t + (largeBox || s + t >= 1L << 32 ? 16 : 8);

        }

        /*
        class FontRecord {
        unsigned int(16) 	font-ID;
        unsigned int(8)	font-name-length;
        unsigned int(8)	font[font-name-length];
        }
         */

        public sealed class BoxRecord
        {
            int top;
            int left;
            int bottom;
            int right;

            public BoxRecord()
            {
            }

            public BoxRecord(int top, int left, int bottom, int right)
            {
                this.top = top;
                this.left = left;
                this.bottom = bottom;
                this.right = right;
            }

            public void parse(ByteBuffer input)
            {
                top = IsoTypeReader.readUInt16(input);
                left = IsoTypeReader.readUInt16(input);
                bottom = IsoTypeReader.readUInt16(input);
                right = IsoTypeReader.readUInt16(input);
            }

            public void getContent(ByteBuffer bb)
            {
                IsoTypeWriter.writeUInt16(bb, top);
                IsoTypeWriter.writeUInt16(bb, left);
                IsoTypeWriter.writeUInt16(bb, bottom);
                IsoTypeWriter.writeUInt16(bb, right);
            }

            public int getSize()
            {
                return 8;
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                BoxRecord boxRecord = (BoxRecord)o;

                if (bottom != boxRecord.bottom) return false;
                if (left != boxRecord.left) return false;
                if (right != boxRecord.right) return false;
                if (top != boxRecord.top) return false;

                return true;
            }

            public override int GetHashCode()
            {
                int result = top;
                result = 31 * result + left;
                result = 31 * result + bottom;
                result = 31 * result + right;
                return result;
            }
        }

        /*
        aligned(8) class StyleRecord {
        unsigned int(16) 	startChar;
        unsigned int(16)	endChar;
        unsigned int(16)	font-ID;
        unsigned int(8)	face-style-flags;
        unsigned int(8)	font-size;
        unsigned int(8)	text-color-rgba[4];
        }
        */
        public sealed class StyleRecord
        {
            int startChar;
            int endChar;
            int fontId;
            int faceStyleFlags;
            int fontSize;
            byte[] textColor = new byte[] { 0xff, 0xff, 0xff, 0xff };

            public StyleRecord()
            {
            }

            public StyleRecord(int startChar, int endChar, int fontId, int faceStyleFlags, int fontSize, byte[] textColor)
            {
                this.startChar = startChar;
                this.endChar = endChar;
                this.fontId = fontId;
                this.faceStyleFlags = faceStyleFlags;
                this.fontSize = fontSize;
                this.textColor = textColor;
            }

            public void parse(ByteBuffer input)
            {
                startChar = IsoTypeReader.readUInt16(input);
                endChar = IsoTypeReader.readUInt16(input);
                fontId = IsoTypeReader.readUInt16(input);
                faceStyleFlags = IsoTypeReader.readUInt8(input);
                fontSize = IsoTypeReader.readUInt8(input);
                textColor = new byte[4];
                textColor[0] = (byte)IsoTypeReader.readUInt8(input);
                textColor[1] = (byte)IsoTypeReader.readUInt8(input);
                textColor[2] = (byte)IsoTypeReader.readUInt8(input);
                textColor[3] = (byte)IsoTypeReader.readUInt8(input);
            }


            public void getContent(ByteBuffer bb)
            {
                IsoTypeWriter.writeUInt16(bb, startChar);
                IsoTypeWriter.writeUInt16(bb, endChar);
                IsoTypeWriter.writeUInt16(bb, fontId);
                IsoTypeWriter.writeUInt8(bb, faceStyleFlags);
                IsoTypeWriter.writeUInt8(bb, fontSize);
                IsoTypeWriter.writeUInt8(bb, textColor[0]);
                IsoTypeWriter.writeUInt8(bb, textColor[1]);
                IsoTypeWriter.writeUInt8(bb, textColor[2]);
                IsoTypeWriter.writeUInt8(bb, textColor[3]);
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                StyleRecord that = (StyleRecord)o;

                if (endChar != that.endChar) return false;
                if (faceStyleFlags != that.faceStyleFlags) return false;
                if (fontId != that.fontId) return false;
                if (fontSize != that.fontSize) return false;
                if (startChar != that.startChar) return false;
                if (!textColor.SequenceEqual(that.textColor)) return false;

                return true;
            }

            public override int GetHashCode()
            {
                int result = startChar;
                result = 31 * result + endChar;
                result = 31 * result + fontId;
                result = 31 * result + faceStyleFlags;
                result = 31 * result + fontSize;
                result = 31 * result + (textColor != null ? Arrays.hashCode(textColor) : 0);
                return result;
            }

            public int getSize()
            {
                return 12;
            }
        }
    }
}