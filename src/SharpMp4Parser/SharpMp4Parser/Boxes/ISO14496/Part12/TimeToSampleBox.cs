/*  
 * Copyright 2008 CoreMedia AG, Hamburg
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

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * This box contains a compact version of a table that allows indexing from decoding time to sample number.
     * Other tables give sample sizes and pointers, from the sample number. Each entry in the table gives the
     * number of consecutive samples with the same time delta, and the delta of those samples. By adding the
     * deltas a complete time-to-sample map may be built.<br>
     * The Decoding Time to Sample Box contains decode time delta's: <code>DT(n+1) = DT(n) + STTS(n)</code> where STTS(n)
     * is the (uncompressed) table entry for sample n.<br>
     * The sample entries are ordered by decoding time stamps; therefore the deltas are all non-negative. <br>
     * The DT axis has a zero origin; <code>DT(i) = SUM(for j=0 to i-1 of delta(j))</code>, and the sum of all
     * deltas gives the length of the media in the track (not mapped to the overall timescale, and not considering
     * any edit list).    <br>
     * The Edit List Box provides the initial CT value if it is non-empty (non-zero).
     */
    public class TimeToSampleBox : AbstractFullBox
    {
        public const string TYPE = "stts";
        static Dictionary<List<Entry>, WeakReference<long[]>> cache = new Dictionary<List<Entry>, WeakReference<long[]>>();
        List<Entry> entries = new List<Entry>();

        private static object _syncRoot = new object();

        public TimeToSampleBox() : base(TYPE)
        { }

        /**
         * Decompresses the list of entries and returns the list of decoding times.
         *
         * @param entries compressed entries
         * @return decoding time per sample
         */
        public static long[] blowupTimeToSamples(List<TimeToSampleBox.Entry> entries)
        {
            lock (_syncRoot)
            {
                WeakReference<long[]> cacheEntry;
                if ((cacheEntry = cache[entries]) != null)
                {
                    long[] cacheVal;
                    if (cacheEntry.TryGetTarget(out cacheVal))
                    {
                        return cacheVal;
                    }
                }
                long numOfSamples = 0;
                foreach (TimeToSampleBox.Entry entry in entries)
                {
                    numOfSamples += entry.getCount();
                }
                Debug.Assert(numOfSamples <= int.MaxValue);
                long[] decodingTime = new long[(int)numOfSamples];

                int current = 0;


                foreach (TimeToSampleBox.Entry entry in entries)
                {
                    for (int i = 0; i < entry.getCount(); i++)
                    {
                        decodingTime[current++] = entry.getDelta();
                    }
                }
                cache.Add(entries, new WeakReference<long[]>(decodingTime));
                return decodingTime;
            }
        }

        protected long getContentSize()
        {
            return 8 + entries.size() * 8;
        }

        public override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            int entryCount = CastUtils.l2i(IsoTypeReader.readUInt32(content));
            entries = new List<Entry>(entryCount);

            for (int i = 0; i < entryCount; i++)
            {
                entries.Add(new Entry(IsoTypeReader.readUInt32(content), IsoTypeReader.readUInt32(content)));
            }

        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt32(byteBuffer, entries.size());
            foreach (Entry entry in entries)
            {
                IsoTypeWriter.writeUInt32(byteBuffer, entry.getCount());
                IsoTypeWriter.writeUInt32(byteBuffer, entry.getDelta());
            }
        }

        public List<Entry> getEntries()
        {
            return entries;
        }

        public void setEntries(List<Entry> entries)
        {
            this.entries = entries;
        }

        public string toString()
        {
            return "TimeToSampleBox[entryCount=" + entries.size() + "]";
        }

        public sealed class Entry
        {
            long count;
            long delta;

            public Entry(long count, long delta)
            {
                this.count = count;
                this.delta = delta;
            }

            public long getCount()
            {
                return count;
            }

            public void setCount(long count)
            {
                this.count = count;
            }

            public long getDelta()
            {
                return delta;
            }

            public void setDelta(long delta)
            {
                this.delta = delta;
            }

            public override string ToString()
            {
                return "Entry{" +
                        "count=" + count +
                        ", delta=" + delta +
                        '}';
            }
        }
    }
}
