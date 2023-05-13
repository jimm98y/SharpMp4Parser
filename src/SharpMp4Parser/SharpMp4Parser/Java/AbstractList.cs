using System;
using System.Collections;
using System.Collections.Generic;

namespace SharpMp4Parser.Java
{
    public static class ListExtensions
    {
        public static List<T> GetRange<T>(this IList<T> list, int startIndex, int length)
        {
            List<T> ret = new List<T>();
            for(int i = startIndex; i < Math.Min(list.Count, length); i++)
            {
                ret.Add(list[i]);
            }

            return ret;
        }
    }

    public class AbstractList<T> : IList<T>
    {
        public virtual T get(int index)
        {
            return _list[index];
        }

        public virtual int size()
        {
            return _list.Count;
        }

        protected List<T> _list = new List<T>();

        public T this[int index] { get => get(index); set => _list[index] = value; }

        public int Count => size();

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}
