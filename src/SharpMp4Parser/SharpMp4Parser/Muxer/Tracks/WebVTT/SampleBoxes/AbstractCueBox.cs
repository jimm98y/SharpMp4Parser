using SharpMp4Parser.IsoParser;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.Muxer.Tracks.WebVTT.SampleBoxes
{
    public abstract class AbstractCueBox : Box
    {
        string content = "";
        string type;

        public AbstractCueBox(string type)
        {
            this.type = type;
        }

        public string getContent()
        {
            return content;
        }

        public void setContent(string content)
        {
            this.content = content;
        }

        public long getSize()
        {
            return 8 + Utf8.utf8StringLengthInBytes(content);
        }

        public void getBox(ByteStream writableByteChannel)
        {
            ByteBuffer header = ByteBuffer.allocate(CastUtils.l2i(getSize()));
            IsoTypeWriter.writeUInt32(header, getSize());
            header.put(IsoFile.fourCCtoBytes(getType()));
            header.put(Utf8.convert(content));
            writableByteChannel.write((ByteBuffer)((Java.Buffer)header).rewind());
        }

        public string getType()
        {
            return type;
        }
    }
}
