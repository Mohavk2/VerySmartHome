using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBulbColor.PluginApp
{
    public struct GroupDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<BulbDTO> Bulbs { get; set; }
    }
}
