using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerySmartHome.MainController
{
    class GroupManager
    {
        CollectionThreadSafe<Device> AllDevices;
        Dictionary<string, CollectionThreadSafe<Device>> UserGroups;

        object Locker = new object();

        public GroupManager()
        {
            AllDevices = new CollectionThreadSafe<Device>();
        }
        public GroupManager(CollectionThreadSafe<Device> allDevices)
        {
            if (allDevices != null)
                AllDevices = allDevices;
            else
                throw new Exception("Collection can't be empty!");
        }
        public void AddDevice(Device device)
        {
            AllDevices.Add(device);
        }
        public void CreateGroup(string Name)
        {
            lock(Locker)
            {
                UserGroups.Add(Name, new CollectionThreadSafe<Device>());
            }
        }
        public void AddDeviceToGroup(string Name, Device device)
        {
            lock(Locker)
            {
                if (UserGroups.ContainsKey(Name))
                {
                    UserGroups[Name].Add(device);
                }
                else
                {
                    UserGroups.Add(Name, new CollectionThreadSafe<Device>());
                    UserGroups[Name].Add(device);
                }
            }
        }
        public void RemoveDeviceFromGroup(string Name, Device device)
        {
            lock (Locker)
            {
                if (UserGroups.ContainsKey(Name))
                {
                    UserGroups[Name].Remove(device);
                }
            }
        }
        public CollectionThreadSafe<Device> GetGroup(string Name)
        {
            lock (Locker)
            {
                if (UserGroups.ContainsKey(Name))
                {
                    return UserGroups[Name];
                }
                else
                    return new CollectionThreadSafe<Device>();
            }
        }
    }
}
