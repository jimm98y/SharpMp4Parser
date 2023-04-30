namespace SharpMp4Parser.Boxes.SampleEntry
{
    /**
      * Created by sannies on 30.05.13.
      */
    public interface SampleEntry : ParsableBox, Container
    {
        int getDataReferenceIndex();

        void setDataReferenceIndex(int dataReferenceIndex);
    }
}