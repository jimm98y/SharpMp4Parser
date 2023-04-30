using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.ISO23009.Part1
{
    /**
     * The Event Message box ('emsg') provides signalling for generic events related to the media
     * presentation time.
     */
    public class EventMessageBox : AbstractFullBox
    {
        public const string TYPE = "emsg";

        string schemeIdUri;
        string value;
        long timescale;
        long presentationTimeDelta;
        long eventDuration;
        long id;
        byte[] messageData;

        public EventMessageBox() : base(TYPE)
        { }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            schemeIdUri = IsoTypeReader.readString(content);
            value = IsoTypeReader.readString(content);
            timescale = IsoTypeReader.readUInt32(content);
            presentationTimeDelta = IsoTypeReader.readUInt32(content);
            eventDuration = IsoTypeReader.readUInt32(content);
            id = IsoTypeReader.readUInt32(content);
            messageData = new byte[content.remaining()];
            content.get(messageData);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUtf8String(byteBuffer, schemeIdUri);
            IsoTypeWriter.writeUtf8String(byteBuffer, value);
            IsoTypeWriter.writeUInt32(byteBuffer, timescale);
            IsoTypeWriter.writeUInt32(byteBuffer, presentationTimeDelta);
            IsoTypeWriter.writeUInt32(byteBuffer, eventDuration);
            IsoTypeWriter.writeUInt32(byteBuffer, id);
            byteBuffer.put(messageData);
        }

        protected override long getContentSize()
        {
            return 22 + Utf8.utf8StringLengthInBytes(schemeIdUri) + Utf8.utf8StringLengthInBytes(value) + messageData.Length;
        }

        public string getSchemeIdUri()
        {
            return schemeIdUri;
        }

        public void setSchemeIdUri(string schemeIdUri)
        {
            this.schemeIdUri = schemeIdUri;
        }

        public string getValue()
        {
            return value;
        }

        public void setValue(string value)
        {
            this.value = value;
        }

        public long getTimescale()
        {
            return timescale;
        }

        public void setTimescale(long timescale)
        {
            this.timescale = timescale;
        }

        public long getPresentationTimeDelta()
        {
            return presentationTimeDelta;
        }

        public void setPresentationTimeDelta(long presentationTimeDelta)
        {
            this.presentationTimeDelta = presentationTimeDelta;
        }

        public long getEventDuration()
        {
            return eventDuration;
        }

        public void setEventDuration(long eventDuration)
        {
            this.eventDuration = eventDuration;
        }

        public long getId()
        {
            return id;
        }

        public void setId(long id)
        {
            this.id = id;
        }

        public byte[] getMessageData()
        {
            return messageData;
        }

        public void setMessageData(byte[] messageData)
        {
            this.messageData = messageData;
        }
    }
}