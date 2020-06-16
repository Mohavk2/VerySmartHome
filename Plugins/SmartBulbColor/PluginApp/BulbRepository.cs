using SmartBulbColor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartBulbColor.PluginApp
{
    internal class BulbRepository
    {
        public delegate void BulbAddedHandler(ColorBulb bulb);
        public event BulbAddedHandler NewDeviceAdded;

        Dictionary<string, List<ColorBulb>> UserGroups = new Dictionary<string, List<ColorBulb>>();
        List<ColorBulb> AllBulbs = new List<ColorBulb>();

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
        public List<ColorBulb> GetDevices()
        {
            return new List<ColorBulb>(AllBulbs);
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
        public void AddDevice(ColorBulb bulb)
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
                UserGroups.Add(Name, new List<ColorBulb>());
            }
        }
        public void AddDeviceToGroup(string Name, ColorBulb device)
        {
            lock(Locker)
            {
                if (UserGroups.ContainsKey(Name))
                {
                    UserGroups[Name].Add(device);
                }
                else
                {
                    UserGroups.Add(Name, new List<ColorBulb>());
                    UserGroups[Name].Add(device);
                }
            }
        }
        public void RemoveDeviceFromGroup(string Name, ColorBulb bulb)
        {
            lock (Locker)
            {
                if (UserGroups.ContainsKey(Name))
                {
                    UserGroups[Name].Remove(bulb);
                }
            }
        }
        public List<ColorBulb> GetGroup(string Name)
        {
            lock (Locker)
            {
                if (UserGroups.ContainsKey(Name))
                {
                    return UserGroups[Name];
                }
                else
                    return new List<ColorBulb>();
            }
        }
        private void OnDeviceAdded(ColorBulb bulb)
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
