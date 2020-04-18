using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using VerySmartHome.Interfaces;

namespace SmartBulbColor.ViewModels
{
    class DispatchedCollection<T> : ObservableCollection<T> where T : IHasID
    {
        static readonly Dispatcher CurrentDispatcher;
        static DispatchedCollection()
        {
            CurrentDispatcher = Dispatcher.CurrentDispatcher;
        }
        public DispatchedCollection() : base() { }
        public void AddSafe(IHasID itemToAdd)
        {
            CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                if(!this.Items.Contains((T)itemToAdd))
                this.Add((T)itemToAdd);
            }));
        }
        public void RemoveSafe(IHasID itemToRemove)
        {
            CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                this.Items.Remove((T)itemToRemove);
            }));
        }
    }
}
