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

using SharpMp4Parser.IsoParser.Boxes.Adobe;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.Java;
using System.Collections.Generic;
using System;
using System.Linq;
using static SharpMp4Parser.Muxer.Tracks.AC3TrackImpl;

namespace SharpMp4Parser.Muxer.Tracks
{
    public class Amf0Track : AbstractTrack
    {
        private SortedDictionary<long, byte[]> rawSamples = new SortedDictionary<long, byte[]>();
        private TrackMetaData trackMetaData = new TrackMetaData();
        private ActionMessageFormat0SampleEntryBox amf0;

        /**
         * Creates a new AMF0 track from
         *
         * @param rawSamples raw samples of the track
         */
        public Amf0Track(Dictionary<long, byte[]> rawSamples) : base("amf0")
        {

            this.rawSamples = new SortedDictionary<long, byte[]>(rawSamples);
            trackMetaData.setCreationTime(new DateTime());
            trackMetaData.setModificationTime(new DateTime());
            trackMetaData.setTimescale(1000); // Text tracks use millieseconds
            trackMetaData.setLanguage("eng");

            amf0 = new ActionMessageFormat0SampleEntryBox();
            amf0.setDataReferenceIndex(1);
        }

        public override IList<Sample> getSamples()
        {
            List<Sample> samples = new List<Sample>();
            foreach (byte[] bytes in rawSamples.Values)
            {
                samples.Add(new SampleImpl(ByteBuffer.wrap(bytes), amf0));
            }
            return samples;
        }

        public override void close()
        {
            // no resources involved - doing nothing
        }

        public override List<SampleEntry> getSampleEntries()
        {
            return new List<SampleEntry>() { amf0 };
        }

        public override long[] getSampleDurations()
        {
            List<long> keys = new List<long>(rawSamples.Keys);
            keys = keys.OrderBy(x => x).ToList();
            long[] rc = new long[keys.Count];
            long lastTimeStamp = 0;
            for (int i = 0; i < keys.Count; i++)
            {
                long key = keys[i];
                long delta = key - lastTimeStamp;
                rc[i] = delta;
            }
            return rc;
        }

        public override List<CompositionTimeToSample.Entry> getCompositionTimeEntries()
        {
            // AMF0 tracks do not have Composition Time
            return null;
        }

        public override long[] getSyncSamples()
        {
            // AMF0 tracks do not have Sync Samples
            return null;
        }

        public override List<SampleDependencyTypeBox.Entry> getSampleDependencies()
        {
            // AMF0 tracks do not have Sample Dependencies
            return null;
        }

        public override TrackMetaData getTrackMetaData()
        {
            return trackMetaData;  //To change body of implemented methods use File | Settings | File Templates.
        }

        public override string getHandler()
        {
            return "data";
        }

        public override SubSampleInformationBox getSubsampleInformationBox()
        {
            return null;
        }
    }
}