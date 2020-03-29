using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using SmartBulbColor.Models;

namespace SmartBulbColor.ViewModels
{
    class BulbDispatchedCollection : ObservableCollection<ColorBulb>
    {
        static readonly Dispatcher CurrentDispatcher;
        static BulbDispatchedCollection()
        {
            CurrentDispatcher = Dispatcher.CurrentDispatcher;
        }
        public BulbDispatchedCollection() : base() { }
        public BulbDispatchedCollection(IEnumerable<ColorBulb> bulbs) : base(bulbs) { }
        public void AddSafe(ColorBulb newItem)
        {
            CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => { this.Add(newItem); }));
        }
        public void RemoveSafe(ColorBulb newItem)
        {
            CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => { this.Remove(newItem); }));
        }
        public void RefreshSafe(ObservableCollection<ColorBulb> collection)
        {
            CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                //Remove forbiden bulbs from the collection
                if (this != null && this.Count != 0)
                {
                    for (int i = 0; i < this.Count; i++)
                    {
                        bool conteinsForbiden = true;
                        for (int j = 0; j < collection.Count; j++)
                        {
                            if (collection[j].Id == this[i].Id)
                                conteinsForbiden = false;
                        }
                        if (conteinsForbiden)
                            this.Remove(this[i]);
                    }
                }
                //Add new found bulbs in the collection
                if (collection != null && collection.Count != 0)
                {
                    for (int i = 0; i < collection.Count; i++)
                    {
                        bool alreadyExists = false;
                        for (int j = 0; j < this.Count; j++)
                        {
                            if (collection[i].Id == this[j].Id)
                                alreadyExists = true;
                        }
                        if (alreadyExists == false)
                            this.Add(collection[i]);
                    }
                }
            }));
        }
    }
}
