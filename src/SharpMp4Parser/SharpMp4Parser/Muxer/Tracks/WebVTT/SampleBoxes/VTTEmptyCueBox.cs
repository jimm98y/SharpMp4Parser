using SharpMp4Parser.IsoParser;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.Muxer.Tracks.WebVTT.SampleBoxes
{
    public class VTTEmptyCueBox : Box
    {
        public VTTEmptyCueBox()
        {  }

        public long getSize()
        {
            return 8;
        }

        public void getBox(WritableByteChannel writableByteChannel)
        {
            ByteBuffer header = ByteBuffer.allocate(8);
            IsoTypeWriter.writeUInt32(header, getSize());
            header.put(IsoFile.fourCCtoBytes(getType()));
            writableByteChannel.write((ByteBuffer)((Java.Buffer)header).rewind());
        }

        public string getType()
        {
            return "vtte";
        }
    }
}
