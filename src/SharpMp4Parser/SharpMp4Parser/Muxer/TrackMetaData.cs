/*
 * Copyright 2012 Sebastian Annies, Hamburg
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
using System;

namespace SharpMp4Parser.Muxer
{
    /**
     *
     */
    public class TrackMetaData : ICloneable
    {
        /**
         * specifies the front-to-back ordering of video tracks; tracks with lower
         * numbers are closer to the viewer. 0 is the normal value, and -1 would be
         * in front of track 0, and so on.
         */
        int layer;
        private string language = "eng";
        private long timescale;
        private DateTime modificationTime = new DateTime();
        private DateTime creationTime = new DateTime();
        private Matrix matrix = Matrix.ROTATE_0;
        private double width;
        private double height;
        private float volume;
        private long trackId = 1; // zero is not allowed
        private int group = 0;

        public string getLanguage()
        {
            return language;
        }

        public void setLanguage(string language)
        {
            this.language = language;
        }

        public long getTimescale()
        {
            return timescale;
        }

        public void setTimescale(long timescale)
        {
            this.timescale = timescale;
        }

        public DateTime getModificationTime()
        {
            return modificationTime;
        }

        public void setModificationTime(DateTime modificationTime)
        {
            this.modificationTime = modificationTime;
        }

        public DateTime getCreationTime()
        {
            return creationTime;
        }

        public void setCreationTime(DateTime creationTime)
        {
            this.creationTime = creationTime;
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

        public long getTrackId()
        {
            return trackId;
        }

        public void setTrackId(long trackId)
        {
            this.trackId = trackId;
        }

        public int getLayer()
        {
            return layer;
        }

        public void setLayer(int layer)
        {
            this.layer = layer;
        }

        public float getVolume()
        {
            return volume;
        }

        public void setVolume(float volume)
        {
            this.volume = volume;
        }

        public int getGroup()
        {
            return group;
        }

        public void setGroup(int group)
        {
            this.group = group;
        }

        public Matrix getMatrix()
        {
            return matrix;
        }

        public void setMatrix(Matrix matrix)
        {
            this.matrix = matrix;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}