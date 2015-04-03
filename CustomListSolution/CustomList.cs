using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Reflection;

namespace CustomCollections
{
    public class CustomList<T> : IList<T>
    {
        private const int _defaultCapacity = 4;
        private const int MAX_LENGTH = 0X7FEFFFFF;

        private T[] _items;
        private int _size;
        private Object _syncRoot;

        static readonly T[] _emptyArray = new T[0];

        #region Constructors
        public CustomList()
        {
            _items = _emptyArray;
        }

        public CustomList(int capacity)
        {
            if (capacity == 0)
                _items = _emptyArray;
            else
                _items = new T[capacity];
        }

        public CustomList(IEnumerable<T> collection)
        {
            ICollection<T> c = collection as ICollection<T>;
            if (c != null)
            {
                int count = c.Count;
                if (count == 0)
                {
                    _items = _emptyArray;
                }
                else
                {
                    _items = new T[c.Count];
                    c.CopyTo(_items, 0);
                }
            }
            else
            {
                _size = 0;
                _items = _emptyArray;

                foreach (T item in collection)
                {
                    Add(item);
                }
            }
        }
        #endregion

        #region Properties

        public int Capacity
        {
            get
            {
                return _items.Length;
            }
            set
            {
                if (value < _size)
                    throw new ArgumentOutOfRangeException(String.Format("{0} is less than elements count.", value));

                if (value != _items.Length)
                {
                    if (value > 0)
                    {
                        T[] newArray = new T[value];
                        if (_size > 0)
                        {
                            ListHelper.Copy(_items, 0, newArray, 0, _size);
                        }
                        _items = newArray;
                    }
                    else
                    {
                        _items = _emptyArray;
                    }
                }
            }
        }

        public int Count
        {
            get { return _size; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_size)
                {
                    throw new ArgumentOutOfRangeException();
                }
                return _items[index];
            }
            set
            {
                if ((uint)index >= (uint)_size)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _items[index] = value;
            }
        }

        internal Object SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    System.Threading.Interlocked.CompareExchange<Object>(ref _syncRoot, new Object(), null);
                }
                return _syncRoot;
            }
        }

        public IList<T> Synchronized
        {
            get { return new SynchronizedList(this); }
        }

        #endregion

        public void Add(T item)
        {
            if (_size == _items.Length) EnsureCapacity(_size + 1);
            _items[_size++] = item;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            InsertRange(_size, collection);
        }

        public int BinarySearch(T value)
        {            
            int pos;
            try
            {
                pos = ListHelper.BinarySearch(_items, 0, _size, value, null);
            }
            catch (ArgumentException e)
            {
                ArgumentException exc = new ArgumentException(e.Message);
                throw exc;
            }
            return pos;
        }

        public int BinarySearch(T value, IComparer<T> comparer)
        {
            int pos;
            try
            {
                pos = ListHelper.BinarySearch(_items, 0, _size, value, comparer);
            }
            catch (ArgumentException e)
            {
                ArgumentException exc = new ArgumentException(e.Message);
                throw exc;
            }
            return pos;
        }

        public int BinarySearch(T value, Comparison<T> comparison)
        {
            int pos;
            try
            {
                if (comparison == null)
                {
                    return ListHelper.BinarySearch(_items, 0, _size, value, null);
                }
                IComparer<T> comparer = new ListHelper.FuncToComparer<T>(comparison);
                pos = ListHelper.BinarySearch(_items, 0, _size, value, comparer);
            }
            catch (ArgumentException e)
            {
                ArgumentException exc = new ArgumentException(e.Message);
                throw exc;
            }
            return pos;
        }

        public void Clear()
        {
            if (_size > 0)
            {
                ListHelper.Clear(_items, 0, _size);
                _size = 0;
            }
        }

        public bool Contains(T item)
        {
            if (ListHelper.IndexOf(_items, item) != -1)
                return true;
            else
                return false;
        }

        public void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ListHelper.Copy(_items, 0, array, arrayIndex, _size);
        }

        public int IndexOf(T item)
        {
            return ListHelper.IndexOf(_items, item);
        }

        public void Insert(int index, T item)
        {
            if ((uint)index > (uint)_size)
            {
                throw new ArgumentOutOfRangeException("Index was out of bounds.");
            }

            if (_size == _items.Length) EnsureCapacity(_size + 1);

            if (index < _size)
            {
                ListHelper.Copy(_items, index, _items, index + 1, _size - index);
            }

            _items[index] = item;
            _size++;
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("Inserted collection was null.");
            }

            if ((uint)index > (uint)_size)
            {
                throw new ArgumentOutOfRangeException("Index was out of bounds.");
            }

            ICollection<T> c = collection as ICollection<T>;
            if (c != null)
            {
                int count = c.Count;
                if (count > 0)
                {
                    EnsureCapacity(_size + count);
                    if (index < _size)
                    {
                        ListHelper.Copy(_items, index, _items, index + count, _size - index);
                    }

                    if (this == collection)
                    {
                        ListHelper.Copy(_items, 0, _items, index, index);
                        ListHelper.Copy(_items, index + count, _items, index * 2, _size - index);
                    }
                    else
                    {
                        T[] itemsToInsert = new T[count];
                        c.CopyTo(itemsToInsert, 0);
                        itemsToInsert.CopyTo(_items, index);
                    }
                    _size += count;
                }
            }
            else
            {
                _size = 0;
                _items = _emptyArray;

                foreach (T item in collection)
                {
                    Add(item);
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            int i = 0;
            foreach (T item in _items)
            {
                if (i < _size)
                {
                    i++;
                    yield return item;
                }
                else
                    yield break;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)_size)
            {
                throw new ArgumentOutOfRangeException("Index was out of list bounds.");
            }

            _size--;

            if (index < _size)
            {
                ListHelper.Copy(_items, index + 1, _items, index, _size - index);
            }

            _items[_size] = default(T);
        }

        public void Sort()
        {
            try
            {
                ListHelper.Sort(_items, 0, _size, null);
            }
            catch (ArgumentException e)
            {
                ArgumentException exc = new ArgumentException(e.Message);
                throw exc;
            }
        }

        public void Sort(IComparer<T> comparer)
        {
            try
            {
                ListHelper.Sort(_items, 0, _size, comparer);
            }
            catch (ArgumentException e)
            {
                ArgumentException exc = new ArgumentException(e.Message);
                throw exc;
            }
        }

        public void Sort(Comparison<T> comparison)
        {
            try
            {
                if (comparison == null)
                {
                    ListHelper.Sort(_items, 0, _size, null);
                }
                IComparer<T> comparer = new ListHelper.FuncToComparer<T>(comparison);
                ListHelper.Sort(_items, 0, _size, comparer);
            }
            catch (ArgumentException e)
            {
                ArgumentException exc = new ArgumentException(e.Message);
                throw exc;
            }
        }

        public T[] ToArray()
        {
            T[] array = new T[_size];
            ListHelper.Copy(_items, 0, array, 0, _size);
            return array;
        }

        public override string ToString()
        {
            return "Count: " + Count.ToString();
        }

        public void TrimExcess()
        {
            Capacity = _size;
        }

        private void EnsureCapacity(int min)
        {
            if (_items.Length < min)
            {
                int newCapacity = _items.Length == 0 ? _defaultCapacity : _items.Length * 2;
                if ((uint)newCapacity > MAX_LENGTH) newCapacity = MAX_LENGTH;
                if (newCapacity < min) newCapacity = min;
                Capacity = newCapacity;
            }
        }

        private static class ListHelper
        {
            public static void Copy(T[] sourceArray, int sourceIndex, T[] destinationArray, int destinationIndex, int length)
            {
                if (sourceArray == null)
                    throw new ArgumentNullException("source array");
                if (destinationArray == null)
                    throw new ArgumentNullException("destination array");
                if (length < 0)
                    throw new ArgumentOutOfRangeException("length");
                if (sourceIndex < sourceArray.GetLowerBound(0))
                    throw new ArgumentOutOfRangeException("sourceIndex");
                if (destinationIndex < destinationArray.GetLowerBound(0))
                    throw new ArgumentOutOfRangeException("destinationIndex");
                if (sourceIndex + length > sourceArray.GetLongLength(0))
                    throw new IndexOutOfRangeException("source array");
                if (destinationIndex + length > destinationArray.GetLongLength(0))
                    throw new IndexOutOfRangeException("destination array");

                if (destinationArray == sourceArray)
                {
                    if (sourceIndex == destinationIndex) return;
                    if (sourceIndex > destinationIndex)
                    {
                        for (int i = 0; i < length; i++)
                            destinationArray[destinationIndex + i] = sourceArray[sourceIndex + i];
                        return;
                    }
                    else
                    {
                        for (int i = length - 1; i >= 0; i--)
                            destinationArray[destinationIndex + i] = sourceArray[sourceIndex + i];
                        return;
                    }
                }

                for (int i = 0; i < length; i++)
                    destinationArray[destinationIndex + i] = sourceArray[sourceIndex + i];
            }

            public static int IndexOf(T[] array, T item)
            {
                if (array == null)
                    throw new ArgumentNullException("array");

                int lo = array.GetLowerBound(0);
                int hi = array.GetUpperBound(0);

                if ((Object)item == null)
                {
                    for (int i = lo; i < hi; i++)
                        if ((Object)array[i] == null)
                            return i;
                    return lo - 1;
                }
                else
                {
                    EqualityComparer<T> c = EqualityComparer<T>.Default;
                    for (int i = lo; i < hi; i++)
                    {
                        if (c.Equals(array[i], item)) return i;
                    }
                    return lo - 1;
                }
            }

            public static void Clear(T[] array, int startIndex, int endIndex)
            {
                if (array == null)
                    throw new ArgumentNullException("array");
                if (startIndex < array.GetLowerBound(0))
                    throw new ArgumentOutOfRangeException("startIndex");
                if (endIndex > array.GetLongLength(0))
                    throw new IndexOutOfRangeException("source array");

                for (int i = startIndex; i < endIndex; i++)
                {
                    array[i] = default(T);
                }
            }

            public static int BinarySearch(T[] array, int startIndex, int count, T value, IComparer<T> comparer)
            {
                if (array == null)
                    throw new NullReferenceException("array");

                if (comparer == null)
                {
                    //Type t = typeof(T);
                    //if (typeof(IComparable).IsAssignableFrom(t))
                    //{                        
                    //    comparer = new FuncToComparer<T>((T t1, T t2) => { return ((IComparable)t1).CompareTo(t2); });
                    //}

                    comparer = Comparer<T>.Default;
                }

                int lo = startIndex;
                int hi = count;

                if (hi - lo < 0)
                    throw new InvalidOperationException("Invalid array lenght.");

                while (lo <= hi)
                {
                    int i = lo + ((hi - lo) >> 1);
                    int order;
                    try
                    {
                        order = comparer.Compare(array[i], value);
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException("Comparer is not valid.", "comparer");
                    }

                    if (order == 0) return i;
                    if (order < 0)
                    {
                        lo = i + 1;
                    }
                    else
                    {
                        hi = i - 1;
                    }
                }

                return ~lo;
            }

            public static void Sort(T[] array, int startIndex, int count, IComparer<T> comparer)
            {
                if (array == null)
                    throw new NullReferenceException("array");
                try
                {
                    Array.Sort(array, startIndex, count, comparer);
                }
                catch(InvalidOperationException e)                    // this type of exception in this method causes only by invalid comparer or array
                {                                                     // both arguments are parameters of this method
                    throw new ArgumentException(e.Message, e.Source); // so we can throw an ArgumentException
                }
            }

            public sealed class FuncToComparer<T> : IComparer<T>
            {
                Comparison<T> comparison;

                public FuncToComparer(Comparison<T> comparison)
                {
                    this.comparison = comparison;
                }

                public int Compare(T x, T y)
                {
                    return comparison(x, y);
                }
            }
        }

        private class SynchronizedList : IList<T>
        {
            private CustomList<T> _list;
            private Object _syncLock;

            internal SynchronizedList(CustomList<T> list)
            {
                _list = list;
                _syncLock = list.SyncRoot;
            }

            public int Count
            {
                get
                {
                    lock (_syncLock)
                    {
                        return _list.Count;
                    }
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return ((ICollection<T>)_list).IsReadOnly;
                }
            }

            public void Add(T item)
            {
                lock (_syncLock)
                {
                    _list.Add(item);
                }
            }

            public void Clear()
            {
                lock (_syncLock)
                {
                    _list.Clear();
                }
            }

            public bool Contains(T item)
            {
                lock (_syncLock)
                {
                    return _list.Contains(item);
                }
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                lock (_syncLock)
                {
                    _list.CopyTo(array, arrayIndex);
                }
            }

            public bool Remove(T item)
            {
                lock (_syncLock)
                {
                    return _list.Remove(item);
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                lock (_syncLock)
                {
                    return _list.GetEnumerator();
                }
            }

            public IEnumerator<T> GetEnumerator()
            {
                lock (_syncLock)
                {
                    return ((IEnumerable<T>)_list).GetEnumerator();
                }
            }

            public T this[int index]
            {
                get
                {
                    lock (_syncLock)
                    {
                        return _list[index];
                    }
                }
                set
                {
                    lock (_syncLock)
                    {
                        _list[index] = value;
                    }
                }
            }

            public int IndexOf(T item)
            {
                lock (_syncLock)
                {
                    return _list.IndexOf(item);
                }
            }

            public void Insert(int index, T item)
            {
                lock (_syncLock)
                {
                    _list.Insert(index, item);
                }
            }

            public void RemoveAt(int index)
            {
                lock (_syncLock)
                {
                    _list.RemoveAt(index);
                }
            }
        }

    }
}
