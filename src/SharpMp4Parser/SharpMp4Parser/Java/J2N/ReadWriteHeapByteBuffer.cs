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

using System;


namespace SharpMp4Parser.Java
{
    //using SR = J2N.Resources.Strings;

    /// <summary>
    /// <see cref="HeapByteBuffer"/>, <see cref="ReadWriteHeapByteBuffer"/> and <see cref="ReadOnlyHeapByteBuffer"/> compose
    /// the implementation of array based byte buffers.
    /// <para/>
    /// <see cref="ReadWriteHeapByteBuffer"/> extends <see cref="HeapByteBuffer"/> with all the write methods.
    /// <para/>
    /// This class is marked sealed for runtime performance.
    /// </summary>
    public class ReadWriteHeapByteBuffer : HeapByteBuffer
    {
        internal static ReadWriteHeapByteBuffer Copy(HeapByteBuffer other, int markOfOther)
        {
            return new ReadWriteHeapByteBuffer(other.backingArray, other.capacity(), other.offset)
            {
                _limit = other.limit(),
                _position = other.position(),
                _mark = markOfOther,
                _order = other._order
            };
        }

        internal ReadWriteHeapByteBuffer(byte[] backingArray)
            : base(backingArray)
        { }

        internal ReadWriteHeapByteBuffer(int capacity)
            : base(capacity)
        { }

        internal ReadWriteHeapByteBuffer(byte[] backingArray, int capacity, int arrayOffset)
            : base(backingArray, capacity, arrayOffset)
        { }

        public override ByteBuffer AsReadOnlyBuffer()
        {
            return ReadOnlyHeapByteBuffer.Copy(this, _mark);
        }

        public override ByteBuffer Compact()
        {
            System.Array.Copy(backingArray, _position + offset, backingArray, offset,
                    remaining());
            _position = _limit - _position;
            _limit = _capacity;
            _mark = UnsetMark;
            return this;
        }

        public override ByteBuffer duplicate() => Copy(this, _mark);

        public override bool IsReadOnly => false;

        protected override byte[] ProtectedArray => backingArray;

        protected override int ProtectedArrayOffset => offset;

        protected override bool ProtectedHasArray => true;


        public override ByteBuffer put(byte value)
        {
            if (_position == _limit)
            {
                throw new BufferOverflowException();
            }
            backingArray[offset + _position++] = value;
            return this;
        }

        public override ByteBuffer put(int index, byte value)
        {
            if (index < 0 || index >= _limit)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            backingArray[offset + index] = value;
            return this;
        }

        /*
         * Override ByteBuffer.put(byte[], int, int) to improve performance.
         * 
         * (non-Javadoc)
         * 
         * @see java.nio.ByteBuffer#put(byte[], int, int)
         */

        public override ByteBuffer put(byte[] source, int offset, int length)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            int len = source.Length;
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "SR.ArgumentOutOfRange_NeedNonNegNum");
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "SR.ArgumentOutOfRange_NeedNonNegNum");
            if (offset > len - length) // Checks for int overflow
                throw new ArgumentOutOfRangeException(nameof(length), "SR.ArgumentOutOfRange_IndexLength");
            if (length > remaining())
                throw new BufferOverflowException();
            if (IsReadOnly)
                throw new ReadOnlyBufferException();

            System.Array.Copy(source, offset, backingArray, base.offset + _position, length);
            _position += length;
            return this;
        }

        public override ByteBuffer putDouble(double value)
        {
            return putLong(BitConversion.DoubleToRawInt64Bits(value));
        }

        public override ByteBuffer putDouble(int index, double value)
        {
            return putLong(index, BitConversion.DoubleToRawInt64Bits(value));
        }

        public override ByteBuffer putSingle(float value)
        {
            return putInt(BitConversion.SingleToInt32Bits(value));
        }

        public override ByteBuffer putSingle(int index, float value)
        {
            return putInt(index, BitConversion.SingleToInt32Bits(value));
        }

        public override ByteBuffer putInt(int value)
        {
            int newPosition = _position + 4;
            if (newPosition < 0) // J2N: Added check for overflowing integer
                throw new ArgumentOutOfRangeException("Position");
            if (newPosition > _limit)
                throw new BufferOverflowException();

            Store(_position, value);
            _position = newPosition;
            return this;
        }

        public override ByteBuffer putInt(int index, int value)
        {
            int newIndex = index + 4;
            if (index < 0 || newIndex > _limit || newIndex < 0) // J2N: Added check for overflowing integer
                throw new ArgumentOutOfRangeException(nameof(index));

            Store(index, value);
            return this;
        }

        public override ByteBuffer putLong(int index, long value)
        {
            int newIndex = index + 8;
            if (index < 0 || newIndex > _limit || newIndex < 0) // J2N: Added check for overflowing integer
                throw new ArgumentOutOfRangeException(nameof(index));

            Store(index, value);
            return this;
        }

        public override ByteBuffer putLong(long value)
        {
            int newPosition = _position + 8;
            if (newPosition < 0) // J2N: Added check for overflowing integer
                throw new ArgumentOutOfRangeException("Position");
            if (newPosition > _limit)
                throw new BufferOverflowException();

            Store(_position, value);
            _position = newPosition;
            return this;
        }

        public override ByteBuffer putShort(int index, short value)
        {
            int newIndex = index + 2;
            if (index < 0 || newIndex > _limit || newIndex < 0) // J2N: Added check for overflowing integer
                throw new ArgumentOutOfRangeException(nameof(index));

            Store(index, value);
            return this;
        }

        public override ByteBuffer putShort(short value)
        {
            int newPosition = _position + 2;
            if (newPosition < 0) // J2N: Added check for overflowing integer
                throw new ArgumentOutOfRangeException("Position");
            if (newPosition > _limit)
                throw new BufferOverflowException();

            Store(_position, value);
            _position = newPosition;
            return this;
        }

        public override ByteBuffer slice()
        {
            return new ReadWriteHeapByteBuffer(backingArray, remaining(), offset + _position)
            {
                _order = this._order
            };
        }
    }
}