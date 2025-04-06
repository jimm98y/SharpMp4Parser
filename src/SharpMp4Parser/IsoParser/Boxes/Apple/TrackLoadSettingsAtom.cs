﻿using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.Apple
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * Track load settings atoms contain information that indicates how the
     * track is to be used in its movie. Applications that read QuickTime
     * files can use this information to process the movie data more efficiently.
     * Track load settings atoms have an atom type value of 'load'.
     */
    public class TrackLoadSettingsAtom : AbstractBox
    {

        public const string TYPE = "load";

        int preloadStartTime;
        int preloadDuration;
        int preloadFlags;
        int defaultHints;

        public TrackLoadSettingsAtom() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return 16;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            byteBuffer.putInt(preloadStartTime);
            byteBuffer.putInt(preloadDuration);
            byteBuffer.putInt(preloadFlags);
            byteBuffer.putInt(defaultHints);
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            preloadStartTime = content.getInt();
            preloadDuration = content.getInt();
            preloadFlags = content.getInt();
            defaultHints = content.getInt();
        }

        public int getPreloadStartTime()
        {
            return preloadStartTime;
        }

        public void setPreloadStartTime(int preloadStartTime)
        {
            this.preloadStartTime = preloadStartTime;
        }

        public int getPreloadDuration()
        {
            return preloadDuration;
        }

        public void setPreloadDuration(int preloadDuration)
        {
            this.preloadDuration = preloadDuration;
        }

        public int getPreloadFlags()
        {
            return preloadFlags;
        }

        public void setPreloadFlags(int preloadFlags)
        {
            this.preloadFlags = preloadFlags;
        }

        public int getDefaultHints()
        {
            return defaultHints;
        }

        public void setDefaultHints(int defaultHints)
        {
            this.defaultHints = defaultHints;
        }
    }
}
