using SmartBulbColor.Domain;
using SmartBulbColor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartBulbColor.PluginApp
{
    internal class BulbRepository
    {
        public delegate void BulbEventHandler(ColorBulbProxy bulb);

        Dictionary<string, ColorBulbProxy> AllBulbs = new Dictionary<string, ColorBulbProxy>();

        ColorBulbProxy CachedBulb = null;

        Dictionary<string, BulbGroup> UserGroups = new Dictionary<string, BulbGroup>();

        object Locker = new object();

        public int Count
        {
            get
            {
                int count;
                lock (Locker)
                {
                    count = AllBulbs.Count;
                }
                return count;
            }
        }

        public BulbRepository()
        {

        }

        public List<string> GetBulbIds()
        {
            lock (Locker)
            {           
                return AllBulbs.Keys.ToList<string>();
            }
        }

        public void AddBulb(ColorBulbProxy bulb)
        {
            lock (Locker)
            {
                AllBulbs.Add(bulb.Id, bulb);
            }
        }

        public ColorBulbProxy RemoveBulb(string bulbId)
        {
            ColorBulbProxy tempBulb = null;
            lock (Locker)
            {
                tempBulb = AllBulbs[bulbId];
                AllBulbs.Remove(bulbId);
            }
            return tempBulb;
        }

        public ColorBulbProxy GetBulb(string bulbId)
        {
            ColorBulbProxy bulbToReturn = null;

            lock (Locker)
            {

                if (CachedBulb != null && CachedBulb.Id == bulbId)
                {
                    bulbToReturn = CachedBulb;
                }
                else
                {
                    CachedBulb = AllBulbs[bulbId];
                    bulbToReturn = CachedBulb;
                }
            }
            if (bulbToReturn == null)
                throw new Exception($"There is no bulb in repository that has id {bulbId}");
            else
                return bulbToReturn;
        }

        public List<ColorBulbProxy> GetAllBulbs()
        {
            List<ColorBulbProxy> tempBulbs = null;
            lock(Locker)
            {
                tempBulbs = AllBulbs.Values.ToList<ColorBulbProxy>();
            }
            if (tempBulbs == null)
                throw new Exception("There is no bulbs in the repository yet!");
            else
                return tempBulbs;
        }

        public List<string> GetGroupNames()
        {
            List<string> groupNames = new List<string>();
            lock(Locker)
            {
                foreach(var groupName in UserGroups.Keys)
                {
                    groupNames.Add(groupName);
                }
            }
            return groupNames;
        }

        public BulbGroup GetGroup(string groupId)
        {
            lock (Locker)
            {
                if (UserGroups.ContainsKey(groupId))
                {
                    return UserGroups[groupId];
                }
                else
                    return new BulbGroup("Empty");
            }
        }

        public List<BulbGroup> GetGroups()
        {
            List<BulbGroup> groups = new List<BulbGroup>();
            lock (Locker)
            {
                foreach (var group in UserGroups)
                {
                    groups.Add(group.Value);
                }
            }
            return groups;
        }

        public BulbGroup AddGroup(BulbGroup group)
        {
            BulbGroup tempGroup;
            lock (Locker)
            {
                UserGroups.Add(group.Id, group);
                tempGroup = UserGroups[group.Id];
            }
            return tempGroup;
        }

        public BulbGroup RenameGroup(string groupId, string newGroupName)
        {
            BulbGroup tempGroup;
            lock (Locker)
            {
                UserGroups[groupId].Rename(newGroupName);
                tempGroup = UserGroups[groupId];
            }
            return tempGroup;
        }

        public BulbGroup RemoveGroup(string groupId)
        {
            BulbGroup tempGroup;
            lock (Locker)
            {
                tempGroup = UserGroups[groupId];
                UserGroups.Remove(groupId);
            }
            return tempGroup;
        }

        public BulbGroup AddBulbToGroup(string groupId, string bulbId)
        {
            BulbGroup tempGroup;
            ColorBulbProxy tempBulb;
            lock (Locker)
            {
                tempBulb = GetBulb(bulbId);
                if (UserGroups.ContainsKey(groupId))
                {
                    UserGroups[groupId].Add(tempBulb);
                    tempGroup = UserGroups[groupId];
                }
                else
                {
                    throw new Exception($"There is no group with id: {groupId} in the repository!");
                }
            }
            return tempGroup;
        }

        public BulbGroup RemoveBulbFromGroup(string groupId, string bulbId)
        {
            BulbGroup tempGroup;
            ColorBulbProxy tempBulb;
            lock (Locker)
            {
                tempBulb = GetBulb(bulbId);
                if (UserGroups.ContainsKey(groupId))
                {
                    UserGroups[groupId].Remove(tempBulb);
                    tempGroup = UserGroups[groupId];
                }
                else
                    return tempGroup = UserGroups[groupId];
            }
            return tempGroup;
        }

        public void Dispose()
        {
            foreach (var bulb in AllBulbs)
            {
                bulb.Value.Dispose();
            }
        }
    }
}
