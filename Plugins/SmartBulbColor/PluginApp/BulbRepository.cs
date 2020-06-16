using SmartBulbColor.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartBulbColor.PluginApp
{
    internal class BulbRepository
    {
        public delegate void BulbAddedHandler(ColorBulbProxy bulb);
        public event BulbAddedHandler NewDeviceAdded;

        Dictionary<string, List<ColorBulbProxy>> UserGroups = new Dictionary<string, List<ColorBulbProxy>>();
        List<ColorBulbProxy> AllBulbs = new List<ColorBulbProxy>();

        object Locker = new object();

        public BulbRepository()
        {

        }
        public List<int> GetDeviceIds()
        {
            lock(Locker)
            {
                List<int> ids = new List<int>();
                foreach(var bulb in AllBulbs)
                {
                    ids.Add(bulb.Id);
                }
                return ids.ToList<int>();
            }
        }
        public List<ColorBulbProxy> GetDevices()
        {
            return new List<ColorBulbProxy>(AllBulbs);
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
        public void AddDevice(ColorBulbProxy bulb)
        {
            lock (Locker)
            {
                AllBulbs.Add(bulb);
                OnDeviceAdded(bulb);
            }
        }
        public void AddGroup(string Name)
        {
            lock(Locker)
            {
                UserGroups.Add(Name, new List<ColorBulbProxy>());
            }
        }
        public void AddDeviceToGroup(string Name, ColorBulbProxy device)
        {
            lock(Locker)
            {
                if (UserGroups.ContainsKey(Name))
                {
                    UserGroups[Name].Add(device);
                }
                else
                {
                    UserGroups.Add(Name, new List<ColorBulbProxy>());
                    UserGroups[Name].Add(device);
                }
            }
        }
        public void RemoveDeviceFromGroup(string Name, ColorBulbProxy bulb)
        {
            lock (Locker)
            {
                if (UserGroups.ContainsKey(Name))
                {
                    UserGroups[Name].Remove(bulb);
                }
            }
        }
        public List<ColorBulbProxy> GetGroup(string Name)
        {
            lock (Locker)
            {
                if (UserGroups.ContainsKey(Name))
                {
                    return UserGroups[Name];
                }
                else
                    return new List<ColorBulbProxy>();
            }
        }
        private void OnDeviceAdded(ColorBulbProxy bulb)
        {
            NewDeviceAdded?.Invoke(bulb);
        }

        public void Dispose()
        {
            foreach(var bulb in AllBulbs)
            {
                bulb.Dispose();
            }
        }
    }
}
