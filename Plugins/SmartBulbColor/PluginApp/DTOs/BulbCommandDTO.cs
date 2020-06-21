using SmartBulbColor.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SmartBulbColor.PluginApp
{
    public class BulbCommandDTO
    {
        public TargetType Target;
        public CommandType Mode;
        public string TargetId;
        public string Method;
        public ArrayList Parameters;
    }
}
