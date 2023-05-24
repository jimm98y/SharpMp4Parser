#if REMOVED
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part30;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace SharpMp4Parser.Muxer.Tracks.TTML
{
    public class TtmlTrackImpl : AbstractTrack
    {
        TrackMetaData trackMetaData = new TrackMetaData();
        XMLSubtitleSampleEntry xmlSubtitleSampleEntry = new XMLSubtitleSampleEntry();

        List<Sample> samples = new List<Sample>();
        SubSampleInformationBox subSampleInformationBox = new SubSampleInformationBox();


        private long[] sampleDurations;


        public class TtmlSample : Sample
        {
            private byte[] finalSample;
            private XMLSubtitleSampleEntry xmlSubtitleSampleEntry;

            public TtmlSample(byte[] finalSample)
            {
                this.finalSample = finalSample;
            }

            public TtmlSample(byte[] finalSample, XMLSubtitleSampleEntry xmlSubtitleSampleEntry) : this(finalSample)
            {
                this.xmlSubtitleSampleEntry = xmlSubtitleSampleEntry;
            }

            public void writeTo(ByteStream channel)
            {
                channel.write(ByteBuffer.wrap(finalSample));
            }
            public long getSize()
            {
                return finalSample.Length;
            }

            public ByteBuffer asByteBuffer()
            {
                return ByteBuffer.wrap(finalSample);
            }

            public SampleEntry getSampleEntry()
            {
                return xmlSubtitleSampleEntry;
            }
        }

        public TtmlTrackImpl(string name, List<XPathDocument> ttmls) : base(name)
        {
            extractLanguage(ttmls);
            List<string> mimeTypes = new List<string>();
            sampleDurations = new long[ttmls.Count];
            XPathFactory xPathfactory = XPathFactory.newInstance();
            XPath xpath = xPathfactory.newXPath();
            xpath.setNamespaceContext(TtmlHelpers.NAMESPACE_CONTEXT);

            for (int sampleNo = 0; sampleNo < ttmls.Count; sampleNo++)
            {
                XPathDocument ttml = ttmls[sampleNo];
                SubSampleInformationBox.SubSampleEntry subSampleEntry = new SubSampleInformationBox.SubSampleEntry();
                subSampleInformationBox.getEntries().Add(subSampleEntry);
                subSampleEntry.setSampleDelta(1);
                sampleDurations[sampleNo] = extractDuration(ttml);

                List<byte[]> images = extractImages(ttml);
                mimeTypes.AddRange(extractMimeTypes(ttml));

                // No changes of XML after this point!
                ByteStream baos = new ByteStream();
                TtmlHelpers.pretty(ttml, baos, 4);
                SubSampleInformationBox.SubSampleEntry.SubsampleEntry xmlEntry =
                        new SubSampleInformationBox.SubSampleEntry.SubsampleEntry();
                xmlEntry.setSubsampleSize(baos.Length);

                subSampleEntry.getSubsampleEntries().Add(xmlEntry);
                foreach (byte[] image in images)
                {
                    baos.write(image);
                    SubSampleInformationBox.SubSampleEntry.SubsampleEntry imageEntry =
                            new SubSampleInformationBox.SubSampleEntry.SubsampleEntry();
                    imageEntry.setSubsampleSize(image.Length);
                    subSampleEntry.getSubsampleEntries().Add(imageEntry);
                }

                byte[] finalSample = baos.toByteArray();
                samples.Add(new TtmlSample(finalSample, xmlSubtitleSampleEntry));
            }


            xmlSubtitleSampleEntry.setNamespace(join(",", getAllNamespaces(ttmls[0])));
            xmlSubtitleSampleEntry.setSchemaLocation("");
            xmlSubtitleSampleEntry.setAuxiliaryMimeTypes(join(",", new List<string>(mimeTypes).ToArray()));
            trackMetaData.setTimescale(30000);
            trackMetaData.setLayer(65535);


        }

        public static string getLanguage(XPathDocument document)
        {
            return document.getDocumentElement().getAttribute("xml:lang");
        }

        protected static List<byte[]> extractImages(XPathDocument ttml)
        {
            XPathFactory xPathfactory = XPathFactory.newInstance();
            XPath xpath = xPathfactory.newXPath();
            XPathExpression expr = xpath.compile("//*/@backgroundImage");
            NodeList nl = (NodeList)expr.evaluate(ttml, XPathConstants.NODESET);

            Dictionary<string, string> internalNames2Original = new Dictionary<string, string>();

            int p = 1;
            for (int i = 0; i < nl.getLength(); i++)
            {
                Node bgImageNode = nl.item(i);
                string uri = bgImageNode.getNodeValue();
                string ext = uri.Substring(uri.LastIndexOf("."));

                string internalName = internalNames2Original[uri];
                if (internalName == null)
                {
                    internalName = "urn:mp4parser:" + p++ + ext;
                    internalNames2Original.Add(internalName, uri);
                }
                bgImageNode.setNodeValue(internalName);

            }
            List<byte[]> images = new List<byte[]>();
            if (internalNames2Original.Count != 0)
            {
                foreach (var internalName2Original in internalNames2Original)
                {

                    Uri pic = new Uri(ttml.getDocumentURI()).resolve(internalName2Original.Value);
                    images.Add(streamToByteArray(pic.toURL().openStream()));

                }
            }
            return images;
        }

        private static string join(string joiner, string[] i)
        {
            StringBuilder result = new StringBuilder();
            foreach (string s in i)
            {
                result.Append(s).Append(joiner);
            }
            result.setLength(result.Length > 0 ? result.Length - 1 : 0);
            return result.ToString();
        }

        private static long latestTimestamp(XPathDocument document)
        {
            XPathFactory xPathfactory = XPathFactory.newInstance();
            XPath xpath = xPathfactory.newXPath();
            xpath.setNamespaceContext(TtmlHelpers.NAMESPACE_CONTEXT);

            try
            {
                XPathExpression xp = xpath.compile("//*[name()='p']");
                NodeList timedNodes = (NodeList)xp.evaluate(document, XPathConstants.NODESET);
                long lastTimeStamp = 0;
                for (int i = 0; i < timedNodes.getLength(); i++)
                {
                    lastTimeStamp = Math.Max(getEndTime(timedNodes.item(i)), lastTimeStamp);
                }
                return lastTimeStamp;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static byte[] streamToByteArray(ByteStream input)
        {
            byte[]
        buffer = new byte[8096];
            ByteStream output = new ByteStream();

            int n = 0;
            while (-1 != (n = input.read(buffer)))
            {
                output.write(buffer, 0, n);
            }
            return output.toByteArray();
        }

        protected long firstTimestamp(XPathDocument document)
        {
            XPathFactory xPathfactory = XPathFactory.newInstance();
            XPath xpath = xPathfactory.newXPath();
            xpath.setNamespaceContext(TtmlHelpers.NAMESPACE_CONTEXT);

            try
            {
                XPathExpression xp = xpath.compile("//*[@begin]");
                NodeList timedNodes = (NodeList)xp.evaluate(document, XPathConstants.NODESET);

                long firstTimestamp = long.MaxValue;
                for (int i = 0; i < timedNodes.getLength(); i++)
                {
                    firstTimestamp = Math.Min(getStartTime(timedNodes.item(i)), firstTimestamp);
                }
                return firstTimestamp;
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected long lastTimestamp(XPathDocument document)
        {
            XPathFactory xPathfactory = XPathFactory.newInstance();
            XPath xpath = xPathfactory.newXPath();
            xpath.setNamespaceContext(TtmlHelpers.NAMESPACE_CONTEXT);

            try
            {
                XPathExpression xp = xpath.compile("//*[@end]");
                NodeList timedNodes = (NodeList)xp.evaluate(document, XPathConstants.NODESET);

                long lastTimeStamp = 0;
                for (int i = 0; i < timedNodes.getLength(); i++)
                {
                    lastTimeStamp = Math.Max(getEndTime(timedNodes.item(i)), lastTimeStamp);
                }
                return lastTimeStamp;
            }
            catch (Exception)
            {
                throw;
            }

        }

        protected void extractLanguage(List<XPathDocument> ttmls)
        {
            string firstLang = null;
            foreach (XPathDocument ttml in ttmls)
            {

                string lang = getLanguage(ttml);
                if (firstLang == null)
                {
                    firstLang = lang;
                    trackMetaData.setLanguage(Locale.forLanguageTag(lang).getISO3Language());
                }
                else if (!firstLang.Equals(lang))
                {
                    throw new Exception("Within one Track all sample documents need to have the same language");
                }

            }
        }

        protected List<string> extractMimeTypes(XPathDocument ttml)
        {
            XPathFactory xPathfactory = XPathFactory.newInstance();

            XPath xpath = xPathfactory.newXPath();

            XPathExpression expr = xpath.compile("//*/@smpte:backgroundImage");
            NodeList nl = (NodeList)expr.evaluate(ttml, XPathConstants.NODESET);

            HashSet<string> mimeTypes = new HashSet<string>();

            int p = 1;
            for (int i = 0; i < nl.getLength(); i++)
            {
                Node bgImageNode = nl.item(i);
                string uri = bgImageNode.getNodeValue();
                string ext = uri.Substring(uri.LastIndexOf("."));
                if (ext.Contains("jpg") || ext.Contains("jpeg"))
                {
                    mimeTypes.Add("image/jpeg");
                }
                else if (ext.Contains("png"))
                {
                    mimeTypes.Add("image/png");
                }
            }
            return new List<string>(mimeTypes);
        }

        long extractDuration(XPathDocument ttml)
        {
            return lastTimestamp(ttml) - firstTimestamp(ttml);
        }

        public override List<SampleEntry> getSampleEntries()
        {
            return new List<SampleEntry>() { xmlSubtitleSampleEntry };
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
            return "subt";
        }

        public override IList<Sample> getSamples()
        {
            return samples;
        }

        public override SubSampleInformationBox getSubsampleInformationBox()
        {
            return subSampleInformationBox;

        }

        public override void close()
        {

        }
    }
}
#endif