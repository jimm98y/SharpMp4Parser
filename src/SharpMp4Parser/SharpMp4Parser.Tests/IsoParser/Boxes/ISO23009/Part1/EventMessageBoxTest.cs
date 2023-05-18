using SharpMp4Parser.IsoParser.Boxes.ISO23009.Part1;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.ISO23009.Part1
{
    [TestClass]
    public class EventMessageBoxTest : BoxRoundtripTest
    {
        [TestMethod]
        public void roundtrip()
        {
            String schemeIdUri;
            String value;
            long timescale;
            long presentationTimeDelta;
            long eventDuration;
            long id;
            byte[] messageData;


            base.roundtrip(new EventMessageBox(),
                        new KeyValuePair<string, object>[]{
                                new KeyValuePair<string, object>("schemeIdUri", "sjkfsdhjklfhskj"),
                                new KeyValuePair<string, object>("value", "sdjsfhksdhddd"),
                                new KeyValuePair<string, object>("timescale", 1L),
                                new KeyValuePair<string, object>("presentationTimeDelta", 2L),
                                new KeyValuePair<string, object>("eventDuration", 3L),
                                new KeyValuePair<string, object>("id", 4L),
                                new KeyValuePair<string, object>("messageData", new byte[]{1, 1, 2, 3, 4, 5, 6, 7})}
                );
        }
    }
}