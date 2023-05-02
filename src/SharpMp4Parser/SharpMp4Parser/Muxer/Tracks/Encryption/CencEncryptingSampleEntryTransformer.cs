using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using System;
using System.IO;
using System.Collections.Generic;

namespace SharpMp4Parser.Muxer.Tracks.Encryption
{
    public class CencEncryptingSampleEntryTransformer
    {
        private Dictionary<SampleEntry, SampleEntry> cache = new Dictionary<SampleEntry, SampleEntry>();

        public SampleEntry transform(SampleEntry se, string encryptionAlgo, Uuid defaultKeyId)
        {
            SampleEntry encSampleEntry = cache[se];
            if (encSampleEntry == null)
            {
                ByteArrayOutputStream baos = new ByteArrayOutputStream();
                try
                {
                    se.getBox(Channels.newChannel(baos));
                    encSampleEntry = (SampleEntry)new IsoFile(new ByteBufferByteChannel(ByteBuffer.wrap(baos.toByteArray()))).getBoxes()[0];
                }
                catch (IOException)
                {
                    throw new Exception("Dumping stsd to memory failed");
                }
                // stsd is now a copy of the original stsd. Not very efficient but we don't have to do that a hundred times ...

                OriginalFormatBox originalFormatBox = new OriginalFormatBox();
                originalFormatBox.setDataFormat(se.getType());
                ProtectionSchemeInformationBox sinf = new ProtectionSchemeInformationBox();
                sinf.addBox(originalFormatBox);

                SchemeTypeBox schm = new SchemeTypeBox();
                schm.setSchemeType(encryptionAlgo);
                schm.setSchemeVersion(0x00010000);
                sinf.addBox(schm);

                SchemeInformationBox schi = new SchemeInformationBox();
                TrackEncryptionBox trackEncryptionBox = new TrackEncryptionBox();
                trackEncryptionBox.setDefaultIvSize(8);
                trackEncryptionBox.setDefaultAlgorithmId(0x01);
                trackEncryptionBox.setDefault_KID(defaultKeyId);
                schi.addBox(trackEncryptionBox);

                sinf.addBox(schi);


                if (se is AudioSampleEntry) 
                {
                    ((AudioSampleEntry)encSampleEntry).setType("enca");
                    ((AudioSampleEntry)encSampleEntry).addBox(sinf);
                } 
                else if (se is VisualSampleEntry) 
                {
                    ((VisualSampleEntry)encSampleEntry).setType("encv");
                    ((VisualSampleEntry)encSampleEntry).addBox(sinf);
                } 
                else
                {
                    throw new Exception("I don't know how to cenc " + se.getType());
                }
                cache.Add(se, encSampleEntry);
            }
            return encSampleEntry;
        }
    }
}
