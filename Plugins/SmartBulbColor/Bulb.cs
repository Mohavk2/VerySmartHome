using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerySmartHome.MainController;

namespace SmartBulbColor
{
    internal sealed class Bulb : Device
    {
        public override int ID {get; set;}
        public override string Name { get; set; }
        public override int IP { get; set; }
        public override int Port { get; set; }
        public override bool IsConnected { get; set; }
        public override bool IsPowered { get; set; }

        public string Model { get; set; }
        public int FwVer { get; set; }
        public int Brightness { get; set; }
        public int ColorMode { get; set; }
        public int ColorTemperature { get; set; }
        public int RGB { get; set; }
        public int Hue { get; set; }
        public int Saturation { get; set; }

    }
}
