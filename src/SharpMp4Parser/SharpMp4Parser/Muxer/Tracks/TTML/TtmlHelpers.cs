#if REMOVED

using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;
using System.IO;

namespace SharpMp4Parser.Muxer.Tracks.TTML
{
    public class TtmlHelpers
    {
        public const string SMPTE_TT_NAMESPACE = "http://www.smpte-ra.org/schemas/2052-1/2010/smpte-tt";
        public const string TTML_NAMESPACE = "http://www.w3.org/ns/ttml";
        public static readonly NamespaceContext NAMESPACE_CONTEXT = new TextTrackNamespaceContext();
        static byte[] namespacesStyleSheet1 = ("<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                "    <xsl:output method=\"text\"/>\n" +
                "    <xsl:key name=\"kElemByNSURI\"\n" +
                "             match=\"*[namespace::*[not(. = ../../namespace::*)]]\"\n" +
                "              use=\"namespace::*[not(. = ../../namespace::*)]\"/>\n" +
                "    <xsl:template match=\"/\">\n" +
                "        <xsl:for-each select=\n" +
                "            \"//namespace::*[not(. = ../../namespace::*)]\n" +
                "                           [count(..|key('kElemByNSURI',.)[1])=1]\">\n" +
                "            <xsl:value-of select=\"concat(.,'&#xA;')\"/>\n" +
                "        </xsl:for-each>\n" +
                "    </xsl:template>\n" +
                "</xsl:stylesheet>").getBytes();

        //public static void main(string[] args)
        //{
        //    DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
        //    dbf.setNamespaceAware(true);
        //    DocumentBuilder builder = dbf.newDocumentBuilder();
        //    Document doc = builder.parse("C:\\dev\\mp4parser\\a.xml");
        //    List<Document> split = TtmlSegmenter.split(doc, 60);
        //    /*        for (Document document : split) {
        //                pretty(document, System.out, 4);
        //            }*/
        //    Track t = new TtmlTrackImpl("a.xml", split);
        //    Movie m = new Movie();
        //    m.addTrack(t);
        //    Container c = new DefaultMp4Builder().build(m);
        //    c.writeContainer(new FileByteStreamBase("output.mp4").getChannel());
        //}

        public static string[] getAllNamespaces(Document doc)
        {
            TransformerFactory tf = TransformerFactory.newInstance();
            Transformer transformer;
            try
            {
                transformer = tf.newTransformer(new StreamSource(new ByteStreamBase(namespacesStyleSheet1)));
                StringWriter sw = new StringWriter();
                transformer.transform(new DOMSource(doc), new StreamResult(sw));
                List<string> r = new List<string>(new List<string>(Arrays.asList(sw.getBuffer().ToString().Split("\n"))));
                return r.toArray(new string[r.Count]);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string toTimeExpression(long ms)
        {
            return toTimeExpression(ms, -1);
        }

        public static string toTimeExpression(long ms, int frames)
        {
            string minus = ms >= 0 ? "" : "-";
            ms = Math.Abs(ms);

            long hours = ms / 1000 / 60 / 60;
            ms -= hours * 1000 * 60 * 60;

            long minutes = ms / 1000 / 60;
            ms -= minutes * 1000 * 60;

            long seconds = ms / 1000;
            ms -= seconds * 1000;
            if (frames >= 0)
            {
                return string.Format("{0}{1:00}:{2:00}:{3:00}:{4}", minus, hours, minutes, seconds, frames);
            }
            else
            {
                return string.Format("{0}{1:00}:{2:00}:{3:00}.{4:000}", minus, hours, minutes, seconds, ms);
            }
        }

        public static long toTime(string expr)
        {
            Pattern p = Pattern.compile("(-?)([0-9][0-9]):([0-9][0-9]):([0-9][0-9])([\\.:][0-9][0-9]?[0-9]?)?");
            Matcher m = p.matcher(expr);
            if (m.matches())
            {
                string minus = m.group(1);
                string hours = m.group(2);
                string minutes = m.group(3);
                string seconds = m.group(4);
                string fraction = m.group(5);
                if (fraction == null)
                {
                    fraction = ".000";
                }

                fraction = fraction.Replace(":", ".");
                long ms = long.Parse(hours) * 60 * 60 * 1000;
                ms += long.Parse(minutes) * 60 * 1000;
                ms += long.Parse(seconds) * 1000;
                if (fraction.Contains(":"))
                {
                    ms += (long)Double.Parse("0" + fraction.Replace(":", ".")) * 40 * 1000; // 40ms == 25fps - simplifying assumption should be ok for here
                }
                else
                {
                    ms += (long)Double.Parse("0" + fraction) * 1000;
                }

                return ms * ("-".Equals(minus) ? -1 : 1);
            }
            else
            {
                throw new Exception("Cannot match '" + expr + "' to time expression");
            }
        }

        public static void pretty(Document document, ByteStreamBase ByteStreamBase, int indent)
        {
            TransformerFactory transformerFactory = TransformerFactory.newInstance();
            Transformer transformer = null;
            try
            {
                transformer = transformerFactory.newTransformer();
            }
            catch (Exception)
            {
                throw;
            }
            transformer.setOutputProperty(OutputKeys.ENCODING, "UTF-8");
            if (indent > 0)
            {
                transformer.setOutputProperty(OutputKeys.INDENT, "yes");
                transformer.setOutputProperty("{http://xml.apache.org/xslt}indent-amount", indent.ToString());
            }
            Result result = new StreamResult(ByteStreamBase);
            Source source = new DOMSource(document);
            try
            {
                transformer.transform(source, result);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public static long getStartTime(Node p)
        {
            long time = 0;
            Node current = p;
            while ((current = current.getParentNode()) != null)
            {
                if (current.getAttributes() != null && current.getAttributes().getNamedItem("begin") != null)
                {
                    time += toTime(current.getAttributes().getNamedItem("begin").getNodeValue());
                }
            }

            if (p.getAttributes() != null && p.getAttributes().getNamedItem("begin") != null)
            {
                return time + toTime(p.getAttributes().getNamedItem("begin").getNodeValue());
            }
            return time;
        }

        public static long getEndTime(Node p)
        {
            long time = 0;
            Node current = p;
            while ((current = current.getParentNode()) != null)
            {
                if (current.getAttributes() != null && current.getAttributes().getNamedItem("begin") != null)
                {
                    time += toTime(current.getAttributes().getNamedItem("begin").getNodeValue());
                }
            }

            if (p.getAttributes() != null && p.getAttributes().getNamedItem("end") != null)
            {
                return time + toTime(p.getAttributes().getNamedItem("end").getNodeValue());
            }
            return time;
        }

        public static void deepCopyDocument(Document ttml, File target)
        {
            try
            {
                XPathFactory xPathfactory = XPathFactory.newInstance();
                XPath xpath = xPathfactory.newXPath();
                XPathExpression expr = xpath.compile("//*/@backgroundImage");
                NodeList nl = (NodeList)expr.evaluate(ttml, XPathConstants.NODESET);
                for (int i = 0; i < nl.getLength(); i++)
                {
                    Node backgroundImage = nl.item(i);
                    Uri backgroundImageUri = new Uri(backgroundImage.getNodeValue());
                    if (!backgroundImageUri.isAbsolute())
                    {
                        copyLarge(new Uri(ttml.getDocumentURI()).resolve(backgroundImageUri).toURL().openStream(), new File(target.toURI().resolve(backgroundImageUri).toURL().getFile()));
                    }
                }
                copyLarge(new Uri(ttml.getDocumentURI()).toURL().openStream(), target);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static long copyLarge(ByteStreamBase input, File outputFile)
        {
            byte[] buffer = new byte[16384];
            long count = 0;
            int n = 0;
            outputFile.getParentFile().mkdirs();
            FileByteStreamBase output = new FileByteStreamBase(outputFile);
            try
            {
                while (-1 != (n = input.read(buffer)))
                {
                    output.write(buffer, 0, n);
                    count += n;
                }
            }
            finally
            {
                output.close();
            }
            return count;
        }

        private class TextTrackNamespaceContext : NamespaceContext
        {
            public string getNamespaceURI(string prefix)
            {
                if (prefix.Equals("ttml"))
                {
                    return TTML_NAMESPACE;
                }
                if (prefix.Equals("smpte"))
                {
                    return SMPTE_TT_NAMESPACE;
                }
                return null;
            }

            public Iterator getPrefixes(string val)
            {
                return Arrays.asList("ttml", "smpte").iterator();
            }

            public string getPrefix(string uri)
            {
                if (uri.Equals(TTML_NAMESPACE))
                {
                    return "ttml";
                }
                if (uri.Equals(SMPTE_TT_NAMESPACE))
                {
                    return "smpte";
                }
                return null;
            }
        }
    }
}

#endif