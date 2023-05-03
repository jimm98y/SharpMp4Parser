namespace SharpMp4Parser.Streaming.Input.AAC
{
    public class AdtsHeader
    {
        public int sampleFrequencyIndex;
        public int mpegVersion;
        public int layer;
        public int protectionAbsent;
        public int profile;
        public int sampleRate;
        public int channelconfig;
        public int original;
        public int home;
        public int copyrightedStream;
        public int copyrightStart;
        public int frameLength;
        public int bufferFullness;
        public int numAacFramesPerAdtsFrame;

        public int getSize()
        {
            return 7 + (protectionAbsent == 0 ? 2 : 0);
        }
    }
}
