using System.Collections.Generic;

namespace VerySmartHome.MainController
{
    class DeviceByIdNotifyList : List<Device>
    {
        public delegate void DeviceInOutHandler(Device device);
        public event DeviceInOutHandler DeviceAdded;
        public event DeviceInOutHandler DeviceRemoved;
        public DeviceByIdNotifyList() : base() { }
        public DeviceByIdNotifyList(IEnumerable<Device> collection) : base(collection) { }
        /// <summary>
        /// Check if the collection contains an item with the same Id
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public bool ContainsId(Device device)
        {
            if (device != null)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].GetId() == device.GetId())
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private bool ContainsId(IList<Device> devices, Device device)
        {
            if (devices != null && device != null)
            {
                for (int i = 0; i < devices.Count; i++)
                {
                    if (devices[i].GetId() == device.GetId())
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
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].GetId() == device.GetId())
                        RemoveAt(i);
                }
            }
        }
        public void RemoveBunch(List<Device> devices)
        {
            if (devices != null)
            {
                for (int i = 0; i < devices.Count; i++)
                {
                    if (ContainsId(this, devices[i]))
                        RemoveById(devices[i]);
                }
            }
        }
        /// <summary>
        /// Adds item. Compares by Id. Notifies.
        /// </summary>
        /// <param name="device">Item to add</param>
        public void AddNewAndNotify(Device device)
        {
            if (!ContainsId(device))
            {
                Add(device);
                OnDeviceAdded(device);
            }
        }
        /// <summary>
        /// Removes item. Compares by Id. Notifies.
        /// </summary>
        /// <param name="device">Item to remove</param>
        public void RemoveObsoleteAndNotify(Device device)
        {
            for(int i = 0; i < this.Count; i++)
            {
                if(this[i].GetId() == device.GetId())
                {
                    var toRemove = this[i];
                    RemoveAt(i);
                    OnDeviceRemoved(toRemove);
                }
            }
        }
        /// <summary>
        /// Adds only new items in the collection from a given list. Compares by Id. Notifies.
        /// </summary>
        /// <param name="devices">Items to compare</param>
        public void AddNewAndNotify(List<Device> devices)
        {
            for (int i = 0; i < devices.Count; i++)
            {
                if (!ContainsId(devices[i]))
                {
                    Add(devices[i]);
                    OnDeviceAdded(devices[i]);
                }
            }
        }
        /// <summary>
        /// Remove obsolete items in compare to a given list. Compares by Id. Notifies.
        /// </summary>
        /// <param name="devices">Items to compare</param>
        public void RemoveObsoleteAndNotify(List<Device> devices)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (!ContainsId(devices, this[i]))
                {
                    var obsolete = this[i];
                    RemoveAt(i);
                    OnDeviceRemoved(obsolete);
                }
            }
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
                    if (!Contains(devices[i]))
                    {
                        Add(devices[i]);
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
                for (int i = 0; i < this.Count; i++)
                {
                    if (!ContainsId(devices, this[i]))
                    {
                        RemoveAt(i);
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

        void OnDeviceAdded(Device device)
        {
            DeviceAdded?.Invoke(device);
        }
        void OnDeviceRemoved(Device device)
        {
            DeviceRemoved?.Invoke(device);
        }
    }
}
