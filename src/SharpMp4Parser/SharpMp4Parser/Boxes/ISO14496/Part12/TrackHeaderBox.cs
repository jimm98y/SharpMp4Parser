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
using SharpMp4Parser.Tools;
using SharpMp4Parser.Support;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * This box specifies the characteristics of a single track. Exactly one Track Header Box is contained in a track.<br>
     * In the absence of an edit list, the presentation of a track starts at the beginning of the overall presentation. An
     * empty edit is used to offset the start time of a track. <br>
     * The default value of the track header flags for media tracks is 7 (track_enabled, track_in_movie,
     * track_in_preview). If in a presentation all tracks have neither track_in_movie nor track_in_preview set, then all
     * tracks shall be treated as if both flags were set on all tracks. Hint tracks should have the track header flags set
     * to 0, so that they are ignored for local playback and preview.
     */
    public class TrackHeaderBox : AbstractFullBox
    {
        public const string TYPE = "tkhd";
        //private static Logger LOG = LoggerFactory.getLogger(TrackHeaderBox.class);
        private DateTime creationTime = new DateTime(0);
        private DateTime modificationTime = new DateTime(0);
        private long trackId;
        private long duration;
        private int layer;
        private int alternateGroup;
        private float volume;
        private Matrix matrix = Matrix.ROTATE_0;
        private double width;
        private double height;


        public TrackHeaderBox() : base(TYPE)
        { }

        public DateTime getCreationTime()
        {
            return creationTime;
        }

        public void setCreationTime(DateTime creationTime)
        {
            this.creationTime = creationTime;
            if (DateHelper.convert(creationTime) >= (1L << 32))
            {
                setVersion(1);
            }
        }

        public DateTime getModificationTime()
        {
            return modificationTime;
        }

        public void setModificationTime(DateTime modificationTime)
        {
            this.modificationTime = modificationTime;
            if (DateHelper.convert(modificationTime) >= (1L << 32))
            {
                setVersion(1);
            }
        }

        public long getTrackId()
        {
            return trackId;
        }

        public void setTrackId(long trackId)
        {
            this.trackId = trackId;
        }

        public long getDuration()
        {
            return duration;
        }

        public void setDuration(long duration)
        {
            this.duration = duration;
            if (duration >= (1L << 32))
            {
                setFlags(1);
            }
        }

        public int getLayer()
        {
            return layer;
        }

        public void setLayer(int layer)
        {
            this.layer = layer;
        }

        public int getAlternateGroup()
        {
            return alternateGroup;
        }

        public void setAlternateGroup(int alternateGroup)
        {
            this.alternateGroup = alternateGroup;
        }

        public float getVolume()
        {
            return volume;
        }

        public void setVolume(float volume)
        {
            this.volume = volume;
        }

        public Matrix getMatrix()
        {
            return matrix;
        }

        public void setMatrix(Matrix matrix)
        {
            this.matrix = matrix;
        }

        public double getWidth()
        {
            return width;
        }

        public void setWidth(double width)
        {
            this.width = width;
        }

        public double getHeight()
        {
            return height;
        }

        public void setHeight(double height)
        {
            this.height = height;
        }

        protected override long getContentSize()
        {
            long contentSize = 4;
            if (getVersion() == 1)
            {
                contentSize += 32;
            }
            else
            {
                contentSize += 20;
            }
            contentSize += 60;
            return contentSize;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            if (getVersion() == 1)
            {
                creationTime = DateHelper.convert(IsoTypeReader.readUInt64(content));
                modificationTime = DateHelper.convert(IsoTypeReader.readUInt64(content));
                trackId = IsoTypeReader.readUInt32(content);
                IsoTypeReader.readUInt32(content);
                duration = content.getLong();
            }
            else
            {
                creationTime = DateHelper.convert(IsoTypeReader.readUInt32(content));
                modificationTime = DateHelper.convert(IsoTypeReader.readUInt32(content));
                trackId = IsoTypeReader.readUInt32(content);
                IsoTypeReader.readUInt32(content);
                duration = content.getInt();
            } // 196

            if (duration < -1)
            {
                //LOG.warn("tkhd duration is not in expected range");
            }

            IsoTypeReader.readUInt32(content);
            IsoTypeReader.readUInt32(content);
            layer = IsoTypeReader.readUInt16(content);    // 204
            alternateGroup = IsoTypeReader.readUInt16(content);
            volume = IsoTypeReader.readFixedPoint88(content);
            IsoTypeReader.readUInt16(content);     // 212
            matrix = Matrix.fromByteBuffer(content);
            width = IsoTypeReader.readFixedPoint1616(content);    // 248
            height = IsoTypeReader.readFixedPoint1616(content);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            if (getVersion() == 1)
            {
                IsoTypeWriter.writeUInt64(byteBuffer, DateHelper.convert(creationTime));
                IsoTypeWriter.writeUInt64(byteBuffer, DateHelper.convert(modificationTime));
                IsoTypeWriter.writeUInt32(byteBuffer, trackId);
                IsoTypeWriter.writeUInt32(byteBuffer, 0);
                byteBuffer.putLong(duration);
            }
            else
            {
                IsoTypeWriter.writeUInt32(byteBuffer, DateHelper.convert(creationTime));
                IsoTypeWriter.writeUInt32(byteBuffer, DateHelper.convert(modificationTime));
                IsoTypeWriter.writeUInt32(byteBuffer, trackId);
                IsoTypeWriter.writeUInt32(byteBuffer, 0);
                byteBuffer.putInt((int)duration);
            } // 196
            IsoTypeWriter.writeUInt32(byteBuffer, 0);
            IsoTypeWriter.writeUInt32(byteBuffer, 0);
            IsoTypeWriter.writeUInt16(byteBuffer, layer);
            IsoTypeWriter.writeUInt16(byteBuffer, alternateGroup);
            IsoTypeWriter.writeFixedPoint88(byteBuffer, volume);
            IsoTypeWriter.writeUInt16(byteBuffer, 0);
            //int i = 0;
            matrix.getContent(byteBuffer);

            IsoTypeWriter.writeFixedPoint1616(byteBuffer, width);
            IsoTypeWriter.writeFixedPoint1616(byteBuffer, height);
        }

        public string toString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("TrackHeaderBox[");
            result.Append("creationTime=").Append(getCreationTime());
            result.Append(";");
            result.Append("modificationTime=").Append(getModificationTime());
            result.Append(";");
            result.Append("trackId=").Append(getTrackId());
            result.Append(";");
            result.Append("duration=").Append(getDuration());
            result.Append(";");
            result.Append("layer=").Append(getLayer());
            result.Append(";");
            result.Append("alternateGroup=").Append(getAlternateGroup());
            result.Append(";");
            result.Append("volume=").Append(getVolume());
            result.Append(";");
            result.Append("matrix=").Append(matrix);
            result.Append(";");
            result.Append("width=").Append(getWidth());
            result.Append(";");
            result.Append("height=").Append(getHeight());
            result.Append("]");
            return result.ToString();
        }

        public bool isEnabled()
        {
            return (getFlags() & 1) > 0;
        }

        public void setEnabled(bool enabled)
        {
            if (enabled)
            {
                setFlags(getFlags() | 1);
            }
            else
            {
                setFlags(getFlags() & ~1);
            }
        }

        public bool isInMovie()
        {
            return (getFlags() & 2) > 0;
        }

        public void setInMovie(bool inMovie)
        {
            if (inMovie)
            {
                setFlags(getFlags() | 2);
            }
            else
            {
                setFlags(getFlags() & ~2);
            }
        }

        public bool isInPreview()
        {
            return (getFlags() & 4) > 0;
        }

        public void setInPreview(bool inPreview)
        {
            if (inPreview)
            {
                setFlags(getFlags() | 4);
            }
            else
            {
                setFlags(getFlags() & ~4);
            }
        }

        public bool isInPoster()
        {
            return (getFlags() & 8) > 0;
        }

        public void setInPoster(bool inPoster)
        {
            if (inPoster)
            {
                setFlags(getFlags() | 8);
            }
            else
            {
                setFlags(getFlags() & ~8);
            }
        }
    }
}
