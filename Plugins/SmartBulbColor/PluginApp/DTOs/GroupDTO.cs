using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBulbColor.PluginApp
{
    public struct GroupDTO
    {
        public string Id;
        public string Name;
        public Dictionary<string, BulbDTO> Bulbs;
    }
}
