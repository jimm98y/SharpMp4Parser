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
using SharpMp4Parser.IsoParser.Boxes.ThreeGPP.TS26245;
using SharpMp4Parser.Java;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

namespace SharpMp4Parser.Muxer.Tracks
{
    /**
     *
     */
    public class TextTrackImpl : AbstractTrack
    {
        TrackMetaData trackMetaData = new TrackMetaData();
        TextSampleEntry tx3g;
        List<Line> subs = new List<Line>();

        List<Sample> samples;

        public TextTrackImpl() : base("subtitles")
        {
            tx3g = new TextSampleEntry("tx3g");
            tx3g.setDataReferenceIndex(1);
            tx3g.setStyleRecord(new TextSampleEntry.StyleRecord());
            tx3g.setBoxRecord(new TextSampleEntry.BoxRecord());

            FontTableBox ftab = new FontTableBox();
            ftab.setEntries(new List<FontTableBox.FontRecord>() { (new FontTableBox.FontRecord(1, "Serif")) });

            tx3g.addBox(ftab);


            trackMetaData.setCreationTime(new DateTime());
            trackMetaData.setModificationTime(new DateTime());
            trackMetaData.setTimescale(1000); // Text tracks use millieseconds
        }

        public List<Line> getSubs()
        {
            return subs;
        }

        public override void close()
        {
            // nothing to close
        }

        private readonly object _syncRoot = new object();

        public override List<Sample> getSamples()
        {
            lock (_syncRoot)
            {
                if (samples == null)
                {
                    samples = new List<Sample>();
                    long lastEnd = 0;
                    foreach (Line sub in subs)
                    {
                        long silentTime = sub.from - lastEnd;
                        if (silentTime > 0)
                        {
                            samples.Add(new SampleImpl(ByteBuffer.wrap(new byte[] { 0, 0 }), tx3g));
                        }
                        else if (silentTime < 0)
                        {
                            throw new Exception("Subtitle display times may not intersect");
                        }
                        ByteArrayOutputStream baos = new ByteArrayOutputStream();
                        DataOutputStream dos = new DataOutputStream(baos);
                        try
                        {
                            dos.writeShort(Encoding.UTF8.GetBytes(sub.text).Length);
                            dos.write(Encoding.UTF8.GetBytes(sub.text));
                            dos.close();
                        }
                        catch (IOException)
                        {
                            throw new Exception("VM is broken. Does not support UTF-8");
                        }
                        samples.Add(new SampleImpl(ByteBuffer.wrap(baos.toByteArray()), tx3g));
                        lastEnd = sub.to;
                    }
                }
                return samples;
            }
        }

        public override List<SampleEntry> getSampleEntries()
        {
            return new List<SampleEntry>() { tx3g };
        }

        public override long[] getSampleDurations()
        {
            List<long> decTimes = new List<long>();

            long lastEnd = 0;
            foreach (Line sub in subs)
            {
                long silentTime = sub.from - lastEnd;
                if (silentTime > 0)
                {

                    decTimes.Add(silentTime);
                }
                else if (silentTime < 0)
                {
                    throw new Exception("Subtitle display times may not intersect");
                }
                decTimes.Add(sub.to - sub.from);
                lastEnd = sub.to;
            }
            long[] decTimesArray = new long[decTimes.Count];
            int index = 0;
            foreach (long decTime in decTimes)
            {
                decTimesArray[index++] = decTime;
            }
            return decTimesArray;
        }

        public override List<CompositionTimeToSample.Entry> getCompositionTimeEntries()
        {
            return null;
        }

        public override long[] getSyncSamples()
        {
            return null;
        }

        public override List<SampleDependencyTypeBox.Entry> getSampleDependencies()
        {
            return null;
        }

        public override TrackMetaData getTrackMetaData()
        {
            return trackMetaData;
        }

        public override string getHandler()
        {
            return "sbtl";
        }

        public override SubSampleInformationBox getSubsampleInformationBox()
        {
            return null;
        }

        public class Line
        {
            public long from;
            public long to;
            public string text;


            public Line(long from, long to, string text)
            {
                this.from = from;
                this.to = to;
                this.text = text;
            }

            public long getFrom()
            {
                return from;
            }

            public string getText()
            {
                return text;
            }

            public long getTo()
            {
                return to;
            }
        }
    }
}