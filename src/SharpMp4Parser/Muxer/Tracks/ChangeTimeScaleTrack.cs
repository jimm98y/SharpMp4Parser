using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Boxes.SampleGrouping;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Muxer.Tracks
{
    /**
     * Changes the timescale of a track by wrapping the track.
     */
    public class ChangeTimeScaleTrack : Track
    {
        //private static Logger LOG = LoggerFactory.getLogger(ChangeTimeScaleTrack.class.getName());

        Track source;
        List<CompositionTimeToSample.Entry> ctts;
        long[] decodingTimes;
        long timeScale;

        /**
         * Changes the time scale of the source track to the target time scale and makes sure
         * that any rounding errors that may have summed are corrected exactly before the syncSamples.
         *
         * @param source          the source track
         * @param targetTimeScale the resulting time scale of this track.
         * @param syncSamples     at these sync points where rounding error are corrected.
         */
        public ChangeTimeScaleTrack(Track source, long targetTimeScale, long[] syncSamples)
        {
            this.source = source;
            this.timeScale = targetTimeScale;
            double timeScaleFactor = (double)targetTimeScale / source.getTrackMetaData().getTimescale();
            ctts = adjustCtts(source.getCompositionTimeEntries(), timeScaleFactor);
            decodingTimes = adjustTts(source.getSampleDurations(), timeScaleFactor, syncSamples, getTimes(source, syncSamples, targetTimeScale));
        }

        private static long[] getTimes(Track track, long[] syncSamples, long targetTimeScale)
        {
            long[] syncSampleTimes = new long[syncSamples.Length];

            int currentSample = 1;  // first syncsample is 1
            long currentDuration = 0;
            int currentSyncSampleIndex = 0;


            while (currentSample <= syncSamples[syncSamples.Length - 1])
            {
                if (currentSample == syncSamples[currentSyncSampleIndex])
                {
                    syncSampleTimes[currentSyncSampleIndex++] = (currentDuration * targetTimeScale) / track.getTrackMetaData().getTimescale();
                }
                currentDuration += track.getSampleDurations()[currentSample - 1];
                currentSample++;
            }
            return syncSampleTimes;

        }

        /**
         * Adjusting the composition times is easy. Just scale it by the factor - that's it. There is no rounding
         * error summing up.
         *
         * @param source
         * @param timeScaleFactor
         * @return
         */
        static List<CompositionTimeToSample.Entry> adjustCtts(List<CompositionTimeToSample.Entry> source, double timeScaleFactor)
        {
            if (source != null)
            {
                List<CompositionTimeToSample.Entry> entries2 = new List<CompositionTimeToSample.Entry>(source.Count);
                foreach (CompositionTimeToSample.Entry entry in source)
                {
                    entries2.Add(new CompositionTimeToSample.Entry(entry.getCount(), (int)Math.Round(timeScaleFactor * entry.getOffset())));
                }
                return entries2;
            }
            else
            {
                return null;
            }
        }

        static long[] adjustTts(long[] sourceArray, double timeScaleFactor, long[] syncSample, long[] syncSampleTimes)
        {
            long summedDurations = 0;

            long[] scaledArray = new long[sourceArray.Length];

            for (int i = 1; i <= sourceArray.Length; i++)
            {
                long duration = sourceArray[i - 1];

                long x = (long)Math.Round(timeScaleFactor * duration);

                int ssIndex;
                if ((ssIndex = Arrays.binarySearch(syncSample, i + 1)) >= 0)
                {
                    // we are at the sample before sync point
                    if (syncSampleTimes[ssIndex] != summedDurations)
                    {
                        long correction = syncSampleTimes[ssIndex] - (summedDurations + x);
                        Java.LOG.debug(String.Format("Sample {0} {1} / {2} - correct by {3}", i, summedDurations, syncSampleTimes[ssIndex], correction));
                        x += correction;
                    }
                }
                summedDurations += x;
                scaledArray[i - 1] = x;
            }
            return scaledArray;
        }

        public void close()
        {
            source.close();
        }

        public List<SampleEntry> getSampleEntries()
        {
            return source.getSampleEntries();
        }

        public long[] getSampleDurations()
        {
            return decodingTimes;
        }

        public List<CompositionTimeToSample.Entry> getCompositionTimeEntries()
        {
            return ctts;
        }

        public long[] getSyncSamples()
        {
            return source.getSyncSamples();
        }

        public List<SampleDependencyTypeBox.Entry> getSampleDependencies()
        {
            return source.getSampleDependencies();
        }

        public TrackMetaData getTrackMetaData()
        {
            TrackMetaData trackMetaData = (TrackMetaData)source.getTrackMetaData().Clone();
            trackMetaData.setTimescale(timeScale);
            return trackMetaData;
        }

        public string getHandler()
        {
            return source.getHandler();
        }

        public IList<Sample> getSamples()
        {
            return source.getSamples();
        }

        public SubSampleInformationBox getSubsampleInformationBox()
        {
            return source.getSubsampleInformationBox();
        }

        public long getDuration()
        {
            long duration = 0;
            foreach (long delta in decodingTimes)
            {
                duration += delta;
            }
            return duration;
        }

        public override string ToString()
        {
            return "ChangeTimeScaleTrack{" +
                    "source=" + source +
                    '}';
        }

        public string getName()
        {
            return "timeScale(" + source.getName() + ")";
        }

        public List<Edit> getEdits()
        {
            return source.getEdits();
        }

        public Dictionary<GroupEntry, long[]> getSampleGroups()
        {
            return source.getSampleGroups();
        }
    }
}