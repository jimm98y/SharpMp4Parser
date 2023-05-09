using SharpMp4Parser.Muxer.Container.MP4;
using SharpMp4Parser.Muxer;

namespace SharpMp4Parser.Tests
{
    public class InTestMovieCreator
    {
        public static Movie createMovieOnlyVideo(params string[] names)
        {
            Movie m = new Movie();
            foreach (string name in names)
            {
                Movie m1 = MovieCreator.build((name));
                foreach (Track track in m1.getTracks())
                {
                    if ("vide".Equals(track.getHandler()))
                    {
                        m.addTrack(track);
                    }
                }

            }
            return m;
        }

    }
}
