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
using SharpMp4Parser.Java;
using System.Collections.Generic;
using System;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.IsoParser;
using System.Diagnostics;

namespace SharpMp4Parser.Muxer.Builder
{
    /**
     * This <code>FragmentIntersectionFinder</code> cuts the input movie video tracks in
     * fragments of the same length exactly before the sync samples. Audio tracks are cut
     * into pieces of similar length.
     */
    public class SyncSampleIntersectFinderImpl : Fragmenter
    {
        //private static Logger LOG = LoggerFactory.getLogger(SyncSampleIntersectFinderImpl.class.getName());

        private readonly int minFragmentDurationSeconds;
        private Movie movie;
        private Track referenceTrack;

        /**
         * Creates a <code>SyncSampleIntersectFinderImpl</code> that will not create any fragment
         * smaller than the given <code>minFragmentDurationSeconds</code>
         *
         * @param movie                      this movie is the reference
         * @param referenceTrack             used for audio tracks to find similar boundaries of segments.
         * @param minFragmentDurationSeconds the smallest allowable duration of a fragment.
         */
        public SyncSampleIntersectFinderImpl(Movie movie, Track referenceTrack, int minFragmentDurationSeconds)
        {
            this.movie = movie;
            this.referenceTrack = referenceTrack;
            this.minFragmentDurationSeconds = minFragmentDurationSeconds;
        }

        static string getFormat(Track track)
        {
            string type = null;
            foreach (SampleEntry sampleEntry in track.getSampleEntries())
            {

                OriginalFormatBox frma;
                frma = Path.getPath< OriginalFormatBox>((Container)sampleEntry, "sinf/frma");
                string _type;
                if (frma != null)
                {
                    _type = frma.getDataFormat();
                }
                else
                {
                    _type = sampleEntry.getType();
                }
                if (type == null)
                {
                    type = _type;
                }
                else
                {
                    if (!type.Equals(_type))
                    {
                        throw new Exception("The SyncSampleIntersectionFindler only works when all SampleEntries are of the same type. " + type + " vs. " + _type);
                    }
                }
            }
            return type;
        }

        /**
         * Calculates the timestamp of all tracks' sync samples.
         *
         * @param movie <code>track</code> is located in this movie
         * @param track get this track's samples timestamps
         * @return a list of timestamps
         */
        public static List<long[]> getSyncSamplesTimestamps(Movie movie, Track track)
        {
            List<long[]> times = new List<long[]>();
            foreach (Track currentTrack in movie.getTracks())
            {
                if (currentTrack.getHandler().Equals(track.getHandler()))
                {
                    long[] currentTrackSyncSamples = currentTrack.getSyncSamples();
                    if (currentTrackSyncSamples != null && currentTrackSyncSamples.Length > 0)
                    {
                        long[] currentTrackTimes = getTimes(currentTrack, movie);
                        times.Add(currentTrackTimes);
                    }
                }
            }
            return times;
        }

        private static long[] getTimes(Track track, Movie m)
        {
            long[] syncSamples = track.getSyncSamples();
            long[] syncSampleTimes = new long[syncSamples.Length];

            int currentSample = 1;  // first syncsample is 1
            long currentDuration = 0;
            int currentSyncSampleIndex = 0;

            long scalingFactor = calculateTracktimesScalingFactor(m, track);

            while (currentSample <= syncSamples[syncSamples.Length - 1])
            {
                if (currentSample == syncSamples[currentSyncSampleIndex])
                {
                    syncSampleTimes[currentSyncSampleIndex++] = currentDuration * scalingFactor;
                }
                currentDuration += track.getSampleDurations()[-1 + currentSample++];
            }
            return syncSampleTimes;
        }

        private static long calculateTracktimesScalingFactor(Movie m, Track track)
        {
            long timeScale = 1;
            foreach (Track track1 in m.getTracks())
            {
                if (track1.getHandler().Equals(track.getHandler()))
                {
                    if (track1.getTrackMetaData().getTimescale() != track.getTrackMetaData().getTimescale())
                    {
                        timeScale = Mp4Math.lcm(timeScale, track1.getTrackMetaData().getTimescale());
                    }
                }
            }
            return timeScale;
        }

        /**
         * Gets an array of sample numbers that are meant to be the first sample of each
         * chunk or fragment.
         *
         * @param track concerned track
         * @return an array containing the ordinal of each fragment's first sample
         */
        public long[] sampleNumbers(Track track)
        {

            if ("vide".Equals(track.getHandler()))
            {
                if (track.getSyncSamples() != null && track.getSyncSamples().Length > 0)
                {
                    List<long[]> times = getSyncSamplesTimestamps(movie, track);
                    return getCommonIndices(track.getSyncSamples(), getTimes(track, movie), track.getTrackMetaData().getTimescale(), times.ToArray());
                }
                else
                {
                    throw new Exception("Video Tracks need sync samples. Only tracks other than video may have no sync samples.");
                }
            }
            else if ("soun".Equals(track.getHandler()))
            {
                if (referenceTrack == null)
                {
                    foreach (Track candidate in movie.getTracks())
                    {
                        if (candidate.getSyncSamples() != null && "vide".Equals(candidate.getHandler()) && candidate.getSyncSamples().Length > 0)
                        {
                            referenceTrack = candidate;
                        }
                    }
                }
                if (referenceTrack != null)
                {

                    // Gets the reference track's fra
                    long[] refSyncSamples = sampleNumbers(referenceTrack);

                    int refSampleCount = referenceTrack.getSamples().Count;

                    long[] syncSamples = new long[refSyncSamples.Length];
                    long minSampleRate = 192000;
                    foreach (Track testTrack in movie.getTracks())
                    {
                        if (getFormat(track).Equals(getFormat(testTrack)))
                        {
                            AudioSampleEntry aase = null;
                            foreach (SampleEntry sampleEntry in testTrack.getSampleEntries())
                            {
                                if (aase == null)
                                {
                                    aase = (AudioSampleEntry)sampleEntry;
                                }
                                else if (aase.getSampleRate() != ((AudioSampleEntry)sampleEntry).getSampleRate())
                                {
                                    throw new Exception("Multiple SampleEntries and different sample rates is not supported");
                                }
                            }
                            Debug.Assert(aase != null);
                            if (aase.getSampleRate() < minSampleRate)
                            {
                                minSampleRate = aase.getSampleRate();
                                long sc = testTrack.getSamples().Count;
                                double stretch = (double)sc / refSampleCount;

                                long samplesPerFr = testTrack.getSampleDurations()[0]; // Assuming all audio tracks have the same number of samples per frame, which they do for all known types

                                for (int i = 0; i < syncSamples.Length; i++)
                                {
                                    long start = (long)Math.Ceiling(stretch * (refSyncSamples[i] - 1) * samplesPerFr);
                                    syncSamples[i] = start;
                                    // The Stretch makes sure that there are as much audio and video chunks!
                                }
                                break;
                            }
                        }
                    }
                    AudioSampleEntry ase = null;
                    foreach (SampleEntry sampleEntry in track.getSampleEntries())
                    {
                        if (ase == null)
                        {
                            ase = (AudioSampleEntry)sampleEntry;
                        }
                        else if (ase.getSampleRate() != ((AudioSampleEntry)sampleEntry).getSampleRate())
                        {
                            throw new Exception("Multiple SampleEntries and different sample rates is not supported");
                        }
                    }
                    Debug.Assert(ase != null);

                    long samplesPerFrame = track.getSampleDurations()[0]; // Assuming all audio tracks have the same number of samples per frame, which they do for all known types
                    double factor = (double)ase.getSampleRate() / (double)minSampleRate;
                    if (factor != Math.Round(factor))
                    { 
                        // Not an integer
                        throw new Exception("Sample rates must be a multiple of the lowest sample rate to create a correct file!");
                    }
                    for (int i = 0; i < syncSamples.Length; i++)
                    {
                        syncSamples[i] = (long)(1 + syncSamples[i] * factor / (double)samplesPerFrame);
                    }
                    return syncSamples;
                }
                throw new Exception("There was absolutely no Track with sync samples. I can't work with that!");
            }
            else
            {
                // Ok, my track has no sync samples - let's find one with sync samples.
                foreach (Track candidate in movie.getTracks())
                {
                    if (candidate.getSyncSamples() != null && candidate.getSyncSamples().Length > 0)
                    {
                        long[] refSyncSamples = sampleNumbers(candidate);
                        int refSampleCount = candidate.getSamples().Count;

                        long[] syncSamples = new long[refSyncSamples.Length];
                        long sc = track.getSamples().Count;
                        double stretch = (double)sc / refSampleCount;

                        for (int i = 0; i < syncSamples.Length; i++)
                        {
                            long start = (long)Math.Ceiling(stretch * (refSyncSamples[i] - 1)) + 1;
                            syncSamples[i] = start;
                            // The Stretch makes sure that there are as much audio and video chunks!
                        }
                        return syncSamples;
                    }
                }
                throw new Exception("There was absolutely no Track with sync samples. I can't work with that!");
            }
        }

        public long[] getCommonIndices(long[] syncSamples, long[] syncSampleTimes, long timeScale, params long[][] otherTracksTimes)
        {
            List<long> nuSyncSamples = new List<long>();
            List<long> nuSyncSampleTimes = new List<long>();


            for (int i = 0; i < syncSampleTimes.Length; i++)
            {
                bool foundInEveryRef = true;
                foreach(long[] times in otherTracksTimes)
                {
                    foundInEveryRef &= (Arrays.binarySearch(times, syncSampleTimes[i]) >= 0);
                }

                if (foundInEveryRef)
                {
                    // use sample only if found in every other track.
                    nuSyncSamples.Add(syncSamples[i]);
                    nuSyncSampleTimes.Add(syncSampleTimes[i]);
                }
            }
            // We have two arrays now:
            // nuSyncSamples: Contains all common sync samples
            // nuSyncSampleTimes: Contains the times of all sync samples

            // Start: Warn user if samples are not matching!
            if (nuSyncSamples.Count < (syncSamples.Length * 0.25))
            {
                string log = "";
                log += string.Format("{0} - Common:  [", nuSyncSamples.Count);
                foreach (long l in nuSyncSamples)
                {
                    log += (string.Format("{0},", l));
                }
                log += ("]");
                //LOG.warn(log);
                log = "";

                log += string.Format("{0} - In    :  [", syncSamples.Length);
                foreach (long l in syncSamples)
                {
                    log += (string.Format("{0},", l));
                }
                log += ("]");
                //LOG.warn(log);
                //LOG.warn("There are less than 25% of common sync samples in the given track.");
                throw new Exception("There are less than 25% of common sync samples in the given track.");
            }
            else if (nuSyncSamples.Count < (syncSamples.Length * 0.5))
            {
                //LOG.info("There are less than 50% of common sync samples in the given track. This is implausible but I'm ok to continue.");
            }
            else if (nuSyncSamples.Count < syncSamples.Length)
            {
                //LOG.trace("Common SyncSample positions vs. this tracks SyncSample positions: " + nuSyncSamples.size() + " vs. " + syncSamples.length);
            }
            // End: Warn user if samples are not matching!


            List<long> finalSampleList = new List<long>();

            if (minFragmentDurationSeconds > 0)
            {
                // if minFragmentDurationSeconds is greater 0
                // we need to throw away certain samples.
                long lastSyncSampleTime = -1;
                List<long>.Enumerator nuSyncSamplesIterator = nuSyncSamples.GetEnumerator();
                List<long>.Enumerator nuSyncSampleTimesIterator = nuSyncSampleTimes.GetEnumerator();
                do
                {
                    long curSyncSample = nuSyncSamplesIterator.Current;
                    long curSyncSampleTime = nuSyncSampleTimesIterator.Current;
                    if (lastSyncSampleTime == -1 || (curSyncSampleTime - lastSyncSampleTime) / timeScale >= minFragmentDurationSeconds)
                    {
                        finalSampleList.Add(curSyncSample);
                        lastSyncSampleTime = curSyncSampleTime;
                    }
                }
                while (nuSyncSamplesIterator.MoveNext() && nuSyncSampleTimesIterator.MoveNext());
            }
            else
            {
                // the list of all samples is the final list of samples
                // since minFragmentDurationSeconds ist not used.
                finalSampleList = nuSyncSamples;
            }


            // transform the list to an array
            long[] finalSampleArray = new long[finalSampleList.Count];
            for (int i = 0; i < finalSampleArray.Length; i++)
            {
                finalSampleArray[i] = finalSampleList[i];
            }
            return finalSampleArray;
        }
    }
}
