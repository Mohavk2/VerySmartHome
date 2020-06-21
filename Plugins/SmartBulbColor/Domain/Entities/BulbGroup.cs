using System.Collections;
using System.Collections.Generic;

namespace SmartBulbColor.Domain.Entities
{
    internal class BulbGroup
    {
        private static int CurrentId = 0;

        public string Id { get; private set; }
        public string Name { get; private set; }
        public Dictionary<string, ColorBulbProxy> Bulbs { get; } = new Dictionary<string, ColorBulbProxy>();

        public BulbGroup(string name)
        {
            Id = CurrentId.ToString();
            CurrentId++;
            Name = name;
        }
        public void Rename(string name)
        {
            Name = name;
        }
        public void Add(ColorBulbProxy bulb)
        {
            Bulbs.Add(bulb.Id, bulb);
        }
        public void AddBunch(List<ColorBulbProxy> bulbs)
        {
            foreach(var bulb in bulbs)
            {
                Bulbs.Add(bulb.Id, bulb);
            }
        }
        public void Remove(ColorBulbProxy bulb)
        {
            Bulbs.Remove(bulb.Id);
        }
        public void RemoveBunch(List<ColorBulbProxy> bulbs)
        {
            foreach(var bulb in bulbs)
            {
                Bulbs.Remove(bulb.Id);
            }
        }
        public IEnumerator GetEnumerator()
        {
            foreach(var bulb in Bulbs)
            {
                yield return bulb;
            }
        }
    }
}
