using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using SmartBulbColor.Models;

namespace SmartBulbColor.ViewModels
{
    class BulbCollectionUIThreadSafe : ObservableCollection<Bulb>
    {
        static readonly Dispatcher CurrentDispatcher;
        static BulbCollectionUIThreadSafe()
        {
            CurrentDispatcher = Dispatcher.CurrentDispatcher;
        }
        public BulbCollectionUIThreadSafe() : base() { }
        public BulbCollectionUIThreadSafe(IEnumerable<Bulb> bulbs) : base(bulbs) { }
        public void AddSafe(Bulb newItem)
        {
            CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => { this.Add(newItem); }));
        }
        public void RemoveSafe(Bulb newItem)
        {
            CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => { this.Remove(newItem); }));
        }
        public void RefreshSafe(ObservableCollection<Bulb> collection)
        {
            CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
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
            }));
        }
    }
}
