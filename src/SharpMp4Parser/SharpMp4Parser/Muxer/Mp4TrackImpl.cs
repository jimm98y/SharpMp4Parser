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
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Boxes.SampleGrouping;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Muxer.Container.MP4;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SharpMp4Parser.Muxer
{
    /**
     * Represents a single track of an MP4 file.
     */
    public class Mp4TrackImpl : AbstractTrack
    {
        private Mp4SampleList samples;
        private SampleDescriptionBox sampleDescriptionBox;
        private long[] decodingTimes;
        private List<CompositionTimeToSample.Entry> compositionTimeEntries;
        private long[] syncSamples = null;
        private List<SampleDependencyTypeBox.Entry> sampleDependencies;
        private TrackMetaData trackMetaData = new TrackMetaData();
        private string handler;
        private SubSampleInformationBox subSampleInformationBox = null;

        /**
         * Creates a track from a TrackBox and potentially fragments. Use <b>fragements parameter
         * only</b> to supply additional fragments that are not located in the main file.
         *
         * @param trackId      ID of the track to extract
         * @param isofile      the parsed MP4 file
         * @param randomAccess the RandomAccessSource to read the samples from
         * @param name         an arbitrary naem to identify track later - e.g. filename
         */
        public Mp4TrackImpl(long trackId, IsoParser.Container isofile, RandomAccessSource randomAccess, string name) : base(name)
        {
            samples = new Mp4SampleList(trackId, isofile, randomAccess);
            TrackBox trackBox = null;
            foreach (TrackBox box in Path.getPaths<TrackBox>(isofile, "moov/trak"))
            {
                if (box.getTrackHeaderBox().getTrackId() == trackId)
                {
                    trackBox = box;
                    break;
                }
            }
            Debug.Assert(trackBox != null, "Could not find TrackBox with trackID " + trackId);
            SampleTableBox stbl = trackBox.getMediaBox().getMediaInformationBox().getSampleTableBox();

            handler = trackBox.getMediaBox().getHandlerBox().getHandlerType();

            List<TimeToSampleBox.Entry> decodingTimeEntries = new List<TimeToSampleBox.Entry>();
            compositionTimeEntries = new List<CompositionTimeToSample.Entry>();
            sampleDependencies = new List<SampleDependencyTypeBox.Entry>();

            decodingTimeEntries.AddRange(stbl.getTimeToSampleBox().getEntries());
            if (stbl.getCompositionTimeToSample() != null)
            {
                compositionTimeEntries.AddRange(stbl.getCompositionTimeToSample().getEntries());
            }
            if (stbl.getSampleDependencyTypeBox() != null)
            {
                sampleDependencies.AddRange(stbl.getSampleDependencyTypeBox().getEntries());
            }
            if (stbl.getSyncSampleBox() != null)
            {
                syncSamples = stbl.getSyncSampleBox().getSampleNumber();
            }
            subSampleInformationBox = Path.getPath<SubSampleInformationBox>(stbl, "subs");

            // gather all movie fragment boxes from the fragments
            List<MovieFragmentBox> movieFragmentBoxes = new List<MovieFragmentBox>();
            movieFragmentBoxes.AddRange(isofile.getBoxes< MovieFragmentBox>(typeof(MovieFragmentBox)));

            sampleDescriptionBox = stbl.getSampleDescriptionBox();
            int lastSubsSample = 0;
            List<MovieExtendsBox> movieExtendsBoxes = Path.getPaths< MovieExtendsBox>(isofile, "moov/mvex");
            if (movieExtendsBoxes.Count > 0) {
                foreach (MovieExtendsBox mvex in movieExtendsBoxes)
                {
                    List<TrackExtendsBox> trackExtendsBoxes = mvex.getBoxes<TrackExtendsBox>(typeof(TrackExtendsBox));
                    foreach (TrackExtendsBox trex in trackExtendsBoxes)
                    {
                        if (trex.getTrackId() == trackId)
                        {
                            List<SubSampleInformationBox> subss = Path.getPaths< SubSampleInformationBox>(isofile, "moof/traf/subs");
                            if (subss.Count > 0)
                            {
                                subSampleInformationBox = new SubSampleInformationBox();
                            }

                            long sampleNumber = 1;
                            foreach (MovieFragmentBox movieFragmentBox in movieFragmentBoxes)
                            {
                                List<TrackFragmentBox> trafs = movieFragmentBox.getBoxes< TrackFragmentBox>(typeof(TrackFragmentBox));
                                foreach (TrackFragmentBox traf in trafs)
                                {
                                    if (traf.getTrackFragmentHeaderBox().getTrackId() == trackId)
                                    {
                                        sampleGroups = getSampleGroups(
                                                stbl.getBoxes< SampleGroupDescriptionBox>(typeof(SampleGroupDescriptionBox)),  // global descriptions
                                                                            Path.getPaths<SampleGroupDescriptionBox>((IsoParser.Container)traf, "sgpd"),  // local description
                                                                            Path.getPaths<SampleToGroupBox>((IsoParser.Container)traf, "sbgp"),
                                                                            sampleGroups, sampleNumber - 1);

                                        SubSampleInformationBox subs = Path.getPath<SubSampleInformationBox>(traf, "subs");
                                        if (subs != null)
                                        {
                                            long difFromLastFragment = sampleNumber - lastSubsSample - 1;
                                            foreach (SubSampleInformationBox.SubSampleEntry subSampleEntry in subs.getEntries())
                                            {
                                                SubSampleInformationBox.SubSampleEntry se = new SubSampleInformationBox.SubSampleEntry();
                                                se.getSubsampleEntries().AddRange(subSampleEntry.getSubsampleEntries());
                                                if (difFromLastFragment != 0)
                                                {
                                                    se.setSampleDelta(difFromLastFragment + subSampleEntry.getSampleDelta());
                                                    difFromLastFragment = 0;
                                                }
                                                else
                                                {
                                                    se.setSampleDelta(subSampleEntry.getSampleDelta());
                                                }
                                                subSampleInformationBox.getEntries().Add(se);
                                            }
                                        }


                                        List<TrackRunBox> truns = traf.getBoxes< TrackRunBox>(typeof(TrackRunBox));
                                        foreach (TrackRunBox trun in truns)
                                        {
                                            TrackFragmentHeaderBox tfhd = traf.getTrackFragmentHeaderBox();
                                            bool first = true;
                                            foreach (TrackRunBox.Entry entry in trun.getEntries())
                                            {
                                                if (trun.isSampleDurationPresent())
                                                {
                                                    if (decodingTimeEntries.Count == 0 ||
                                                            decodingTimeEntries[decodingTimeEntries.Count - 1].getDelta() != entry.getSampleDuration())
                                                    {
                                                        decodingTimeEntries.Add(new TimeToSampleBox.Entry(1, entry.getSampleDuration()));
                                                    }
                                                    else
                                                    {
                                                        TimeToSampleBox.Entry e = decodingTimeEntries[decodingTimeEntries.Count - 1];
                                                        e.setCount(e.getCount() + 1);
                                                    }
                                                }
                                                else
                                                {
                                                    if (tfhd.hasDefaultSampleDuration())
                                                    {
                                                        decodingTimeEntries.Add(new TimeToSampleBox.Entry(1, tfhd.getDefaultSampleDuration()));
                                                    }
                                                    else
                                                    {
                                                        decodingTimeEntries.Add(new TimeToSampleBox.Entry(1, trex.getDefaultSampleDuration()));
                                                    }
                                                }

                                                if (trun.isSampleCompositionTimeOffsetPresent())
                                                {
                                                    if (compositionTimeEntries.Count == 0 ||
                                                            compositionTimeEntries[compositionTimeEntries.Count - 1].getOffset() != entry.getSampleCompositionTimeOffset())
                                                    {
                                                        compositionTimeEntries.Add(new CompositionTimeToSample.Entry(1, CastUtils.l2i(entry.getSampleCompositionTimeOffset())));
                                                    }
                                                    else
                                                    {
                                                        CompositionTimeToSample.Entry e = compositionTimeEntries[compositionTimeEntries.Count - 1];
                                                        e.setCount(e.getCount() + 1);
                                                    }
                                                }
                                                SampleFlags sampleFlags;
                                                if (trun.isSampleFlagsPresent())
                                                {
                                                    sampleFlags = entry.getSampleFlags();
                                                }
                                                else
                                                {
                                                    if (first && trun.isFirstSampleFlagsPresent())
                                                    {
                                                        sampleFlags = trun.getFirstSampleFlags();
                                                    }
                                                    else
                                                    {
                                                        if (tfhd.hasDefaultSampleFlags())
                                                        {
                                                            sampleFlags = tfhd.getDefaultSampleFlags();
                                                        }
                                                        else
                                                        {
                                                            sampleFlags = trex.getDefaultSampleFlags();
                                                        }
                                                    }
                                                }
                                                if (sampleFlags != null && !sampleFlags.isSampleIsDifferenceSample())
                                                {
                                                    //iframe
                                                    syncSamples = Mp4Arrays.copyOfAndAppend(syncSamples, sampleNumber);
                                                }
                                                sampleNumber++;
                                                first = false;
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
                foreach (MovieFragmentBox movieFragmentBox in movieFragmentBoxes)
                {
                    foreach (TrackFragmentBox traf in movieFragmentBox.getBoxes<TrackFragmentBox>(typeof(TrackFragmentBox))) 
                    {
                        if (traf.getTrackFragmentHeaderBox().getTrackId() == trackId)
                        {
                            sampleGroups = getSampleGroups(
                                    stbl.getBoxes<SampleGroupDescriptionBox>(typeof(SampleGroupDescriptionBox)),
                                                    Path.getPaths<SampleGroupDescriptionBox>((IsoParser.Container)traf, "sgpd"),
                                                    Path.getPaths<SampleToGroupBox>((IsoParser.Container)traf, "sbgp"), sampleGroups, 0);
                        }
                    }
                }
            } 
            else
            {
                sampleGroups = getSampleGroups(stbl.getBoxes<SampleGroupDescriptionBox>(typeof(SampleGroupDescriptionBox)), null, stbl.getBoxes<SampleToGroupBox>(typeof(SampleToGroupBox)), sampleGroups, 0);
            }

            decodingTimes = TimeToSampleBox.blowupTimeToSamples(decodingTimeEntries);

            MediaHeaderBox mdhd = trackBox.getMediaBox().getMediaHeaderBox();
            TrackHeaderBox tkhd = trackBox.getTrackHeaderBox();

            trackMetaData.setTrackId(tkhd.getTrackId());
            trackMetaData.setCreationTime(mdhd.getCreationTime());
            trackMetaData.setLanguage(mdhd.getLanguage());

            trackMetaData.setModificationTime(mdhd.getModificationTime());
            trackMetaData.setTimescale(mdhd.getTimescale());
            trackMetaData.setHeight(tkhd.getHeight());
            trackMetaData.setWidth(tkhd.getWidth());
            trackMetaData.setLayer(tkhd.getLayer());
            trackMetaData.setMatrix(tkhd.getMatrix());
            trackMetaData.setVolume(tkhd.getVolume());
            EditListBox elst = Path.getPath< EditListBox>(trackBox, "edts/elst");
            MovieHeaderBox mvhd = Path.getPath< MovieHeaderBox>(isofile, "moov/mvhd");
            if (elst != null)
            {
                Debug.Assert(mvhd != null);
                foreach (EditListBox.Entry e in elst.getEntries())
                {
                    edits.Add(new Edit(e.getMediaTime(), mdhd.getTimescale(), e.getMediaRate(), (double)e.getSegmentDuration() / mvhd.getTimescale()));
                }
            }

        }

        private Dictionary<GroupEntry, long[]> getSampleGroups(List<SampleGroupDescriptionBox> globalSgdbs, List<SampleGroupDescriptionBox> localSgdbs, List<SampleToGroupBox> sbgps,
                                                        Dictionary<GroupEntry, long[]> sampleGroups, long startIndex)
        {

            foreach (SampleToGroupBox sbgp in sbgps)
            {
                int sampleNum = 0;
                foreach (SampleToGroupBox.Entry entry in sbgp.getEntries())
                {
                    if (entry.getGroupDescriptionIndex() > 0)
                    {
                        GroupEntry groupEntry = null;
                        if (entry.getGroupDescriptionIndex() > 0xffff)
                        {
                            foreach (SampleGroupDescriptionBox localSgdb in localSgdbs)
                            {
                                if (localSgdb.getGroupingType().Equals(sbgp.getGroupingType()))
                                {
                                    groupEntry = localSgdb.getGroupEntries()[(entry.getGroupDescriptionIndex() - 1) & 0xffff];
                                }
                            }
                        }
                        else
                        {
                            foreach (SampleGroupDescriptionBox globalSgdb in globalSgdbs)
                            {
                                if (globalSgdb.getGroupingType().Equals(sbgp.getGroupingType()))
                                {
                                    groupEntry = globalSgdb.getGroupEntries()[(entry.getGroupDescriptionIndex() - 1)];
                                }
                            }
                        }
                        Debug.Assert(groupEntry != null);
                        long[] samples = sampleGroups[groupEntry];
                        if (samples == null)
                        {
                            samples = new long[0];
                        }

                        long[] nuSamples = new long[CastUtils.l2i(entry.getSampleCount()) + samples.Length];
                        System.Array.Copy(samples, 0, nuSamples, 0, samples.Length);
                        for (int i = 0; i < entry.getSampleCount(); i++)
                        {
                            nuSamples[samples.Length + i] = startIndex + sampleNum + i;
                        }
                        sampleGroups.Add(groupEntry, nuSamples);

                    }
                    sampleNum += (int)entry.getSampleCount();
                }
            }


            return sampleGroups;
        }

        public override void close()
        {

        }

        public override IList<Sample> getSamples()
        {
            return samples;
        }

        public override long[] getSampleDurations()
        {
            return decodingTimes;
        }

        public override List<SampleEntry> getSampleEntries()
        {
            return sampleDescriptionBox.getBoxes< SampleEntry>(typeof(SampleEntry));
        }

        public override List<CompositionTimeToSample.Entry> getCompositionTimeEntries()
        {
            return compositionTimeEntries;
        }

        public override long[] getSyncSamples()
        {
            if (syncSamples == null || syncSamples.Length == samples.Count)
            {
                return null;
            }
            else
            {
                return syncSamples;
            }
        }

        public override List<SampleDependencyTypeBox.Entry> getSampleDependencies()
        {
            return sampleDependencies;
        }

        public override TrackMetaData getTrackMetaData()
        {
            return trackMetaData;
        }

        public override string getHandler()
        {
            return handler;
        }

        public override SubSampleInformationBox getSubsampleInformationBox()
        {
            return subSampleInformationBox;
        }
    }
}
