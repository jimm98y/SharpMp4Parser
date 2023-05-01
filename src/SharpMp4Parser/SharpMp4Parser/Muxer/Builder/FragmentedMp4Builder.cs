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
     * Creates a fragmented MP4 file.
     */
    public class FragmentedMp4Builder : Mp4Builder
    {
        //private static org.slf4j.Logger LOG = LoggerFactory.getLogger(FragmentedMp4Builder.class.getName());

        protected Fragmenter fragmenter;

        public FragmentedMp4Builder()
        {
        }

        private static long getTrackDuration(Movie movie, Track track)
        {
            return (track.getDuration() * movie.getTimescale()) / track.getTrackMetaData().getTimescale();
        }

        public DateTime getDate()
        {
            return new DateTime();
        }

        public ParsableBox createFtyp(Movie movie)
        {
            List<string> minorBrands = new List<string>();
            minorBrands.Add("mp42");
            minorBrands.Add("iso6");
            minorBrands.Add("avc1");
            minorBrands.Add("isom");
            return new FileTypeBox("iso6", 1, minorBrands);
        }

        protected List<Box> createMoofMdat(Movie movie)
        {
            List<Box> moofsMdats = new List<Box>();
            Dictionary<Track, long[]> intersectionMap = new Dictionary<Track, long[]>();
            Dictionary<Track, double> track2currentTime = new Dictionary<Track, double>();

            foreach (Track track in movie.getTracks())
            {
                long[] intersects = fragmenter.sampleNumbers(track);
                intersectionMap.Add(track, intersects);
                track2currentTime.Add(track, 0.0);
            }

            int sequence = 1;
            while (intersectionMap.Count > 0)
            {
                Track earliestTrack = null;
                double earliestTime = double.MaxValue;
                foreach (var trackEntry in track2currentTime)
                {
                    if (trackEntry.Value < earliestTime)
                    {
                        earliestTime = trackEntry.Value;
                        earliestTrack = trackEntry.Key;
                    }
                }
                Debug.Assert(earliestTrack != null);

                long[] startSamples = intersectionMap[earliestTrack];
                long startSample = startSamples[0];
                long endSample = startSamples.Length > 1 ? startSamples[1] : earliestTrack.getSamples().size() + 1;

                long[] times = earliestTrack.getSampleDurations();
                long timscale = earliestTrack.getTrackMetaData().getTimescale();
                for (long i = startSample; i < endSample; i++)
                {
                    earliestTime += (double)times[CastUtils.l2i(i - 1)] / timscale;
                }
                createFragment(moofsMdats, earliestTrack, startSample, endSample, sequence);

                if (startSamples.Length == 1)
                {
                    intersectionMap.Remove(earliestTrack);
                    track2currentTime.Remove(earliestTrack);
                    // all sample written.
                }
                else
                {
                    long[] nuStartSamples = new long[startSamples.Length - 1];
                    System.Array.Copy(startSamples, 1, nuStartSamples, 0, nuStartSamples.Length);
                    intersectionMap.Add(earliestTrack, nuStartSamples);
                    track2currentTime.Add(earliestTrack, earliestTime);
                }

                sequence++;
            }

            /* sequence = 1;
              // this loop has two indices:

              for (int cycle = 0; cycle < maxNumberOfFragments; cycle++) {

                  final List<Track> sortedTracks = sortTracksInSequence(movie.getTracks(), cycle, intersectionMap);

                  for (Track track : sortedTracks) {
                      long[] startSamples = intersectionMap.get(track);
                      long startSample = startSamples[cycle];
                      // one based sample numbers - the first sample is 1
                      long endSample = cycle + 1 < startSamples.length ? startSamples[cycle + 1] : track.getSamples().size() + 1;
                      createFragment(moofsMdats, track, startSample, endSample, sequence);
                      sequence++;
                  }
              }*/
            return moofsMdats;
        }

        protected int createFragment(List<Box> moofsMdats, Track track, long startSample, long endSample, int sequence)
        {


            // if startSample == endSample the cycle is empty!
            if (startSample != endSample)
            {
                moofsMdats.Add(createMoof(startSample, endSample, track, sequence));
                moofsMdats.Add(createMdat(startSample, endSample, track, sequence));
            }
            return sequence;
        }

        /**
         * {@inheritDoc}
         */
        public IsoParser.Container build(Movie movie)
        {
            //LOG.debug("Creating movie " + movie);
            if (fragmenter == null)
            {
                fragmenter = new DefaultFragmenterImpl(2);
            }
            BasicContainer isoFile = new BasicContainer();


            isoFile.addBox(createFtyp(movie));
            //isoFile.addBox(createPdin(movie));
            isoFile.addBox(createMoov(movie));

            foreach (Box box in createMoofMdat(movie))
            {
                isoFile.addBox(box);
            }
            isoFile.addBox(createMfra(movie, isoFile));

            return isoFile;
        }

        public class Mdat : Box
        {
            long size_ = -1;

            public long getSize()
            {
                if (size_ != -1) return size_;
                long size = 8; // I don't expect 2gig fragments
                foreach (Sample sample in getSamples(startSample, endSample, track))
                {
                    size += sample.getSize();
                }
                size_ = size;
                return size;
            }

            public string getType()
            {
                return "mdat";
            }

            public void getBox(WritableByteChannel writableByteChannel)
            {
                ByteBuffer header = ByteBuffer.allocate(8);
                IsoTypeWriter.writeUInt32(header, CastUtils.l2i(getSize()));
                header.put(IsoFile.fourCCtoBytes(getType()));
                ((Java.Buffer)header).rewind();
                writableByteChannel.write(header);

                List<Sample> samples = getSamples(startSample, endSample, track);
                foreach (Sample sample in samples)
                {
                    sample.writeTo(writableByteChannel);
                }
            }
        }

        protected Box createMdat(long startSample, long endSample, Track track, int i)
        {
            return new Mdat();
        }

        protected void createTfhd(long startSample, long endSample, Track track, int sequenceNumber, TrackFragmentBox parent)
        {
            TrackFragmentHeaderBox tfhd = new TrackFragmentHeaderBox();
            SampleFlags sf = new SampleFlags();

            tfhd.setDefaultSampleFlags(sf);
            tfhd.setBaseDataOffset(-1);
            tfhd.setSampleDescriptionIndex(track.getSampleEntries().IndexOf(track.getSamples()[CastUtils.l2i(startSample)].getSampleEntry()) + 1);
            tfhd.setTrackId(track.getTrackMetaData().getTrackId());
            tfhd.setDefaultBaseIsMoof(true);
            parent.addBox(tfhd);
        }

        protected void createMfhd(long startSample, long endSample, Track track, int sequenceNumber, MovieFragmentBox parent)
        {
            MovieFragmentHeaderBox mfhd = new MovieFragmentHeaderBox();
            mfhd.setSequenceNumber(sequenceNumber);
            parent.addBox(mfhd);
        }

        protected void createTraf(long startSample, long endSample, Track track, int sequenceNumber, MovieFragmentBox parent)
        {
            TrackFragmentBox traf = new TrackFragmentBox();
            parent.addBox(traf);
            createTfhd(startSample, endSample, track, sequenceNumber, traf);
            createTfdt(startSample, track, traf);
            createTrun(startSample, endSample, track, sequenceNumber, traf);

            if (track is CencEncryptedTrack)
            {
                createSaiz(startSample, endSample, (CencEncryptedTrack)track, sequenceNumber, traf);
                createSenc(startSample, endSample, (CencEncryptedTrack)track, sequenceNumber, traf);
                createSaio(startSample, endSample, (CencEncryptedTrack)track, sequenceNumber, traf, parent);
            }


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
                SampleGroupDescriptionBox sgpd = new SampleGroupDescriptionBox();
                string type = sg.Key;
                sgpd.setGroupEntries(sg.Value);
                sgpd.setGroupingType(type);
                SampleToGroupBox sbgp = new SampleToGroupBox();
                sbgp.setGroupingType(type);
                SampleToGroupBox.Entry last = null;
                for (int i = CastUtils.l2i(startSample - 1); i < CastUtils.l2i(endSample - 1); i++)
                {
                    int index = 0;
                    for (int j = 0; j < sg.Value.Count; j++)
                    {
                        GroupEntry groupEntry = sg.Value[j];
                        long[] sampleNums = track.getSampleGroups()[groupEntry];
                        if (Arrays.binarySearch(sampleNums, i) >= 0)
                        {
                            index = j + 0x10001;
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
                traf.addBox(sgpd);
                traf.addBox(sbgp);
            }
        }

        protected void createSenc(long startSample, long endSample, CencEncryptedTrack track, int sequenceNumber, TrackFragmentBox parent)
        {
            SampleEncryptionBox senc = new SampleEncryptionBox();
            senc.setSubSampleEncryption(track.hasSubSampleEncryption());
            senc.setEntries(track.getSampleEncryptionEntries().GetRange(CastUtils.l2i(startSample - 1), CastUtils.l2i(endSample - 1)));
            parent.addBox(senc);
        }

        protected void createSaio(long startSample, long endSample,
                                  CencEncryptedTrack track, int sequenceNumber,
                                  TrackFragmentBox parent, MovieFragmentBox moof)
        {
            SampleAuxiliaryInformationOffsetsBox saio = new SampleAuxiliaryInformationOffsetsBox();
            parent.addBox(saio);
            Debug.Assert(parent.getBoxes<TrackRunBox>(typeof(TrackRunBox)).Count == 1, "Don't know how to deal with multiple Track Run Boxes when encrypting");
            saio.setAuxInfoType("cenc");
            saio.setFlags(1);
            long offset = 0;
            offset += 8; // traf header till 1st child box
            foreach (Box box in parent.getBoxes())
            {
                if (box is SampleEncryptionBox) 
                {
                    offset += ((SampleEncryptionBox)box).getOffsetToFirstIV();
                    break;
                } 
                else
                {
                    offset += box.getSize();
                }
            }
            offset += 16; // traf header till 1st child box
            foreach (Box box in moof.getBoxes())
            {
                if (box == parent)
                {
                    break;
                }
                else
                {
                    offset += box.getSize();
                }

            }

            saio.setOffsets(new long[] { offset });
        }

        protected void createSaiz(long startSample, long endSample, CencEncryptedTrack track, int sequenceNumber, TrackFragmentBox parent)
        {
            SampleEntry se = track.getSampleEntries()[CastUtils.l2i(parent.getTrackFragmentHeaderBox().getSampleDescriptionIndex() - 1)];

            TrackEncryptionBox tenc = Path.getPath< TrackEncryptionBox>((IsoParser.Container)se, "sinf[0]/schi[0]/tenc[0]");

            SampleAuxiliaryInformationSizesBox saiz = new SampleAuxiliaryInformationSizesBox();
            saiz.setAuxInfoType("cenc");
            saiz.setFlags(1);
            if (track.hasSubSampleEncryption())
            {
                short[] sizes = new short[CastUtils.l2i(endSample - startSample)];
                List<CencSampleAuxiliaryDataFormat> auxs =
                        track.getSampleEncryptionEntries().GetRange(CastUtils.l2i(startSample - 1), CastUtils.l2i(endSample - 1));
                for (int i = 0; i < sizes.Length; i++)
                {
                    sizes[i] = (short)auxs[i].getSize();
                }
                saiz.setSampleInfoSizes(sizes);
            }
            else
            {
                Debug.Assert(tenc != null);
                saiz.setDefaultSampleInfoSize(tenc.getDefaultIvSize());
                saiz.setSampleCount(CastUtils.l2i(endSample - startSample));
            }
            parent.addBox(saiz);
        }

        /**
         * Gets all samples starting with <code>startSample</code> (one based -&gt; one is the first) and
         * ending with <code>endSample</code> (exclusive).
         *
         * @param startSample low endpoint (inclusive) of the sample sequence
         * @param endSample   high endpoint (exclusive) of the sample sequence
         * @param track       source of the samples
         * @return a <code>List&lt;Sample&gt;</code> of raw samples
         */
        protected List<Sample> getSamples(long startSample, long endSample, Track track)
        {
            // since startSample and endSample are one-based substract 1 before addressing list elements
            return track.getSamples().GetRange(CastUtils.l2i(startSample) - 1, CastUtils.l2i(endSample) - 1);
        }

        /**
         * Gets the sizes of a sequence of samples.
         *
         * @param startSample    low endpoint (inclusive) of the sample sequence
         * @param endSample      high endpoint (exclusive) of the sample sequence
         * @param track          source of the samples
         * @param sequenceNumber the fragment index of the requested list of samples
         * @return the sample sizes in the given interval
         */
        protected long[] getSampleSizes(long startSample, long endSample, Track track, int sequenceNumber)
        {
            List<Sample> samples = getSamples(startSample, endSample, track);

            long[] sampleSizes = new long[samples.Count];
            for (int i = 0; i < sampleSizes.Length; i++)
            {
                sampleSizes[i] = samples[i].getSize();
            }
            return sampleSizes;
        }

        protected void createTfdt(long startSample, Track track, TrackFragmentBox parent)
        {
            TrackFragmentBaseMediaDecodeTimeBox tfdt = new TrackFragmentBaseMediaDecodeTimeBox();
            tfdt.setVersion(1);
            long startTime = 0;
            long[] times = track.getSampleDurations();
            for (int i = 1; i < startSample; i++)
            {
                startTime += times[i - 1];
            }
            tfdt.setBaseMediaDecodeTime(startTime);
            parent.addBox(tfdt);
        }

        /**
         * Creates one or more track run boxes for a given sequence.
         *
         * @param startSample    low endpoint (inclusive) of the sample sequence
         * @param endSample      high endpoint (exclusive) of the sample sequence
         * @param track          source of the samples
         * @param sequenceNumber the fragment index of the requested list of samples
         * @param parent         the created box must be added to this box
         */
        protected void createTrun(long startSample, long endSample, Track track, int sequenceNumber, TrackFragmentBox parent)
        {
            TrackRunBox trun = new TrackRunBox();
            trun.setVersion(1);
            long[] sampleSizes = getSampleSizes(startSample, endSample, track, sequenceNumber);

            trun.setSampleDurationPresent(true);
            trun.setSampleSizePresent(true);
            List<TrackRunBox.Entry> entries = new List<TrackRunBox.Entry>(CastUtils.l2i(endSample - startSample));


            List<CompositionTimeToSample.Entry> compositionTimeEntries = track.getCompositionTimeEntries();
            int compositionTimeQueueIndex = 0;
            CompositionTimeToSample.Entry[] compositionTimeQueue =
                    compositionTimeEntries != null && compositionTimeEntries.Count > 0 ?
                            compositionTimeEntries.ToArray() : null;
            long compositionTimeEntriesLeft = compositionTimeQueue != null ? compositionTimeQueue[compositionTimeQueueIndex].getCount() : -1;

            trun.setSampleCompositionTimeOffsetPresent(compositionTimeEntriesLeft > 0);

            // fast forward composition stuff
            for (long i = 1; i < startSample; i++)
            {
                if (compositionTimeQueue != null)
                {
                    //trun.setSampleCompositionTimeOffsetPresent(true);
                    if (--compositionTimeEntriesLeft == 0 && (compositionTimeQueue.Length - compositionTimeQueueIndex) > 1)
                    {
                        compositionTimeQueueIndex++;
                        compositionTimeEntriesLeft = compositionTimeQueue[compositionTimeQueueIndex].getCount();
                    }
                }
            }

            bool sampleFlagsRequired = (track.getSampleDependencies() != null && track.getSampleDependencies().Count > 0 ||
                    track.getSyncSamples() != null && track.getSyncSamples().Length != 0);

            trun.setSampleFlagsPresent(sampleFlagsRequired);

            for (int i = 0; i < sampleSizes.Length; i++)
            {
                TrackRunBox.Entry entry = new TrackRunBox.Entry();
                entry.setSampleSize(sampleSizes[i]);
                if (sampleFlagsRequired)
                {
                    //if (false) {
                    SampleFlags sflags = new SampleFlags();

                    if (track.getSampleDependencies() != null && track.getSampleDependencies().Count != 0)
                    {
                        SampleDependencyTypeBox.Entry e = track.getSampleDependencies()[i];
                        sflags.setSampleDependsOn(e.getSampleDependsOn());
                        sflags.setSampleIsDependedOn(e.getSampleIsDependedOn());
                        sflags.setSampleHasRedundancy(e.getSampleHasRedundancy());
                    }
                    if (track.getSyncSamples() != null && track.getSyncSamples().Length > 0)
                    {
                        // we have to mark non-sync samples!
                        if (Arrays.binarySearch(track.getSyncSamples(), startSample + i) >= 0)
                        {
                            sflags.setSampleIsDifferenceSample(false);
                            sflags.setSampleDependsOn(2);
                        }
                        else
                        {
                            sflags.setSampleIsDifferenceSample(true);
                            sflags.setSampleDependsOn(1);
                        }
                    }
                    // i don't have sample degradation
                    entry.setSampleFlags(sflags);

                }

                entry.setSampleDuration(track.getSampleDurations()[CastUtils.l2i(startSample + i - 1)]);

                if (compositionTimeQueue != null)
                {
                    entry.setSampleCompositionTimeOffset(compositionTimeQueue[compositionTimeQueueIndex].getOffset());
                    if (--compositionTimeEntriesLeft == 0 && (compositionTimeQueue.Length - compositionTimeQueueIndex) > 1)
                    {
                        compositionTimeQueueIndex++;
                        compositionTimeEntriesLeft = compositionTimeQueue[compositionTimeQueueIndex].getCount();
                    }
                }
                entries.Add(entry);
            }

            trun.setEntries(entries);

            parent.addBox(trun);
        }

        /**
         * Creates a 'moof' box for a given sequence of samples.
         *
         * @param startSample    low endpoint (inclusive) of the sample sequence
         * @param endSample      high endpoint (exclusive) of the sample sequence
         * @param track          source of the samples
         * @param sequenceNumber the fragment index of the requested list of samples
         * @return the list of TrackRun boxes.
         */
        protected ParsableBox createMoof(long startSample, long endSample, Track track, int sequenceNumber)
        {
            MovieFragmentBox moof = new MovieFragmentBox();
            createMfhd(startSample, endSample, track, sequenceNumber, moof);
            createTraf(startSample, endSample, track, sequenceNumber, moof);

            TrackRunBox firstTrun = moof.getTrackRunBoxes()[0];
            firstTrun.setDataOffset(1); // dummy to make size correct
            firstTrun.setDataOffset((int)(8 + moof.getSize())); // mdat header + moof size

            return moof;
        }

        /**
         * Creates a single 'mvhd' movie header box for a given movie.
         *
         * @param movie the concerned movie
         * @return an 'mvhd' box
         */
        protected ParsableBox createMvhd(Movie movie)
        {
            MovieHeaderBox mvhd = new MovieHeaderBox();
            mvhd.setVersion(1);
            mvhd.setCreationTime(getDate());
            mvhd.setModificationTime(getDate());
            mvhd.setDuration(0);//no duration in moov for fragmented movies
            long movieTimeScale = movie.getTimescale();
            mvhd.setTimescale(movieTimeScale);
            // find the next available trackId
            long nextTrackId = 0;
            foreach (Track track in movie.getTracks()) 
            {
                nextTrackId = nextTrackId < track.getTrackMetaData().getTrackId() ? track.getTrackMetaData().getTrackId() : nextTrackId;
            }
            mvhd.setNextTrackId(++nextTrackId);
            return mvhd;
        }

        /**
         * Creates a fully populated 'moov' box with all child boxes. Child boxes are:
         * <ul>
         * <li>{@link #createMvhd(Movie) mvhd}</li>
         * <li>{@link #createMvex(Movie)  mvex}</li>
         * <li>a {@link #createTrak(Track, Movie)  trak} for every track</li>
         * </ul>
         *
         * @param movie the concerned movie
         * @return fully populated 'moov'
         */
        protected ParsableBox createMoov(Movie movie)
        {
            MovieBox movieBox = new MovieBox();

            movieBox.addBox(createMvhd(movie));
            foreach (Track track in movie.getTracks())
            {
                movieBox.addBox(createTrak(track, movie));
            }
            movieBox.addBox(createMvex(movie));

            // metadata here
            return movieBox;
        }

        /**
         * Creates a 'tfra' - track fragment random access box for the given track with the isoFile.
         * The tfra contains a map of random access points with time as key and offset within the isofile
         * as value.
         *
         * @param track   the concerned track
         * @param isoFile the track is contained in
         * @return a track fragment random access box.
         */
        protected Box createTfra(Track track, IsoParser.Container isoFile)
        {
            TrackFragmentRandomAccessBox tfra = new TrackFragmentRandomAccessBox();
            tfra.setVersion(1); // use long offsets and times
            List<TrackFragmentRandomAccessBox.Entry> offset2timeEntries = new List<TrackFragmentRandomAccessBox.Entry>();

            TrackExtendsBox trex = null;
            List<TrackExtendsBox> trexs = Path.getPaths< TrackExtendsBox>(isoFile, "moov/mvex/trex");
            foreach (TrackExtendsBox innerTrex in trexs)
            {
                if (innerTrex.getTrackId() == track.getTrackMetaData().getTrackId())
                {
                    trex = innerTrex;
                }
            }

            long offset = 0;
            long duration = 0;

            foreach (Box box in isoFile.getBoxes())
            {
                if (box is MovieFragmentBox)
                {
                    List<TrackFragmentBox> trafs = ((MovieFragmentBox)box).getBoxes< TrackFragmentBox>(typeof(TrackFragmentBox));
                    for (int i = 0; i < trafs.Count; i++)
                    {
                        TrackFragmentBox traf = trafs[i];

                        if (traf.getTrackFragmentHeaderBox().getTrackId() == track.getTrackMetaData().getTrackId())
                        {

                            // here we are at the offset required for the current entry.
                            List<TrackRunBox> truns = traf.getBoxes< TrackRunBox>(typeof(TrackRunBox));
                            for (int j = 0; j < truns.Count; j++)
                            {
                                List<TrackFragmentRandomAccessBox.Entry> offset2timeEntriesThisTrun = new List<TrackFragmentRandomAccessBox.Entry>();
                                TrackRunBox trun = truns[j];
                                for (int k = 0; k < trun.getEntries().Count; k++)
                                {
                                    TrackRunBox.Entry trunEntry = trun.getEntries()[k];
                                    SampleFlags sf;
                                    if (k == 0 && trun.isFirstSampleFlagsPresent())
                                    {
                                        sf = trun.getFirstSampleFlags();
                                    }
                                    else if (trun.isSampleFlagsPresent())
                                    {
                                        sf = trunEntry.getSampleFlags();
                                    }
                                    else
                                    {
                                        sf = trex.getDefaultSampleFlags();
                                    }
                                    if (sf == null && track.getHandler().Equals("vide"))
                                    {
                                        throw new Exception("Cannot find SampleFlags for video track but it's required to build tfra");
                                    }
                                    if (sf == null || sf.getSampleDependsOn() == 2)
                                    {
                                        offset2timeEntriesThisTrun.Add(new TrackFragmentRandomAccessBox.Entry(
                                                duration,
                                                offset,
                                                i + 1, j + 1, k + 1));
                                    }
                                    duration += trunEntry.getSampleDuration();
                                }
                                if (offset2timeEntriesThisTrun.Count == trun.getEntries().Count && trun.getEntries().Count > 0)
                                {
                                    // Oooops every sample seems to be random access sample
                                    // is this an audio track? I don't care.
                                    // I just use the first for trun sample for tfra random access
                                    offset2timeEntries.Add(offset2timeEntriesThisTrun[0]);
                                }
                                else
                                {
                                    offset2timeEntries.AddRange(offset2timeEntriesThisTrun);
                                }
                            }
                        }
                    }
                }


                offset += box.getSize();
            }
            tfra.setEntries(offset2timeEntries);
            tfra.setTrackId(track.getTrackMetaData().getTrackId());
            return tfra;
        }

        /**
         * Creates a 'mfra' - movie fragment random access box for the given movie in the given
         * isofile. Uses {@link #createTfra(Track, Container)}
         * to generate the child boxes.
         *
         * @param movie   concerned movie
         * @param isoFile concerned isofile
         * @return a complete 'mfra' box
         */
        protected ParsableBox createMfra(Movie movie, IsoParser.Container isoFile)
        {
            MovieFragmentRandomAccessBox mfra = new MovieFragmentRandomAccessBox();
            foreach (Track track in movie.getTracks())
            {
                mfra.addBox(createTfra(track, isoFile));
            }

            MovieFragmentRandomAccessOffsetBox mfro = new MovieFragmentRandomAccessOffsetBox();
            mfra.addBox(mfro);
            mfro.setMfraSize(mfra.getSize());
            return mfra;
        }

        protected ParsableBox createTrex(Movie movie, Track track)
        {
            TrackExtendsBox trex = new TrackExtendsBox();
            trex.setTrackId(track.getTrackMetaData().getTrackId());
            trex.setDefaultSampleDescriptionIndex(1);
            trex.setDefaultSampleDuration(0);
            trex.setDefaultSampleSize(0);
            SampleFlags sf = new SampleFlags();
            if ("soun".Equals(track.getHandler()) || "subt".Equals(track.getHandler()))
            {
                // as far as I know there is no audio encoding
                // where the sample are not self contained.
                // same seems to be true for subtitle tracks
                sf.setSampleDependsOn(2);
                sf.setSampleIsDependedOn(2);
            }
            trex.setDefaultSampleFlags(sf);
            return trex;
        }

        /**
         * Creates a 'mvex' - movie extends box and populates it with 'trex' boxes
         * by calling {@link #createTrex(Movie, Track)}
         * for each track to generate them
         *
         * @param movie the source movie
         * @return a complete 'mvex'
         */
        protected ParsableBox createMvex(Movie movie)
        {
            MovieExtendsBox mvex = new MovieExtendsBox();
            MovieExtendsHeaderBox mved = new MovieExtendsHeaderBox();
            mved.setVersion(1);
            foreach (Track track in movie.getTracks())
            {
                long trackDuration = getTrackDuration(movie, track);
                if (mved.getFragmentDuration() < trackDuration)
                {
                    mved.setFragmentDuration(trackDuration);
                }
            }
            mvex.addBox(mved);

            foreach (Track track in movie.getTracks())
            {
                mvex.addBox(createTrex(movie, track));
            }
            return mvex;
        }

        protected ParsableBox createTkhd(Movie movie, Track track)
        {
            TrackHeaderBox tkhd = new TrackHeaderBox();
            tkhd.setVersion(1);
            tkhd.setFlags(7); // enabled, in movie, in previe, in poster

            tkhd.setAlternateGroup(track.getTrackMetaData().getGroup());
            tkhd.setCreationTime(track.getTrackMetaData().getCreationTime());
            // We need to take edit list box into account in trackheader duration
            // but as long as I don't support edit list boxes it is sufficient to
            // just translate media duration to movie timescale
            tkhd.setDuration(0);//no duration in moov for fragmented movies
            tkhd.setHeight(track.getTrackMetaData().getHeight());
            tkhd.setWidth(track.getTrackMetaData().getWidth());
            tkhd.setLayer(track.getTrackMetaData().getLayer());
            tkhd.setModificationTime(getDate());
            tkhd.setTrackId(track.getTrackMetaData().getTrackId());
            tkhd.setVolume(track.getTrackMetaData().getVolume());
            return tkhd;
        }

        protected ParsableBox createMdhd(Movie movie, Track track)
        {
            MediaHeaderBox mdhd = new MediaHeaderBox();
            mdhd.setCreationTime(track.getTrackMetaData().getCreationTime());
            mdhd.setModificationTime(getDate());
            mdhd.setDuration(0);//no duration in moov for fragmented movies
            mdhd.setTimescale(track.getTrackMetaData().getTimescale());
            mdhd.setLanguage(track.getTrackMetaData().getLanguage());
            return mdhd;
        }

        protected ParsableBox createStbl(Movie movie, Track track)
        {
            SampleTableBox stbl = new SampleTableBox();

            createStsd(track, stbl);
            stbl.addBox(new TimeToSampleBox());
            stbl.addBox(new SampleToChunkBox());
            stbl.addBox(new SampleSizeBox());
            stbl.addBox(new StaticChunkOffsetBox());
            return stbl;
        }

        protected void createStsd(Track track, SampleTableBox stbl)
        {
            SampleDescriptionBox stsd = new SampleDescriptionBox();
            stsd.setBoxes(track.getSampleEntries().Select(x => (Box)x).ToList());
            stbl.addBox(stsd);
        }

        protected ParsableBox createMinf(Track track, Movie movie)
        {
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
            minf.addBox(createDinf(movie, track));
            minf.addBox(createStbl(movie, track));
            return minf;
        }

        protected ParsableBox createMdiaHdlr(Track track, Movie movie)
        {
            HandlerBox hdlr = new HandlerBox();
            hdlr.setHandlerType(track.getHandler());
            return hdlr;
        }

        protected ParsableBox createMdia(Track track, Movie movie)
        {
            MediaBox mdia = new MediaBox();
            mdia.addBox(createMdhd(movie, track));


            mdia.addBox(createMdiaHdlr(track, movie));


            mdia.addBox(createMinf(track, movie));
            return mdia;
        }

        protected ParsableBox createTrak(Track track, Movie movie)
        {
            //LOG.debug("Creating Track " + track);
            TrackBox trackBox = new TrackBox();
            trackBox.addBox(createTkhd(movie, track));
            ParsableBox edts = createEdts(track, movie);
            if (edts != null)
            {
                trackBox.addBox(edts);
            }
            trackBox.addBox(createMdia(track, movie));
            return trackBox;
        }

        protected ParsableBox createEdts(Track track, Movie movie)
        {
            if (track.getEdits() != null && track.getEdits().Count > 0)
            {
                EditListBox elst = new EditListBox();
                elst.setVersion(1);
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
            } else
            {
                return null;
            }
        }

        protected DataInformationBox createDinf(Movie movie, Track track)
        {
            DataInformationBox dinf = new DataInformationBox();
            DataReferenceBox dref = new DataReferenceBox();
            dinf.addBox(dref);
            DataEntryUrlBox url = new DataEntryUrlBox();
            url.setFlags(1);
            dref.addBox(url);
            return dinf;
        }

        public Fragmenter getFragmenter()
        {
            return fragmenter;
        }

        public void setFragmenter(Fragmenter fragmenter)
        {
            this.fragmenter = fragmenter;
        }
    }
}