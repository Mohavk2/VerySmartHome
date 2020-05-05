using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public class DeviceRepository<T> : IDisposable where T: Device
    {
        public delegate void DeviceAddedHandler(T device);
        public event DeviceAddedHandler NewDeviceAdded;

        Dictionary<string, List<T>> UserGroups = new Dictionary<string, List<T>>();
        List<T> AllDevices = new List<T>();

        object Locker = new object();

        public DeviceRepository()
        {

        }
        public int[] GetDeviceIds()
        {
            lock(Locker)
            {
                int[] ids = new int[AllDevices.Count];
                for(int i = 0; i < AllDevices.Count; i++)
                {
                    ids[i] = AllDevices[i].Id;
                }
                return ids;
            }
        }
        public List<T> GetDevices()
        {
            return new List<T>(AllDevices);
        }
        public string[] GetUserGroupNames()
        {
            lock (Locker)
            {
                string[] names = new string[UserGroups.Count];
                int i = 0;
                foreach(var group in UserGroups)
                {
                    names[i] = group.Key;
                    i++;
                }
                return names;
            }
        }
        public void AddDevice(T device)
        {
            lock (Locker)
            {
                AllDevices.Add(device);
                OnDeviceAdded(device);
            }
        }
        public void AddGroup(string Name)
        {
            lock(Locker)
            {
                UserGroups.Add(Name, new List<T>());
            }
        }
        public void AddDeviceToGroup(string Name, T device)
        {
            lock(Locker)
            {
                if (UserGroups.ContainsKey(Name))
                {
                    UserGroups[Name].Add(device);
                }
                else
                {
                    UserGroups.Add(Name, new List<T>());
                    UserGroups[Name].Add(device);
                }
            }
        }
        public void RemoveDeviceFromGroup(string Name, T device)
        {
            lock (Locker)
            {
                if (UserGroups.ContainsKey(Name))
                {
                    UserGroups[Name].Remove(device);
                }
            }
        }
        public List<T> GetGroup(string Name)
        {
            lock (Locker)
            {
                if (UserGroups.ContainsKey(Name))
                {
                    return UserGroups[Name];
                }
                else
                    return new List<T>();
            }
        }
        private void OnDeviceAdded(T device)
        {
            NewDeviceAdded?.Invoke(device);
        }

        public void Dispose()
        {
            foreach(var device in AllDevices)
            {
                device.Dispose();
            }
        }
    }
}
