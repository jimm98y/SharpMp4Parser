#region Copyright 2010 by Apache Harmony, Licensed under the Apache License, Version 2.0
/*  Licensed to the Apache Software Foundation (ASF) under one or more
 *  contributor license agreements.  See the NOTICE file distributed with
 *  this work for additional information regarding copyright ownership.
 *  The ASF licenses this file to You under the Apache License, Version 2.0
 *  (the "License"); you may not use this file except in compliance with
 *  the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */
#endregion

namespace SharpMp4Parser.Java
{
    /// <summary>
    /// <see cref="HeapByteBuffer"/>, <see cref="ReadWriteHeapByteBuffer"/> and <see cref="ReadOnlyHeapByteBuffer"/> compose
    /// the implementation of array based byte buffers.
    /// <para/>
    /// <see cref="ReadOnlyHeapByteBuffer"/> extends <see cref="HeapByteBuffer"/> with all the write methods
    /// throwing read only exception.
    /// <para/>
    /// This class is marked sealed for runtime performance.
    /// </summary>
    internal sealed class ReadOnlyHeapByteBuffer : HeapByteBuffer
    {
        internal static ReadOnlyHeapByteBuffer Copy(HeapByteBuffer other, int markOfOther)
        {
            return new ReadOnlyHeapByteBuffer(other.backingArray, other.capacity(), other.offset)
            {
                _limit = other.limit(),
                _position = other.position(),
                _mark = markOfOther,
                _order = other._order
            };
        }
        internal ReadOnlyHeapByteBuffer(byte[] backingArray, int capacity, int arrayOffset)
            : base(backingArray, capacity, arrayOffset)
        { }

        public override ByteBuffer AsReadOnlyBuffer() => Copy(this, _mark);

        public override ByteBuffer Compact()
        {
            throw new ReadOnlyBufferException();
        }

        public override ByteBuffer duplicate() => Copy(this, _mark);

        public override bool IsReadOnly => true;

        protected override byte[] ProtectedArray
        {
            get { throw new ReadOnlyBufferException(); }
        }
        protected override int ProtectedArrayOffset
        {
            get { throw new ReadOnlyBufferException(); }
        }

        protected override bool ProtectedHasArray => false;

        public override ByteBuffer put(byte value)
        {
            throw new ReadOnlyBufferException();
        }

        public override ByteBuffer put(int index, byte value)
        {
            throw new ReadOnlyBufferException();
        }

        public override ByteBuffer put(byte[] source, int offset, int length)
        {
            throw new ReadOnlyBufferException();
        }

        public override ByteBuffer putDouble(double value)
        {
            throw new ReadOnlyBufferException();
        }

        public override ByteBuffer putDouble(int index, double value)
        {
            throw new ReadOnlyBufferException();
        }

        public override ByteBuffer putSingle(float value)
        {
            throw new ReadOnlyBufferException();
        }

        public override ByteBuffer putSingle(int index, float value)
        {
            throw new ReadOnlyBufferException();
        }

        public override ByteBuffer putInt(int value)
        {
            throw new ReadOnlyBufferException();
        }

        public override ByteBuffer putInt(int index, int value)
        {
            throw new ReadOnlyBufferException();
        }

        public override ByteBuffer putLong(int index, long value)
        {
            throw new ReadOnlyBufferException();
        }

        public override ByteBuffer putLong(long value)
        {
            throw new ReadOnlyBufferException();
        }

        public override ByteBuffer putShort(int index, short value)
        {
            throw new ReadOnlyBufferException();
        }

        public override ByteBuffer putShort(short value)
        {
            throw new ReadOnlyBufferException();
        }

        public override ByteBuffer put(ByteBuffer buffer)
        {
            throw new ReadOnlyBufferException();
        }

        public override ByteBuffer slice()
        {
            return new ReadOnlyHeapByteBuffer(backingArray, remaining(), offset + _position)
            {
                _order = this._order
            };
        }
    }
}