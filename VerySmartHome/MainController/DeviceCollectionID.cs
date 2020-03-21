using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerySmartHome.MainController
{
    class DeviceCollectionID : ObservableCollection<Device>
    {
        public DeviceCollectionID() : base() { }
        public DeviceCollectionID(IEnumerable<Device> collection) : base(collection) { }
        /// <summary>
        /// Check if the collection contains an item with the same Id
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public bool ContainsId(Device device)
        {
            if (device != null)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].GetId() == device.GetId())
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Removes item that matches by Id. Using GetId() method.
        /// </summary>
        /// <param name="device">Item to remove</param>
        public void RemoveById(Device device)
        {
            if (device != null)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].GetId() == device.GetId())
                        Items.RemoveAt(i);
                }
            }
        }
        /// <summary>
        /// Adds only new items in the collection from a given list. Compares by Id. Returns added items.
        /// </summary>
        /// <param name="devices">Items to compare</param>
        /// <returns>Added items</returns>
        public List<Device> AddNewAndReturn(List<Device> devices)
        {
            var added = new List<Device>();
            if (devices == null)
                return added;
            for (int i = 0; i < devices.Count; i++)
            {
                if (!Items.Contains(devices[i]))
                {
                    added.Add(devices[i]);
                    Items.Add(devices[i]);
                }
            }
            return added;
        }
        /// <summary>
        /// Remove obsolete items in compare to a given list. Compares by Id. Returns removed items.
        /// </summary>
        /// <param name="devices">Items to compare</param>
        /// <returns>Removed items</returns>
        public List<Device> RemoveObsoleteAndReturn(List<Device> devices)
        {
            var obsolete = new List<Device>();
            if (devices == null)
                return (List<Device>)Items;
            for (int i = 0; i < Items.Count; i++)
            {
                if (!devices.Contains(Items[i]))
                {
                    obsolete.Add(Items[i]);
                    Items.RemoveAt(i);
                }
            }
            return obsolete;
        }
        /// <summary>
        /// Adds only new items in the collection from a given list. Compares by Id.
        /// </summary>
        /// <param name="devices">Items to compare</param>
        public void AddNew(List<Device> devices)
        {
            if (devices != null)
            {
                for (int i = 0; i < devices.Count; i++)
                {
                    if (!Items.Contains(devices[i]))
                    {
                        Items.Add(devices[i]);
                    }
                }
            }
        }
        /// <summary>
        /// Remove obsolete items in compare to a given list. Compares by Id.
        /// </summary>
        /// <param name="devices">Items to compare</param>
        public void RemoveObsolete(List<Device> devices)
        {
            if (devices != null)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    if (!devices.Contains(Items[i]))
                    {
                        Items.RemoveAt(i);
                    }
                }
            }
        }
        /// <summary>
        /// A items and adds new based on given list
        /// </summary>
        /// <param name="devices">Items to compare</param>
        public void Refresh(List<Device> devices)
        {
            RemoveObsolete(devices);
            AddNew(devices);
        }
    }
}
