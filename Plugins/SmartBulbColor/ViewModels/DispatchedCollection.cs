using CommonLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace SmartBulbColor.ViewModels
{
    class DispatchedCollection<T> : ObservableCollection<T>
    {
        readonly Dispatcher CurrentDispatcher;
        public DispatchedCollection()
        {
            CurrentDispatcher = Dispatcher.CurrentDispatcher;
        }
        public DispatchedCollection(IEnumerable<T> collection)
        {
            foreach(var item in collection)
            {
                Items.Add(item);
            }
        }
        public void AddSafe(T itemToAdd)
        {
            CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                if(!this.Items.Contains((T)itemToAdd))
                this.Add((T)itemToAdd);
            }));
        }
        public void RemoveSafe(T itemToRemove)
        {
            CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                this.Items.Remove((T)itemToRemove);
            }));
        }
    }
}
