using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using VerySmartHome.Interfaces;

namespace SmartBulbColor.ViewModels
{
    class DispatchedCollection<T> : ObservableCollection<T> where T : IComparableById
    {
        static readonly Dispatcher CurrentDispatcher;
        static DispatchedCollection()
        {
            CurrentDispatcher = Dispatcher.CurrentDispatcher;
        }
        public DispatchedCollection() : base() { }
        public void AddSafe(IComparableById itemToAdd)
        {
            CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => 
            {
                for(int i = 0; i < Count; i++)
                {
                    if (Items[i].GetId() == itemToAdd.GetId())
                        return;
                }
                this.Add((T)itemToAdd); 
            }));
        }
        public void RemoveSafe(IComparableById itemToRemove)
        {
            CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => 
            {
                for (int i = 0; i < Count; i++)
                {
                    if (Items[i].GetId() == itemToRemove.GetId())
                        this.RemoveAt(i);
                } 
            }));
        }
    }
}
