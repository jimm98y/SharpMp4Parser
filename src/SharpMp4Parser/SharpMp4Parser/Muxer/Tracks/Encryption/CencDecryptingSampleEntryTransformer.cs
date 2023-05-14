using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;
using System.IO;
using Path = SharpMp4Parser.IsoParser.Tools.Path;

namespace SharpMp4Parser.Muxer.Tracks.Encryption
{
    public class CencDecryptingSampleEntryTransformer
    {
        private Dictionary<SampleEntry, SampleEntry> cache = new Dictionary<SampleEntry, SampleEntry>();

        public SampleEntry transform(SampleEntry se)
        {
            SampleEntry decSe = cache[se];
            if (decSe == null)
            {
                OriginalFormatBox frma;
                if (se.getType().Equals("enca"))
                {
                    frma = Path.getPath<OriginalFormatBox>((AudioSampleEntry)se, "sinf/frma");
                }
                else if (se.getType().Equals("encv"))
                {
                    frma = Path.getPath<OriginalFormatBox>((VisualSampleEntry)se, "sinf/frma");
                }
                else
                {
                    return se; // it's no encrypted SampleEntry - do nothing
                }
                if (frma == null)
                {
                    throw new Exception("Could not find frma box");
                }


                ByteStream baos = new ByteStream();
                try
                {
                    // This creates a copy cause I can't change the original instance
                    se.getBox(Channels.newChannel(baos));
                    decSe = (SampleEntry)new IsoFile(new ByteBufferByteChannel(ByteBuffer.wrap(baos.toByteArray()))).getBoxes()[0];
                }
                catch (IOException)
                {
                    throw new Exception("Dumping stsd to memory failed");
                }

                if (decSe is AudioSampleEntry) 
                {
                    ((AudioSampleEntry)decSe).setType(frma.getDataFormat());
                }
                else if (decSe is VisualSampleEntry)
                {
                    ((VisualSampleEntry)decSe).setType(frma.getDataFormat());
                } 
                else
                {
                    throw new Exception("I don't know " + decSe.getType());
                }

                List<Box> nuBoxes = new List<Box>();
                foreach (Box box in decSe.getBoxes())
                {
                    if (!box.getType().Equals("sinf"))
                    {
                        nuBoxes.Add(box);
                    }
                }
                decSe.setBoxes(nuBoxes);
                cache.Add(se, decSe);
            }
            return decSe;
        }
    }
}
