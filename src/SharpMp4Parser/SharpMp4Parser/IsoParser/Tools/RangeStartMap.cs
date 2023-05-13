using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpMp4Parser.IsoParser.Tools
{
    // Algorithm from here: https://github.com/mes51/Intervallo/tree/master
    public class RangeStartMap<TKey, TValue> : IDictionary<TKey, TValue> where TKey : IComparable<TKey>
    {
        public RangeStartMap()
        {
            Dictionary = new SortedDictionary<TKey, TValue>();
        }

        public TValue this[TKey key]
        {
            get
            {
                var realKey = SelectKey(key);
                if (realKey.IsDefined)
                {
                    return Dictionary[realKey.Value];
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
            set
            {
                var realKey = SelectKey(key);
                if (realKey.IsDefined)
                {
                    Dictionary[realKey.Value] = value;
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
        }

        public int Count
        {
            get
            {
                return Dictionary.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return Dictionary.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return Dictionary.Values;
            }
        }

        SortedDictionary<TKey, TValue> Dictionary { get; }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(TKey key, TValue value)
        {
            Dictionary.Add(key, value);
        }

        public void Clear()
        {
            Dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (ContainsKey(item.Key))
            {
                return Equals(this[item.Key], item.Value);
            }
            return false;
        }

        public bool ContainsKey(TKey key)
        {
            return Dictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            Dictionary.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public bool Remove(TKey key)
        {
            return Dictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var realKey = SelectKey(key);
            if (realKey.IsDefined)
            {
                value = Dictionary[realKey.Value];
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Optional<TKey> SelectKey(TKey key)
        {
            if (Count < 1)
            {
                return Optional<TKey>.None();
            }

            var keys = Keys.ToArray();
            if (key.CompareTo(keys[0]) < 0)
            {
                return Optional<TKey>.Some(keys[0]);
            }

            for (var i = 1; i < keys.Length; i++)
            {
                if (key.CompareTo(keys[i]) < 0)
                {
                    return Optional<TKey>.Some(keys[i - 1]);
                }
            }

            return Optional<TKey>.Some(keys.Last());
        }

        public TKey[] SelectKeyByRange(TKey begin, TKey end)
        {
            if (Count < 1)
            {
                return new TKey[0];
            }

            var keys = Keys.ToList();
            if (begin.CompareTo(keys.Last()) > 0)
            {
                return new TKey[] { keys.Last() };
            }
            else if (end.CompareTo(keys.First()) < 0)
            {
                return new TKey[] { keys.First() };
            }

            var beginIndex = Math.Max(keys.FindIndex((k) => begin.CompareTo(k) >= 0), 0);
            var endIndex = keys.FindLastIndex((k) => end.CompareTo(k) >= 0);
            if (endIndex < 0)
            {
                endIndex = Count - 1;
            }

            return keys.Skip(beginIndex).Take(endIndex - beginIndex + 1).ToArray();
        }

        public KeyValuePair<TKey, TValue> GetPair(TKey key)
        {
            var realKey = SelectKey(key);
            if (realKey.IsDefined)
            {
                return new KeyValuePair<TKey, TValue>(realKey.Value, Dictionary[realKey.Value]);
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }
    }

    public abstract class Optional<T> : IEnumerable<T>
    {
        public abstract T Value { get; }

        public abstract bool IsEmpty { get; }

        public bool IsDefined
        {
            get
            {
                return !IsEmpty;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (IsDefined)
            {
                yield return Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static Optional<T> None()
        {
            return new None<T>();
        }

        public static Optional<T> Some(T value)
        {
            return new Some<T>(value);
        }
    }

    public class Some<T> : Optional<T>
    {
        internal Some(T value)
        {
            Value = value;
        }

        public override T Value { get; }

        public override bool IsEmpty
        {
            get
            {
                return false;
            }
        }
    }

    public class None<T> : Optional<T>
    {
        internal None() { }

        public override T Value
        {
            get
            {
                throw new Exception();
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return true;
            }
        }
    }
}
