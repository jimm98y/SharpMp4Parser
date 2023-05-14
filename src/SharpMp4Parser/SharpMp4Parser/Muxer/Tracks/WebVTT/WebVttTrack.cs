using SharpMp4Parser.IsoParser;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part30;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using SharpMp4Parser.Muxer.Tracks.WebVTT.SampleBoxes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpMp4Parser.Muxer.Tracks.WebVTT
{
    public class WebVttTrack : AbstractTrack
    {
        private const string WEBVTT_FILE_HEADER_STRING = "^\uFEFF?WEBVTT((\\u0020|\u0009).*)?$";
        private static readonly Regex WEBVTT_FILE_HEADER =
                new Regex(WEBVTT_FILE_HEADER_STRING);
        private const string WEBVTT_METADATA_HEADER_STRING = "\\S*[:=]\\S*";
        private static readonly Regex WEBVTT_METADATA_HEADER =
                new Regex(WEBVTT_METADATA_HEADER_STRING);
        private const string WEBVTT_CUE_IDENTIFIER_STRING = "^(?!.*(-->)).*$";
        private static readonly Regex WEBVTT_CUE_IDENTIFIER =
                new Regex(WEBVTT_CUE_IDENTIFIER_STRING);
        private const string WEBVTT_TIMESTAMP_STRING = "(\\d+:)?[0-5]\\d:[0-5]\\d\\.\\d{3}";
        private static readonly Regex WEBVTT_TIMESTAMP = new Regex(WEBVTT_TIMESTAMP_STRING);
        private const string WEBVTT_CUE_SETTING_STRING = "\\S*:\\S*";
        private static readonly Regex WEBVTT_CUE_SETTING = new Regex(WEBVTT_CUE_SETTING_STRING);

        public class EmptySample : Sample
        {
            ByteBuffer vtte;

            public EmptySample()
            {
                VTTEmptyCueBox vttEmptyCueBox = new VTTEmptyCueBox();
                vtte = ByteBuffer.allocate(CastUtils.l2i(vttEmptyCueBox.getSize()));
                try
                {
                    vttEmptyCueBox.getBox(new ByteStream(new Java.ByteStream(vtte.array()))); // originally bytebufferchannel
                }
                catch (Exception)
                {
                    throw;
                }
                ((Java.Buffer)vtte).rewind();
            }


            public void writeTo(ByteStream channel)
            {
                channel.write(vtte.duplicate());
            }

            public long getSize()
            {
                return vtte.remaining();
            }

            public ByteBuffer asByteBuffer()
            {
                return (ByteBuffer)vtte.duplicate();
            }

            public SampleEntry getSampleEntry()
            {
                return sampleEntry;
            }
        }

        private readonly Sample EMPTY_SAMPLE = new EmptySample();
        TrackMetaData trackMetaData = new TrackMetaData();
        List<Sample> samples = new List<Sample>();
        long[] sampleDurations = new long[0];
        public static WebVTTSampleEntry sampleEntry;

        public WebVttTrack(ByteStream input, string trackName, CultureInfo locale) : base(trackName)
        {
            trackMetaData.setTimescale(1000);
            trackMetaData.setLanguage(locale.ThreeLetterISOLanguageName);
            long mediaTimestampUs = 0;


            sampleEntry = new WebVTTSampleEntry();
            WebVTTConfigurationBox webVttConf = new WebVTTConfigurationBox();
            sampleEntry.addBox(webVttConf);
            sampleEntry.addBox(new WebVTTSourceLabelBox());

            BufferedReader webvttData = new BufferedReader(new ByteStreamBaseReader(input, "UTF-8"));
            string line;

            // file should start with "WEBVTT"
            line = webvttData.readLine();
            if (line == null || !WEBVTT_FILE_HEADER.Match(line).Success)
            {
                throw new Exception("Expected WEBVTT. Got " + line);
            }
            webVttConf.setConfig(webVttConf.getConfig() + "\n" + line);
            while (true)
            {
                line = webvttData.readLine();
                if (line == null)
                {
                    // we reached EOF before finishing the header
                    throw new Exception("Expected an empty line after webvtt header");
                }
                else if (string.IsNullOrEmpty(line))
                {
                    // we've read the newline that separates the header from the body
                    break;
                }

                Match matcher = WEBVTT_METADATA_HEADER.Match(line);
                if (!matcher.Success)
                {
                    throw new Exception("Expected WebVTT metadata header. Got " + line);
                }
                webVttConf.setConfig(webVttConf.getConfig() + "\n" + line);
            }


            // process the cues and text
            while ((line = webvttData.readLine()) != null)
            {
                if ("".Equals(line.Trim()))
                {
                    continue;
                }
                // parse the cue identifier (if present) {
                Match matcher = WEBVTT_CUE_IDENTIFIER.Match(line);
                if (matcher.Success)
                {
                    // ignore the identifier (we currently don't use it) and read the next line
                    line = webvttData.readLine();
                }

                long startTime;
                long endTime;

                // parse the cue timestamps
                matcher = WEBVTT_TIMESTAMP.Match(line);

                // parse start timestamp
                if (!matcher.Success)
                {
                    throw new Exception("Expected cue start time: " + line);
                }
                else
                {
                    startTime = parseTimestampUs(matcher.Groups[0].Value);
                }

                // parse end timestamp
                string endTimeString;
                if (!matcher.NextMatch().Success)
                {
                    throw new Exception("Expected cue end time: " + line);
                }
                else
                {
                    endTimeString = matcher.Groups[0].Value;
                    endTime = parseTimestampUs(endTimeString);
                }

                // parse the (optional) cue setting list
                line = line.Substring(line.IndexOf(endTimeString) + endTimeString.Length);
                matcher = WEBVTT_CUE_SETTING.Match(line);
                string settings = null;
                while (matcher.Success)
                {
                    settings = matcher.Groups[0].Value;
                }
                StringBuilder payload = new StringBuilder();
                while (((line = webvttData.readLine()) != null) && (!string.IsNullOrEmpty(line)))
                {
                    if (payload.Length > 0)
                    {
                        payload.Append("\n");
                    }
                    payload.Append(line.Trim());
                }

                if (startTime != mediaTimestampUs)
                {
                    //System.err.println("" + mediaTimestampUs + " - " + startTime + " Add empty sample");
                    sampleDurations = Mp4Arrays.copyOfAndAppend(sampleDurations, startTime - mediaTimestampUs);
                    samples.Add(EMPTY_SAMPLE);
                }
                sampleDurations = Mp4Arrays.copyOfAndAppend(sampleDurations, endTime - startTime);
                VTTCueBox vttCueBox = new VTTCueBox();
                if (settings != null)
                {
                    CueSettingsBox csb = new CueSettingsBox();
                    csb.setContent(settings);
                    vttCueBox.setCueSettingsBox(csb);
                }
                CuePayloadBox cuePayloadBox = new CuePayloadBox();
                cuePayloadBox.setContent(payload.ToString());
                vttCueBox.setCuePayloadBox(cuePayloadBox);

                samples.Add(new BoxBearingSample(new List<Box>() { vttCueBox }));


                mediaTimestampUs = endTime;
                // samples.add();
            }
        }

        private static long parseTimestampUs(string s)
        {
            Regex regex = new Regex(WEBVTT_TIMESTAMP_STRING);
            if (!regex.Match(s).Success)
            {
                throw new FormatException("has invalid format");
            }

            Regex r = new Regex("\\.");
            string[] parts = r.Split(s, 2);
            long value = 0;
            foreach (string group in parts[0].Split(':'))
            {
                value = value * 60 + long.Parse(group);
            }
            return (value * 1000 + long.Parse(parts[1]));
        }

        public override List<SampleEntry> getSampleEntries()
        {
            return new List<SampleEntry>() { sampleEntry };
        }

        public override long[] getSampleDurations()
        {
            long[] adoptedSampleDuration = new long[sampleDurations.Length];
            for (int i = 0; i < adoptedSampleDuration.Length; i++)
            {
                adoptedSampleDuration[i] = sampleDurations[i] * trackMetaData.getTimescale() / 1000;
            }
            return adoptedSampleDuration;
        }

        public override TrackMetaData getTrackMetaData()
        {
            return trackMetaData;
        }

        public override string getHandler()
        {
            return "text";
        }

        public override IList<Sample> getSamples()
        {
            return samples;
        }

        public override void close()
        {

        }

        private class BoxBearingSample : Sample
        {
            List<Box> boxes;

            public BoxBearingSample(List<Box> boxes)
            {
                this.boxes = boxes;
            }

            public void writeTo(ByteStream channel)
            {
                foreach (Box box in boxes)
                {
                    box.getBox(channel);
                }
            }

            public long getSize()
            {
                long l = 0;
                foreach (Box box in boxes)
                {
                    l += box.getSize();
                }
                return l;
            }

            public ByteBuffer asByteBuffer()
            {
                ByteStream baos = new ByteStream();
                try
                {
                    writeTo(Channels.newChannel(baos));
                }
                catch (Exception)
                {
                    throw;
                }

                return ByteBuffer.wrap(baos.toByteArray());
            }

            public SampleEntry getSampleEntry()
            {
                return WebVttTrack.sampleEntry;
            }
        }
    }
}