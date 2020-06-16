using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBulbColor.PluginApp.DTOs
{
    public struct GroupDTO
    {
        public string Name;
        public List<BulbDTO> Bulbs;
    }
}
