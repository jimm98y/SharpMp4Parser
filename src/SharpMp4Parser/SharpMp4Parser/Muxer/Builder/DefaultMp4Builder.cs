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

using SharpMp4Parser.IsoParser;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Boxes.SampleGrouping;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using SharpMp4Parser.Muxer.Tracks.Encryption;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SharpMp4Parser.Muxer.Builder
{
    /**
     * Creates a plain MP4 file from a video. Plain as plain can be.
     */
    public class DefaultMp4Builder : Mp4Builder
    {

        //private static Logger LOG = LoggerFactory.getLogger(DefaultMp4Builder.class);
        Dictionary<Track, StaticChunkOffsetBox> chunkOffsetBoxes = new Dictionary<Track, StaticChunkOffsetBox>();
        HashSet<SampleAuxiliaryInformationOffsetsBox> sampleAuxiliaryInformationOffsetsBoxes = new HashSet<SampleAuxiliaryInformationOffsetsBox>();
        Dictionary<Track, IList<Sample>> track2Sample = new Dictionary<Track, IList<Sample>>();
        Dictionary<Track, long[]> track2SampleSizes = new Dictionary<Track, long[]>();

        private Fragmenter fragmenter;

        private static long sum(int[] ls)
        {
            long rc = 0;
            foreach (long l in ls)
            {
                rc += l;
            }
            return rc;
        }

        private static long sum(long[] ls)
        {
            long rc = 0;
            foreach (long l in ls)
            {
                rc += l;
            }
            return rc;
        }


        public void setFragmenter(Fragmenter fragmenter)
        {
            this.fragmenter = fragmenter;
        }


        /**
         * {@inheritDoc}
         */
        public IsoParser.Container build(Movie movie)
        {
            if (fragmenter == null)
            {
                fragmenter = new DefaultFragmenterImpl(2);
            }
            //LOG.debug("Creating movie {}", movie);
            foreach (Track track in movie.getTracks())
            {
                // getting the samples may be a time consuming activity
                IList<Sample> samples = track.getSamples();
                putSamples(track, samples);
                long[] sizes = new long[samples.Count];
                for (int i = 0; i < sizes.Length; i++)
                {
                    Sample b = samples[i];
                    sizes[i] = b.getSize();
                }
                track2SampleSizes.Add(track, sizes);

            }

            BasicContainer isoFile = new BasicContainer();

            isoFile.addBox(createFileTypeBox(movie));

            Dictionary<Track, int[]> chunks = new Dictionary<Track, int[]>();
            foreach (Track track in movie.getTracks())
            {
                chunks.Add(track, getChunkSizes(track));
            }
            ParsableBox moov = createMovieBox(movie, chunks);
            isoFile.addBox(moov);
            List<SampleSizeBox> stszs = Path.getPaths< SampleSizeBox>(moov, "trak/mdia/minf/stbl/stsz");

            long contentSize = 0;
            foreach (SampleSizeBox stsz in stszs)
            {
                contentSize += sum(stsz.getSampleSizes());

            }
            //LOG.debug("About to create mdat");
            InterleaveChunkMdat mdat = new InterleaveChunkMdat(movie, chunks, contentSize);

            long dataOffset = 16;
            foreach (Box lightBox in isoFile.getBoxes())
            {
                dataOffset += lightBox.getSize();
            }
            isoFile.addBox(mdat);
            //LOG.debug("mdat crated");

            /*
            dataOffset is where the first sample starts. In this special mdat the samples always start
            at offset 16 so that we can use the same offset for large boxes and small boxes
             */

            foreach (StaticChunkOffsetBox chunkOffsetBox in chunkOffsetBoxes.Values)
            {
                long[] offsets = chunkOffsetBox.getChunkOffsets();
                for (int i = 0; i < offsets.Length; i++)
                {
                    offsets[i] += dataOffset;
                }
            }
            foreach (SampleAuxiliaryInformationOffsetsBox saio in sampleAuxiliaryInformationOffsetsBoxes)
            {
                long offset = saio.getSize(); // the calculation is systematically wrong by 4, I don't want to debug why. Just a quick correction --san 14.May.13
                offset += 4 + 4 + 4 + 4 + 4 + 24;
                // size of all header we were missing otherwise (moov, trak, mdia, minf, stbl)
                offset = Offsets.find(isoFile, saio, offset);

                long[] saioOffsets = saio.getOffsets();
                for (int i = 0; i < saioOffsets.Length; i++)
                {
                    saioOffsets[i] = saioOffsets[i] + offset;

                }
                saio.setOffsets(saioOffsets);
            }

            return isoFile;
        }

        protected IList<Sample> putSamples(Track track, IList<Sample> samples)
        {
            track2Sample.Add(track, samples);
            return samples;
        }

        protected FileTypeBox createFileTypeBox(Movie movie)
        {
            List<string> minorBrands = new List<string>();

            minorBrands.Add("mp42");
            minorBrands.Add("iso6");
            minorBrands.Add("avc1");
            minorBrands.Add("isom");
            return new FileTypeBox("iso6", 1, minorBrands);
        }

        protected MovieBox createMovieBox(Movie movie, Dictionary<Track, int[]> chunks)
        {
            MovieBox movieBox = new MovieBox();
            MovieHeaderBox mvhd = new MovieHeaderBox();

            mvhd.setCreationTime(new DateTime());
            mvhd.setModificationTime(new DateTime());
            mvhd.setMatrix(movie.getMatrix());
            long movieTimeScale = getTimescale(movie);
            long duration = 0;

            foreach (Track track in movie.getTracks())
            {
                long tracksDuration;

                if (track.getEdits() == null || track.getEdits().Count == 0)
                {
                    tracksDuration = (track.getDuration() * movieTimeScale / track.getTrackMetaData().getTimescale());
                }
                else
                {
                    double d = 0;
                    foreach (Edit edit in track.getEdits())
                    {
                        d += (long)edit.getSegmentDuration();
                    }
                    tracksDuration = (long)(d * movieTimeScale);
                }


                if (tracksDuration > duration)
                {
                    duration = tracksDuration;
                }
            }

            mvhd.setDuration(duration);
            mvhd.setTimescale(movieTimeScale);
            // find the next available trackId
            long nextTrackId = 0;
            foreach (Track track in movie.getTracks())
            {
                nextTrackId = nextTrackId < track.getTrackMetaData().getTrackId() ? track.getTrackMetaData().getTrackId() : nextTrackId;
            }
            mvhd.setNextTrackId(++nextTrackId);

            movieBox.addBox(mvhd);
            foreach (Track track in movie.getTracks())
            {
                movieBox.addBox(createTrackBox(track, movie, chunks));
            }
            // metadata here
            ParsableBox udta = createUdta(movie);
            if (udta != null)
            {
                movieBox.addBox(udta);
            }
            return movieBox;

        }

        /**
         * Override to create a user data box that may contain metadata.
         *
         * @param movie source movie
         * @return a 'udta' box or <code>null</code> if none provided
         */
        protected ParsableBox createUdta(Movie movie)
        {
            return null;
        }

        protected TrackBox createTrackBox(Track track, Movie movie, Dictionary<Track, int[]> chunks)
        {

            TrackBox trackBox = new TrackBox();
            TrackHeaderBox tkhd = new TrackHeaderBox();

            tkhd.setEnabled(true);
            tkhd.setInMovie(true);
            //        tkhd.setInPreview(true);
            //        tkhd.setInPoster(true);
            tkhd.setMatrix(track.getTrackMetaData().getMatrix());

            tkhd.setAlternateGroup(track.getTrackMetaData().getGroup());
            tkhd.setCreationTime(track.getTrackMetaData().getCreationTime());

            if (track.getEdits() == null || track.getEdits().Count == 0)
            {
                tkhd.setDuration(track.getDuration() * getTimescale(movie) / track.getTrackMetaData().getTimescale());
            }
            else
            {
                long d = 0;
                foreach (Edit edit in track.getEdits())
                {
                    d += (long)edit.getSegmentDuration();
                }
                tkhd.setDuration(d * track.getTrackMetaData().getTimescale());
            }


            tkhd.setHeight(track.getTrackMetaData().getHeight());
            tkhd.setWidth(track.getTrackMetaData().getWidth());
            tkhd.setLayer(track.getTrackMetaData().getLayer());
            tkhd.setModificationTime(new DateTime());
            tkhd.setTrackId(track.getTrackMetaData().getTrackId());
            tkhd.setVolume(track.getTrackMetaData().getVolume());

            trackBox.addBox(tkhd);

            trackBox.addBox(createEdts(track, movie));

            MediaBox mdia = new MediaBox();
            trackBox.addBox(mdia);
            MediaHeaderBox mdhd = new MediaHeaderBox();
            mdhd.setCreationTime(track.getTrackMetaData().getCreationTime());
            mdhd.setDuration(track.getDuration());
            mdhd.setTimescale(track.getTrackMetaData().getTimescale());
            mdhd.setLanguage(track.getTrackMetaData().getLanguage());
            mdia.addBox(mdhd);
            HandlerBox hdlr = new HandlerBox();
            mdia.addBox(hdlr);

            hdlr.setHandlerType(track.getHandler());

            MediaInformationBox minf = new MediaInformationBox();
            if (track.getHandler().Equals("vide"))
            {
                minf.addBox(new VideoMediaHeaderBox());
            }
            else if (track.getHandler().Equals("soun"))
            {
                minf.addBox(new SoundMediaHeaderBox());
            }
            else if (track.getHandler().Equals("text"))
            {
                minf.addBox(new NullMediaHeaderBox());
            }
            else if (track.getHandler().Equals("subt"))
            {
                minf.addBox(new SubtitleMediaHeaderBox());
            }
            else if (track.getHandler().Equals("hint"))
            {
                minf.addBox(new HintMediaHeaderBox());
            }
            else if (track.getHandler().Equals("sbtl"))
            {
                minf.addBox(new NullMediaHeaderBox());
            }

            // dinf: all these three boxes tell us is that the actual
            // data is in the current file and not somewhere external
            DataInformationBox dinf = new DataInformationBox();
            DataReferenceBox dref = new DataReferenceBox();
            dinf.addBox(dref);
            DataEntryUrlBox url = new DataEntryUrlBox();
            url.setFlags(1);
            dref.addBox(url);
            minf.addBox(dinf);
            //

            ParsableBox stbl = createStbl(track, movie, chunks);
            minf.addBox(stbl);
            mdia.addBox(minf);
            //LOG.debug("done with trak for track_{}", track.getTrackMetaData().getTrackId());
            return trackBox;
        }

        protected ParsableBox createEdts(Track track, Movie movie)
        {
            if (track.getEdits() != null && track.getEdits().Count > 0)
            {
                EditListBox elst = new EditListBox();
                elst.setVersion(0); // quicktime won't play file when version = 1
                List<EditListBox.Entry> entries = new List<EditListBox.Entry>();

                foreach (Edit edit in track.getEdits())
                {
                    entries.Add(new EditListBox.Entry(elst,
                            (long)Math.Round(edit.getSegmentDuration() * movie.getTimescale()),
                            edit.getMediaTime() * track.getTrackMetaData().getTimescale() / edit.getTimeScale(),
                            edit.getMediaRate()));
                }

                elst.setEntries(entries);
                EditBox edts = new EditBox();
                edts.addBox(elst);
                return edts;
            }
            else
            {
                return null;
            }
        }

        protected ParsableBox createStbl(Track track, Movie movie, Dictionary<Track, int[]> chunks)
        {
            SampleTableBox stbl = new SampleTableBox();

            createStsd(track, stbl);
            createStts(track, stbl);
            createCtts(track, stbl);
            createStss(track, stbl);
            createSdtp(track, stbl);
            createStsc(track, chunks, stbl);
            createStsz(track, stbl);
            createStco(track, movie, chunks, stbl);


            Dictionary<string, List<GroupEntry>> groupEntryFamilies = new Dictionary<string, List<GroupEntry>>();
            foreach (var sg in track.getSampleGroups())
            {
                string type = sg.Key.getType();
                List<GroupEntry> groupEntries = groupEntryFamilies[type];
                if (groupEntries == null)
                {
                    groupEntries = new List<GroupEntry>();
                    groupEntryFamilies.Add(type, groupEntries);
                }
                groupEntries.Add(sg.Key);
            }
            foreach (var sg in groupEntryFamilies)
            {
                SampleGroupDescriptionBox sgdb = new SampleGroupDescriptionBox();
                String type = sg.Key;
                sgdb.setGroupingType(type);
                sgdb.setGroupEntries(sg.Value);
                SampleToGroupBox sbgp = new SampleToGroupBox();
                sbgp.setGroupingType(type);
                SampleToGroupBox.Entry last = null;
                for (int i = 0; i < track.getSamples().Count; i++)
                {
                    int index = 0;
                    for (int j = 0; j < sg.Value.Count; j++)
                    {
                        GroupEntry groupEntry = sg.Value[j];
                        long[] sampleNums = track.getSampleGroups()[groupEntry];
                        if (Arrays.binarySearch(sampleNums, i) >= 0)
                        {
                            index = j + 1;
                        }
                    }
                    if (last == null || last.getGroupDescriptionIndex() != index)
                    {
                        last = new SampleToGroupBox.Entry(1, index);
                        sbgp.getEntries().Add(last);
                    }
                    else
                    {
                        last.setSampleCount(last.getSampleCount() + 1);
                    }
                }
                stbl.addBox(sgdb);
                stbl.addBox(sbgp);
            }

            if (track is CencEncryptedTrack)
            {
                createCencBoxes((CencEncryptedTrack)track, stbl, chunks[track]);
            }
            createSubs(track, stbl);
            //LOG.debug("done with stbl for track_{}", track.getTrackMetaData().getTrackId());
            return stbl;
        }

        protected void createSubs(Track track, SampleTableBox stbl)
        {
            if (track.getSubsampleInformationBox() != null)
            {
                stbl.addBox(track.getSubsampleInformationBox());
            }
        }

        protected void createCencBoxes(CencEncryptedTrack track, SampleTableBox stbl, int[] chunkSizes)
        {

            SampleAuxiliaryInformationSizesBox saiz = new SampleAuxiliaryInformationSizesBox();

            saiz.setAuxInfoType("cenc");
            saiz.setFlags(1);
            List<CencSampleAuxiliaryDataFormat> sampleEncryptionEntries = track.getSampleEncryptionEntries();
            if (track.hasSubSampleEncryption())
            {
                short[] sizes = new short[sampleEncryptionEntries.Count];
                for (int i = 0; i < sizes.Length; i++)
                {
                    sizes[i] = (short)sampleEncryptionEntries[i].getSize();
                }
                saiz.setSampleInfoSizes(sizes);
            }
            else
            {
                saiz.setDefaultSampleInfoSize(8); // 8 bytes iv
                saiz.setSampleCount(track.getSamples().Count);
            }

            SampleAuxiliaryInformationOffsetsBox saio = new SampleAuxiliaryInformationOffsetsBox();
            SampleEncryptionBox senc = new SampleEncryptionBox();
            senc.setSubSampleEncryption(track.hasSubSampleEncryption());
            senc.setEntries(sampleEncryptionEntries);

            long offset = senc.getOffsetToFirstIV();
            int index = 0;
            long[] offsets = new long[chunkSizes.Length];


            for (int i = 0; i < chunkSizes.Length; i++)
            {
                offsets[i] = offset;
                for (int j = 0; j < chunkSizes[i]; j++)
                {
                    offset += sampleEncryptionEntries[index++].getSize();
                }
            }
            saio.setOffsets(offsets);

            stbl.addBox(saiz);
            stbl.addBox(saio);
            stbl.addBox(senc);
            sampleAuxiliaryInformationOffsetsBoxes.Add(saio);


        }

        protected void createStsd(Track track, SampleTableBox stbl)
        {
            SampleDescriptionBox stsd = new SampleDescriptionBox();
            stsd.setBoxes(track.getSampleEntries().Select(x => (Box)x).ToList());
            stbl.addBox(stsd);
        }

        public class TrackComparer : IComparer<Track>
        {
            public int Compare(Track o1, Track o2)
            {
                return CastUtils.l2i(o1.getTrackMetaData().getTrackId() - o2.getTrackMetaData().getTrackId());
            }
        }

        protected void createStco(Track targetTrack, Movie movie, Dictionary<Track, int[]> chunks, SampleTableBox stbl)
        {
            if (!chunkOffsetBoxes.ContainsKey(targetTrack) || chunkOffsetBoxes[targetTrack] == null)
            {
                // The ChunkOffsetBox we create here is just a stub
                // since we haven't created the whole structure we can't tell where the
                // first chunk starts (mdat box). So I just let the chunk offset
                // start at zero and I will add the mdat offset later.

                long offset = 0;
                // all tracks have the same number of chunks
                //LOG.debug("Calculating chunk offsets for track_{}", targetTrack.getTrackMetaData().getTrackId());

                List<Track> tracks = new List<Track>(chunks.Keys);
                tracks = tracks.OrderBy(x => x, new TrackComparer()).ToList();

                Dictionary<Track, int> trackToChunk = new Dictionary<Track, int>();
                Dictionary<Track, int> trackToSample = new Dictionary<Track, int>();
                Dictionary<Track, double> trackToTime = new Dictionary<Track, double>();
                foreach (Track track in tracks)
                {
                    trackToChunk.Add(track, 0);
                    trackToSample.Add(track, 0);
                    trackToTime.Add(track, 0.0);
                    chunkOffsetBoxes.Add(track, new StaticChunkOffsetBox());
                }

                while (true)
                {
                    Track nextChunksTrack = null;
                    foreach (Track track in tracks)
                    {
                        // This always chooses the least progressed track
                        if ((nextChunksTrack == null || trackToTime[track] < trackToTime[nextChunksTrack]) &&
                                // either first OR track's next chunk's starttime is smaller than nextTrack's next chunks starttime
                                // AND their need to be chunks left!
                                (trackToChunk[track] < chunks[track].Length))
                        {
                            nextChunksTrack = track;
                        }
                    }
                    if (nextChunksTrack == null)
                    {
                        break; // no next
                    }
                    // found the next one
                    ChunkOffsetBox chunkOffsetBox = chunkOffsetBoxes[nextChunksTrack];
                    chunkOffsetBox.setChunkOffsets(Mp4Arrays.copyOfAndAppend(chunkOffsetBox.getChunkOffsets(), offset));

                    int nextChunksIndex = trackToChunk[nextChunksTrack];

                    int numberOfSampleInNextChunk = chunks[nextChunksTrack][nextChunksIndex];
                    int startSample = trackToSample[nextChunksTrack];
                    double time = trackToTime[nextChunksTrack];

                    long[] durs = nextChunksTrack.getSampleDurations();
                    for (int j = startSample; j < startSample + numberOfSampleInNextChunk; j++)
                    {
                        offset += track2SampleSizes[nextChunksTrack][j];
                        time += (double)durs[j] / nextChunksTrack.getTrackMetaData().getTimescale();
                    }
                    trackToChunk[nextChunksTrack] = nextChunksIndex + 1;
                    trackToSample[nextChunksTrack] = startSample + numberOfSampleInNextChunk;
                    trackToTime[nextChunksTrack] = time;
                }

            }

            stbl.addBox(chunkOffsetBoxes[targetTrack]);
        }

        protected void createStsz(Track track, SampleTableBox stbl)
        {
            SampleSizeBox stsz = new SampleSizeBox();
            stsz.setSampleSizes(track2SampleSizes[track]);

            stbl.addBox(stsz);
        }

        protected void createStsc(Track track, Dictionary<Track, int[]> chunks, SampleTableBox stbl)
        {
            int[] tracksChunkSizes = chunks[track];

            SampleToChunkBox stsc = new SampleToChunkBox();
            stsc.setEntries(new List<SampleToChunkBox.Entry>());
            long lastChunkSize = int.MinValue; // to be sure the first chunks hasn't got the same size
            long lastSampleDescriptionIndex = int.MinValue;
            IList<Sample> samples = track.getSamples();

            int currentSampleIndex = 0;
            List<SampleEntry> sampleEntries = track.getSampleEntries();

            for (int i = 0; i < tracksChunkSizes.Length; i++)
            {
                Sample sample = samples[currentSampleIndex];
                int currentSampleDescriptionIndex = sampleEntries.IndexOf(sample.getSampleEntry()) + 1; // one base

                if (lastChunkSize != tracksChunkSizes[i] || lastSampleDescriptionIndex != currentSampleDescriptionIndex)
                {
                    stsc.getEntries().Add(new SampleToChunkBox.Entry(i + 1, tracksChunkSizes[i], currentSampleDescriptionIndex));
                    lastChunkSize = tracksChunkSizes[i];
                    lastSampleDescriptionIndex = currentSampleDescriptionIndex;
                }
                currentSampleIndex += tracksChunkSizes[i];
            }
            stbl.addBox(stsc);
        }

        protected void createSdtp(Track track, SampleTableBox stbl)
        {
            if (track.getSampleDependencies() != null && track.getSampleDependencies().Count != 0)
            {
                SampleDependencyTypeBox sdtp = new SampleDependencyTypeBox();
                sdtp.setEntries(track.getSampleDependencies());
                stbl.addBox(sdtp);
            }
        }

        protected void createStss(Track track, SampleTableBox stbl)
        {
            long[] syncSamples = track.getSyncSamples();
            if (syncSamples != null && syncSamples.Length > 0)
            {
                SyncSampleBox stss = new SyncSampleBox();
                stss.setSampleNumber(syncSamples);
                stbl.addBox(stss);
            }
        }

        protected void createCtts(Track track, SampleTableBox stbl)
        {
            List<CompositionTimeToSample.Entry> compositionTimeToSampleEntries = track.getCompositionTimeEntries();
            if (compositionTimeToSampleEntries != null && compositionTimeToSampleEntries.Count != 0)
            {
                CompositionTimeToSample ctts = new CompositionTimeToSample();
                ctts.setEntries(compositionTimeToSampleEntries);
                stbl.addBox(ctts);
            }
        }

        protected void createStts(Track track, SampleTableBox stbl)
        {
            TimeToSampleBox.Entry lastEntry = null;
            List<TimeToSampleBox.Entry> entries = new List<TimeToSampleBox.Entry>();

            foreach (long delta in track.getSampleDurations())
            {
                if (lastEntry != null && lastEntry.getDelta() == delta)
                {
                    lastEntry.setCount(lastEntry.getCount() + 1);
                }
                else
                {
                    lastEntry = new TimeToSampleBox.Entry(1, delta);
                    entries.Add(lastEntry);
                }

            }
            TimeToSampleBox stts = new TimeToSampleBox();
            stts.setEntries(entries);
            stbl.addBox(stts);
        }

        /**
         * Gets the chunk sizes for the given track.
         *
         * @param track the track we are talking about
         * @return the size of each chunk in number of samples
         */
        int[] getChunkSizes(Track track)
        {

            long[] referenceChunkStarts = fragmenter.sampleNumbers(track);
            int[] chunkSizes = new int[referenceChunkStarts.Length];


            for (int i = 0; i < referenceChunkStarts.Length; i++)
            {
                long start = referenceChunkStarts[i] - 1;
                long end;
                if (referenceChunkStarts.Length == i + 1)
                {
                    end = track.getSamples().Count;
                }
                else
                {
                    end = referenceChunkStarts[i + 1] - 1;
                }

                chunkSizes[i] = CastUtils.l2i(end - start);
            }
            Debug.Assert(this.track2Sample[track].Count == sum(chunkSizes), "The number of samples and the sum of all chunk lengths must be equal");
            return chunkSizes;


        }

        public long getTimescale(Movie movie)
        {
            long timescale = 0;
            var tracks = movie.getTracks();
            for (int i = 0; i < tracks.Count; i++)
            {
                Track track = tracks[i];
                if (i == 0)
                    timescale = track.getTrackMetaData().getTimescale();
                else
                    timescale = Mp4Math.lcm(timescale, track.getTrackMetaData().getTimescale());
            }
            return timescale;
        }

        private class InterleaveChunkMdat : Box
        {
            List<Track> tracks;
            List<List<Sample>> chunkList = new List<List<Sample>>();


            long contentSize;

            public InterleaveChunkMdat(Movie movie, Dictionary<Track, int[]> chunks, long contentSize)
            {
                this.contentSize = contentSize;
                this.tracks = movie.getTracks();
                List<Track> tracks = new List<Track>(chunks.Keys);
                tracks = tracks.OrderBy(x => x, new TrackComparer()).ToList();
                Dictionary<Track, int> trackToChunk = new Dictionary<Track, int>();
                Dictionary<Track, int> trackToSample = new Dictionary<Track, int>();
                Dictionary<Track, double> trackToTime = new Dictionary<Track, double>();
                foreach (Track track in tracks)
                {
                    trackToChunk.Add(track, 0);
                    trackToSample.Add(track, 0);
                    trackToTime.Add(track, 0.0);
                }

                while (true)
                {
                    Track nextChunksTrack = null;
                    foreach (Track track in tracks)
                    {
                        if ((nextChunksTrack == null || trackToTime[track] < trackToTime[nextChunksTrack]) &&
                                // either first OR track's next chunk's starttime is smaller than nextTrack's next chunks starttime
                                // AND their need to be chunks left!
                                (trackToChunk[track] < chunks[track].Length))
                        {
                            nextChunksTrack = track;
                        }
                    }
                    if (nextChunksTrack == null)
                    {
                        break;
                    }

                    // found the next one
                    int nextChunksIndex = trackToChunk[nextChunksTrack];
                    int numberOfSampleInNextChunk = chunks[nextChunksTrack][nextChunksIndex];
                    int startSample = trackToSample[nextChunksTrack];
                    double time = trackToTime[nextChunksTrack];
                    for (int j = startSample; j < startSample + numberOfSampleInNextChunk; j++)
                    {
                        time += (double)nextChunksTrack.getSampleDurations()[j] / nextChunksTrack.getTrackMetaData().getTimescale();
                    }
                    chunkList.Add(nextChunksTrack.getSamples().ToList().GetRange(startSample, numberOfSampleInNextChunk));

                    trackToChunk[nextChunksTrack] = nextChunksIndex + 1;
                    trackToSample[nextChunksTrack] = startSample + numberOfSampleInNextChunk;
                    trackToTime[nextChunksTrack] = time;
                }
            }

            public string getType()
            {
                return "mdat";
            }

            public long getSize()
            {
                return 16 + contentSize;
            }

            private bool isSmallBox(long contentSize)
            {
                return (contentSize + 8) < 4294967296L;
            }

            public void getBox(WritableByteChannel writableByteChannel)
            {
                ByteBuffer bb = ByteBuffer.allocate(16);
                long size = getSize();
                if (isSmallBox(size))
                {
                    IsoTypeWriter.writeUInt32(bb, size);
                }
                else
                {
                    IsoTypeWriter.writeUInt32(bb, 1);
                }
                bb.put(IsoFile.fourCCtoBytes("mdat"));
                if (isSmallBox(size))
                {
                    bb.put(new byte[8]);
                }
                else
                {
                    IsoTypeWriter.writeUInt64(bb, size);
                }

                ((Java.Buffer)bb).rewind();
                writableByteChannel.write(bb);
                long writtenBytes = 0;
                long writtenMegaBytes = 0;

                //LOG.debug("About to write {}", contentSize);
                foreach (List<Sample> samples in chunkList)
                {
                    foreach (Sample sample in samples)
                    {
                        sample.writeTo(writableByteChannel);
                        writtenBytes += sample.getSize();
                        if (writtenBytes > 1024 * 1024)
                        {
                            writtenBytes -= 1024 * 1024;
                            writtenMegaBytes++;
                            //LOG.debug("Written {} MB", writtenMegaBytes);
                        }
                    }
                }

            }
        }
    }
}
