using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBulbColor.PluginApp
{
    public struct BulbDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public string Ip { get; set; }
        public bool IsPowered { get; set; }
        public string BelongsToGroup { get; set; }
        public bool IsOnline { get; set; }
        public bool IsMusicModeOn { get; set; }
        public int FwVer { get; set; }
        public int Brightness { get; set; }
        public int ColorMode { get; set; }
        public int ColorTemperature { get; set; }
        public bool Flowing { get; set; }
        public int Delayoff { get; set; }
        public int Rgb { get; set; }
        public int Hue { get; set; }
        public int Saturation { get; set; }
    }
}
