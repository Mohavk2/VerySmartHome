using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace SmartBulbColor.ViewModels
{
    class DispatchedCollection<T> : ObservableCollection<T>
    {
        static readonly Dispatcher CurrentDispatcher;
        static DispatchedCollection()
        {
            CurrentDispatcher = Dispatcher.CurrentDispatcher;
        }
        public DispatchedCollection() : base() { }
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
