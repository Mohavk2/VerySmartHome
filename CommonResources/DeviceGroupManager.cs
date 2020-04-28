using System.Collections.Generic;

namespace CommonLibrary
{
    class BulbGroupManager<T> where T: Device
    {
        Dictionary<string, CollectionThreadSafe<T>> UserGroups;

        object Locker = new object();

        public BulbGroupManager()
        {
            UserGroups = new Dictionary<string, CollectionThreadSafe<T>>();
            var allDeviceGroup = new CollectionThreadSafe<T>();
            UserGroups.Add("ALL", allDeviceGroup);
        }
        public void CreateGroup(string Name)
        {
            lock(Locker)
            {
                UserGroups.Add(Name, new CollectionThreadSafe<T>());
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
                    UserGroups.Add(Name, new CollectionThreadSafe<T>());
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
        public CollectionThreadSafe<T> GetGroup(string Name)
        {
            lock (Locker)
            {
                if (UserGroups.ContainsKey(Name))
                {
                    return UserGroups[Name];
                }
                else
                    return new CollectionThreadSafe<T>();
            }
        }
    }
}
