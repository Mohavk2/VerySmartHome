using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerySmartHome.MainController;

namespace SmartBulbColor
{
    internal sealed class Bulb : Device
    {
        private const string LOCATION = "Location: yeelight://";
        private const string ID = "id: ";
        private const string MODEL = "model: ";
        private const string FW_VER = "fw_ver: ";
        private const string POWER = "power: ";
        private const string BRIGHT = "bright: ";
        private const string COLOR_MODE = "color_mode: ";
        private const string CT = "ct: ";
        private const string RGB = "rgb: ";
        private const string HUE = "hue: ";
        private const string SAT = "sat: ";
        private const string NAME = "name: ";

        public override int Id { get; set; } = 0;
        public override string Name { get; set; } = "";
        public override string Ip { get; set; } = "";
        public override int Port { get; set; } = 0;
        public override bool IsOnline { get; set; } = false;
        public override bool IsPowered { get; set; } = false;

        public string Model { get; set; } = "";
        public int FwVer { get; set; } = 0;
        public int Brightness { get; set; } = 0;
        public int ColorMode { get; set; } = 0;
        public int ColorTemperature { get; set; } = 0;
        public int Rgb { get; set; } = 0;
        public int Hue { get; set; } = 0;
        public int Saturation { get; set; } = 0;

        public Socket AcceptedClient;

        public static Bulb Parse( string bulbResponse)
        {
            var bulb = new Bulb();

            if (bulbResponse.Length != 0)
            {
                var response = bulbResponse.Split(new[] {"\r\n"}, StringSplitOptions.None);

                var ipPort = response[4].Substring(LOCATION.Length);
                var temp = ipPort.Split(':');
                bulb.Ip = temp[0];
                bulb.Port = int.Parse(temp[1]);

                bulb.Id = Convert.ToInt32(response[6].Substring(ID.Length), 16);
                bulb.Model = response[7].Substring(MODEL.Length);
                bulb.FwVer = int.Parse(response[8].Substring(FW_VER.Length));
                bulb.IsPowered = response[10].Substring(POWER.Length) == "on" ? true : false;
                bulb.Brightness = int.Parse(response[11].Substring(BRIGHT.Length));
                bulb.ColorMode = int.Parse(response[12].Substring(COLOR_MODE.Length));
                bulb.ColorTemperature = int.Parse(response[13].Substring(CT.Length));
                bulb.Rgb = int.Parse(response[14].Substring(RGB.Length));
                bulb.Hue = int.Parse(response[15].Substring(HUE.Length));
                bulb.Saturation = int.Parse(response[16].Substring(SAT.Length));
                bulb.Name = response[17].Substring(NAME.Length);
                if (bulb.Name == "")
                    bulb.Name = "Bulb";
                if (bulb.Model == "color")
                    return bulb;
                else return new Bulb();
            }
            return bulb;
        }
        public string GetReport()
        {
            const string divider = "\r\n";
            string report =
                LOCATION.ToUpper() + Ip + ":" + Port + divider +
                ID.ToUpper() + Id + divider +
                MODEL.ToUpper() + Model + divider +
                FW_VER.ToUpper() + FwVer + divider +
                POWER.ToUpper() + IsPowered + divider +
                BRIGHT.ToUpper() + Brightness + divider +
                COLOR_MODE.ToUpper() + ColorMode + divider +
                CT.ToUpper() + ColorTemperature + divider +
                RGB.ToUpper() + Rgb + divider +
                HUE.ToUpper() + Hue + divider +
                SAT.ToUpper() + Saturation + divider +
                NAME.ToUpper() + Name + divider;
            return report;
        }
        public override string ToString()
        {
            return $"{Name} ID: {Id}";
        }
        public override int GetHashCode()
        {
            return Id;
        }
        public override bool Equals(Object obj)
        {
            var bulb = obj as Bulb;
            return this.Id == bulb.Id;
        }
        //~Bulb()
        //{
        //    AcceptedClient?.Dispose();
        //}
    }
}
