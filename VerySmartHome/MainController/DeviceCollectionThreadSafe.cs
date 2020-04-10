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
    public class DeviceCollectionThreadSafe<T> : IEnumerable<T>, IEnumerator<T> where T : IComparableById
    {
        List<T> Items;
        int Position;
        object Locker = new object();
        T IEnumerator<T>.Current { get => Items[Position]; }
        object IEnumerator.Current { get => (T)Items[Position]; }
        public int Count
        {
            get
            {
                lock(Locker)
                {
                    return Items.Count;
                }
            }
        }
        public DeviceCollectionThreadSafe()
        {
            Items = new List<T>();
            Position = -1;
        }
        public void Add(T item)
        {
            lock(Locker)
            {
                Items.Add(item);
            }
        }
        public void Remove(T item)
        {
            lock(Locker)
            {
                Items.Remove(item);
            }
        }
        public bool MoveNext()
        {
            Monitor.Enter(Locker);
            if (Position > Items.Count)
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
