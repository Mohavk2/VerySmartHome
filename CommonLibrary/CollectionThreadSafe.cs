using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace CommonLibrary
{
    public class CollectionThreadSafe<T> : IEnumerable<T>, IEnumerator<T>
    {
        List<T> Items;

        int Position;
        object Locker = new object();

        T IEnumerator<T>.Current { get => Items[Position]; }
        object IEnumerator.Current { get => Items[Position]; }
        public int Count
        {
            get
            {
                lock (Locker)
                {
                    return Items.Count;
                }
            }
        }
        public CollectionThreadSafe()
        {
            Items = new List<T>();
            Position = -1;
        }
        public bool Contains(T item)
        {
            lock (Locker)
            {
                if (Items.Contains(item))
                    return true;
                else
                    return false;
            }
        }
        /// <summary>
        /// Adding will not apply if the collection already contains the object you trying to add
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            lock (Locker)
            {
                if (!Items.Contains(item))
                {
                    Items.Add(item);
                }
            }
        }
        public void Remove(T item)
        {
            lock (Locker)
            {
                Items.Remove(item);
            }
        }
        public bool MoveNext()
        {
            if (!Monitor.IsEntered(Locker))
            {
                Monitor.Enter(Locker);
            }
            try
            {
                if (Position < Items.Count - 1)
                {
                    Position++;
                    return true;
                }
                else
                {
                    Reset();
                    return false;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + "Something bad happend with the threadsafe collection and it's deadlocked now!");
            }
        }
        public void Reset()
        {
            Position = -1;
            Monitor.Exit(Locker);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<T>).GetEnumerator();
        }
        public IEnumerator<T> GetEnumerator()
        {
            return this;
        }
        public void Dispose()
        {

        }
    }
}
