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
using SharpMp4Parser.IsoParser.Tools;
using System.Collections.Generic;

namespace SharpMp4Parser.Muxer
{
    /**
     *
     */
    public class Movie
    {
        Matrix matrix = Matrix.ROTATE_0;
        List<Track> tracks = new List<Track>();

        public Movie()
        {
        }

        public Movie(List<Track> tracks)
        {
            this.tracks = tracks;
        }

        public List<Track> getTracks()
        {
            return tracks;
        }

        public void setTracks(List<Track> tracks)
        {
            this.tracks = tracks;
        }

        public void addTrack(Track nuTrack)
        {
            // do some checking
            // perhaps the movie needs to get longer!
            if (getTrackByTrackId(nuTrack.getTrackMetaData().getTrackId()) != null)
            {
                // We already have a track with that trackId. Create a new one
                nuTrack.getTrackMetaData().setTrackId(getNextTrackId());
            }
            tracks.Add(nuTrack);
        }


        public override string ToString()
        {
            string s = "Movie{ ";
            foreach (Track track in tracks)
            {
                s += "track_" + track.getTrackMetaData().getTrackId() + " (" + track.getHandler() + ") ";
            }

            s += '}';
            return s;
        }

        public long getNextTrackId()
        {
            long nextTrackId = 0;
            foreach (Track track in tracks)
            {
                nextTrackId = nextTrackId < track.getTrackMetaData().getTrackId() ? track.getTrackMetaData().getTrackId() : nextTrackId;
            }
            return ++nextTrackId;
        }


        public Track getTrackByTrackId(long trackId)
        {
            foreach (Track track in tracks)
            {
                if (track.getTrackMetaData().getTrackId() == trackId)
                {
                    return track;
                }
            }
            return null;
        }


        public long getTimescale()
        {
            var enumerator = this.getTracks().GetEnumerator();
            enumerator.MoveNext();
            long timescale = enumerator.Current.getTrackMetaData().getTimescale();
            foreach (Track track in this.getTracks())
            {
                timescale = Mp4Math.lcm(track.getTrackMetaData().getTimescale(), timescale);
            }
            return timescale;
        }

        public Matrix getMatrix()
        {
            return matrix;
        }

        public void setMatrix(Matrix matrix)
        {
            this.matrix = matrix;
        }
    }
}
