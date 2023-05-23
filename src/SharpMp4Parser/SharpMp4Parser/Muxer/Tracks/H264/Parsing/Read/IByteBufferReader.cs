namespace SharpMp4Parser.Muxer.Tracks.H264.Parsing.Read
{
    /**
     * Created by Jimm98y on 5/24/2023.
     */
    public interface IByteBufferReader
    {
        bool moreRBSPData();
        bool readBool();
        bool readBool(string message);
        long readNBit(int n);
        long readNBit(int n, string message);
        int readSE();
        int readSE(string message);
        void readTrailingBits();
        int readU(int i, string str);
        int readUE();
        int readUE(string message);
    }
}
