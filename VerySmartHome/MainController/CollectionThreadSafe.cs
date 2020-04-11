using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VerySmartHome.Interfaces;

namespace VerySmartHome.MainController
{
    public class CollectionThreadSafe<T> : IEnumerable<T>, IEnumerator<T> where T : IComparableById
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
                for (int i = 0; i < Items.Count; i++)
                {
                    if (item.GetId() == Items[i].GetId())
                        return true;
                }
                return false;
            }
        }
        public void Add(T item)
        {
            lock (Locker)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (Items[i].GetId() == item.GetId())
                        return;
                }
                Items.Add(item);
            }
        }
        public void Remove(T item)
        {
            lock (Locker)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (Items[i].GetId() == item.GetId())
                        Items.RemoveAt(i);
                }
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
            catch(Exception e)
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
