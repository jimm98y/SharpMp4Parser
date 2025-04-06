﻿using SharpMp4Parser.Java;

namespace SharpMp4Parser.Muxer
{
    public interface DataSource : Closeable
    {
        /**
         * Reads a sequence of bytes from this channel into the given buffer.
         * Bytes are read starting at this channel's current position, and
         * then the file position is updated with the number of bytes actually
         * read.
         *
         * @param byteBuffer sink for this read operation
         * @return number of bytes actually read
         * @throws IOException If some I/O error occurs
         */
        int read(ByteBuffer byteBuffer);


        /**
         * Returns the current size of this DataSource.<br>
         *
         * @return The current size of this DataSource,
         * measured in bytes
         * @throws IOException If some I/O error occurs
         */
        long size();

        /**
         * Returns the DataSource's current position.
         *
         * @return This DataSource's file position,
         * a non-negative integer counting the number of bytes
         * from the beginning of the data to the current position
         * @throws IOException If some I/O error occurs
         */
        long position();

        /**
         * Sets the DataSource's position.
         *
         * @param nuPos The new position, a non-negative integer counting
         *              the number of bytes from the beginning of the data
         * @throws IOException If some I/O error occurs
         */
        void position(long nuPos);

        /**
         * Transfers bytes from this DataSource to the given writable byte
         * channel.
         * <br>
         * An attempt should be made to read up to count bytes starting at
         * the given position in this DataSource and write them to the
         * target channel.  An invocation of this method may or may not transfer
         * all of the requested bytes;
         *
         * @param position The position within the DataSource at which the transfer is to begin;
         *                 must be non-negative
         * @param count    The maximum number of bytes to be transferred; must be
         *                 non-negative
         * @param target   The target channel
         * @return the actual number of bytes written
         * @throws IOException If some I/O error occurs
         */
        long transferTo(long position, long count, ByteStream target);

        /**
         * Maps a part of this <code>DataSource</code> into a <code>ByteBuffer</code>. It might utilize
         * an operating system supported memory mapped file or potentially just reads the requested
         * portion of the file into the memory.
         *
         * @param startPosition where the requested block start
         * @param size          size of the requested block
         * @return the requested portion of the <code>DataSource</code>
         * @throws IOException If some I/O error occurs
         */
        ByteBuffer map(long startPosition, long size);

        /**
         * Tries to free all resources.
         *
         * @throws IOException If some I/O error occurs
         */
        //void close();
    }
}