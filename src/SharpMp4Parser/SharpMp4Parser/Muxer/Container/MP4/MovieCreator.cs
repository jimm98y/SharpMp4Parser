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

using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using System.Collections.Generic;
using SharpMp4Parser.IsoParser.Tools;

namespace SharpMp4Parser.Muxer.Container.MP4
{
    /**
     * Shortcut to build a movie from an MP4 file.
     */
    public class MovieCreator
    {

        public static Movie build(string file)
        {
            File f = new File(file);
            FileInputStream fis = new FileInputStream(f);
            Movie m = build(fis.getChannel(), new FileRandomAccessSourceImpl(new RandomAccessFile(f, "r")), file);
            fis.close();
            return m;
        }

        /**
         * Creates <code>Movie</code> object from a <code>ReadableByteChannel</code>.
         *
         * @param name                track name to identify later
         * @param readableByteChannel the box structure is read from this channel
         * @param randomAccessSource  the samples or read from this randomAccessSource
         * @return a representation of the movie
         * @throws IOException in case of I/O error during IsoFile creation
         */
        public static Movie build(ReadableByteChannel readableByteChannel, RandomAccessSource randomAccessSource, string name)
        {
            IsoFile isoFile = new IsoFile(readableByteChannel);
            Movie m = new Movie();
            List<TrackBox> trackBoxes = isoFile.getMovieBox().getBoxes<TrackBox>(typeof(TrackBox));
            foreach (TrackBox trackBox in trackBoxes)
            {
                SchemeTypeBox schm = Path.getPath<SchemeTypeBox>(trackBox, "mdia[0]/minf[0]/stbl[0]/stsd[0]/enc.[0]/sinf[0]/schm[0]");
                if (schm != null && (schm.getSchemeType().Equals("cenc") || schm.getSchemeType().Equals("cbc1")))
                {
                    m.addTrack(new CencMp4TrackImplImpl(
        trackBox.getTrackHeaderBox().getTrackId(), isoFile,
                            randomAccessSource, name + "[" + trackBox.getTrackHeaderBox().getTrackId() + "]"));
                }
                else if (schm != null && (schm.getSchemeType().Equals("piff")))
                {
                    m.addTrack(new PiffMp4TrackImpl(
                    trackBox.getTrackHeaderBox().getTrackId(), isoFile,
                            randomAccessSource, name + "[" + trackBox.getTrackHeaderBox().getTrackId() + "]"));
                }
                else
                {
                    m.addTrack(new Mp4TrackImpl(
                    trackBox.getTrackHeaderBox().getTrackId(), isoFile,
                            randomAccessSource, name + "[" + trackBox.getTrackHeaderBox().getTrackId() + "]"));
                }
            }
            m.setMatrix(isoFile.getMovieBox().getMovieHeaderBox().getMatrix());
            return m;
        }
    }
}
