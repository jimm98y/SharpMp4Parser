﻿using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser
{
    /**
     * The most basic imaginable box. It does not have any parsing functionality it can be used to create boxes
     * programmatically.
     */
    public interface Box
    {
        /**
         * The box's 4-cc type.
         *
         * @return the 4 character type of the box
         */
        string getType();

        long getSize();

        /**
         * Writes the complete box - size | 4-cc | content - to the given <code>ByteStreamBase</code>.
         *
         * @param ByteStreamBase the box's sink
         * @throws IOException in case of problems with the <code>Channel</code>
         */
        void getBox(ByteStream writableByteChannel);
    }
}
