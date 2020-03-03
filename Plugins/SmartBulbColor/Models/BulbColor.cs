using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerySmartHome.MainController;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace SmartBulbColor.Models
{
    enum Effect { Sudden = 0, Smooth = 1 }
    internal sealed class BulbColor : Device
    {
        private const string DeviceType = "MiBulbColor";
        private const string SSDPMessage = ("M-SEARCH * HTTP/1.1\r\n"+"HOST: 239.255.255.250:1982\r\n"+"MAN: \"ssdp:discover\"\r\n"+"ST: wifi_bulb");
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

        private static string LightOnImgPath = "/Models/LightON.png";
        private static string LightOffImgPath = "/Models/LightOFF.png";

        public static LinkedList<BulbColor> DiscoverBulbs()
        {
            return ParseBulbs(Discoverer.GetDeviceResponses());
        }
        private static SSDPDiscoverer Discoverer = new SSDPDiscoverer(SSDPMessage);
        private static LinkedList<BulbColor> ParseBulbs(List<string> responses)
        {
            LinkedList<BulbColor> bulbs = new LinkedList<BulbColor>();
            for (int i = 0; i < responses.Count; i++)
            {
                BulbColor bulb = Parse(responses[i]);
                if (bulb.Model == "color")
                {
                    bulb.IsOnline = true;
                    bulbs.AddLast(bulb);
                }
            }
            return bulbs;
        }
        private static BulbColor Parse(string bulbResponse)
        {
            var bulb = new BulbColor();

            if (bulbResponse.Length != 0)
            {
                var response = bulbResponse.Split(new[] { "\r\n" }, StringSplitOptions.None);

                var ipPort = response[4].Substring(LOCATION.Length);
                var temp = ipPort.Split(':');
                bulb.Ip = temp[0];
                bulb.Port = int.Parse(temp[1]);

                bulb.Id = Convert.ToInt32(response[6].Substring(ID.Length), 16);
                bulb.Model = response[7].Substring(MODEL.Length);
                bulb.FwVer = int.Parse(response[8].Substring(FW_VER.Length));
                bulb.IsPowered = response[10].Substring(POWER.Length) == "on" ? true : false;
                bulb.StateIconPath = bulb.IsPowered ? LightOnImgPath : LightOffImgPath;
                bulb.Brightness = int.Parse(response[11].Substring(BRIGHT.Length));
                bulb.ColorMode = int.Parse(response[12].Substring(COLOR_MODE.Length));
                bulb.ColorTemperature = int.Parse(response[13].Substring(CT.Length));
                bulb.Rgb = int.Parse(response[14].Substring(RGB.Length));
                bulb.Hue = int.Parse(response[15].Substring(HUE.Length));
                bulb.Saturation = int.Parse(response[16].Substring(SAT.Length));
                bulb.Name = response[17].Substring(NAME.Length);
                //if (bulb.Name == "")
                    //bulb.Name = "Bulb";
                if (bulb.Model == "color")
                    return bulb;
                else return new BulbColor();
            }
            return bulb;
        }

        private string _name = "";
        public override string Name 
        {
            get { return _name; } 
            set 
            {
                if(_name != value && value != "")
                {
                    var command = $"{{\"id\":{this.Id},\"method\":\"set_name\",\"params\":[\"{value}\"]}}\r\n";
                    byte[] commandBuffer = Encoding.UTF8.GetBytes(command);
                    if (IsMusicModeOn)
                    {
                        AcceptedClient.Send(commandBuffer);
                    }
                    SendCommand(commandBuffer);
                    _name = value;
                    OnPropertyChanged("Name");
                }
            } 
        }

        private int _id = 0;
        public override int Id 
        {
            get { return _id; } 
            set 
            {
                _id = value;
                OnPropertyChanged("Id");
            } 
        }

        private string _ip = "";
        public override string Ip
        {
            get { return _ip; }
            set
            {
                _ip = value;
                OnPropertyChanged("Ip");
            }
        }

        private int _port = 0;
        public override int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged("Port");
            }
        }

        private bool _isOnline = false;
        public override bool IsOnline
        {
            get { return _isOnline; }
            set
            {
                _isOnline = value;
                OnPropertyChanged("IsOnline");
            }
        }

        private bool _isPowered = false;
        public override bool IsPowered 
        {
            get { return _isPowered; }
            set
            {
                _isPowered = value;
                if (_isPowered)
                {
                    StateIconPath = LightOnImgPath;
                    OnPropertyChanged("IsPowered");
                }
                else
                {
                    StateIconPath = LightOffImgPath;
                    OnPropertyChanged("IsPowered");
                }
            }
        }

        private string _stateIconPath = "";
        public string StateIconPath
        {
            get { return _stateIconPath; }
            set
            {
                _stateIconPath = value;
                OnPropertyChanged("StateIconPath");
            }
        }

        private bool _isMusicModeOn = false;
        public bool IsMusicModeOn
        {
            get { return _isMusicModeOn; }
            set
            {
                _isMusicModeOn = value;
                OnPropertyChanged("IsMusicModeOn");
            }
        }

        private string _model = "";
        public string Model
        {
            get { return _model; }
            set
            {
                _model = value;
                OnPropertyChanged("Model");
            }
        }

        private int _fwVer = 0;
        public int FwVer
        {
            get { return _fwVer; }
            set
            {
                _fwVer = value;
                OnPropertyChanged("FwVer");
            }
        }

        private int _brightness = 0;
        public int Brightness
        {
            get { return _brightness; }
            set
            {
                _brightness = value;
                OnPropertyChanged("Brightness");
            }
        }

        private int _colorMode = 0;
        public int ColorMode 
        {
            get { return _colorMode; }
            set
            {
                _colorMode = value;
                OnPropertyChanged("ColorMode");
            }
        }

        private int _colorTemperature = 0;
        public int ColorTemperature
        {
            get { return _colorTemperature; }
            set
            {
                _colorTemperature = value;
                OnPropertyChanged("ColorTemperature");
            }
        }

        private int _rgb = 0;
        public int Rgb
        {
            get { return _rgb; }
            set
            {
                _rgb = value;
                OnPropertyChanged("Rgb");
            }
        }

        private int _hue = 0;
        public int Hue
        {
            get { return _hue; }
            set
            {
                _hue = value;
                OnPropertyChanged("_hue");
            }
        }

        private int _saturation = 0;
        public int Saturation
        {
            get { return _saturation; }
            set
            {
                _saturation = value;
                OnPropertyChanged("Saturation");
            }
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

        public Socket AcceptedClient;

        public void SendRequestForMusicMode(IPAddress localIP, int localPort)
        {
            var command = $"{{\"id\":{this.Id},\"method\":\"set_music\",\"params\":[1, \"{localIP}\", {localPort}]}}\r\n";
            byte[] commandBuffer = Encoding.UTF8.GetBytes(command);

            SendCommand(commandBuffer);
            IsPowered = true;
        }
        public void SetMusicModeClient(Socket acceptedClient)
        {
            if(acceptedClient != null)
            {
                AcceptedClient = acceptedClient;
            }
        }
        public void ConnectMusicModeClient(int localPort)
        {
            try
            {
                AcceptedClient.Connect(IPAddress.Parse(Ip), localPort);
                IsMusicModeOn = true;
            }
            catch(Exception e)
            {
                throw new Exception($"MusicMode Client can't connect to a remote device ID:{Id}");
            }
        }
        public void TurnMusicModeOFF()
        {
            if(IsMusicModeOn)
            {
                var command = $"{{\"id\":{this.Id},\"method\":\"set_music\",\"params\":[0]}}\r\n";
                byte[] commandBuffer = Encoding.UTF8.GetBytes(command);
                SendCommand(commandBuffer);
                IsMusicModeOn = false;
            }
        }
        public void SetColorMode(int value)
        {
            var command = $"{{\"id\":{this.Id},\"method\":\"set_power\",\"params\":[\"on\", \"smooth\", 30, {value}]}}\r\n";
            byte[] commandBuffer = Encoding.UTF8.GetBytes(command);
            if (IsMusicModeOn)
            {
                AcceptedClient.Send(commandBuffer);
                IsPowered = true;
            }
            else
            {
                SendCommand(commandBuffer);
                IsPowered = true;
            }
        }
        public bool SetPower(bool power, Effect effect, int duration )
        {
            var pow = power ? "on" : "off";

            if(duration < 30)
            {
                duration = 30;
            }
            string eff = "";

            if (effect == Effect.Smooth)
            {
                eff = "smooth";
            }
            if (effect == Effect.Sudden)
            {
                eff = "sudden";
            }
            var command = $"{{\"id\":{this.Id},\"method\":\"set_power\",\"params\":[\"{pow}\", \"{eff}\", {duration}]}}\r\n";
            byte[] commandBuffer = Encoding.UTF8.GetBytes(command);

            if(SendCommandAndConfirm(commandBuffer))
            {
                IsPowered = !IsPowered;
                return true;
            }
            else
            {
                return false;
            }
        }
        public void TogglePower()
        {
            if(IsPowered)
            {
                SetPower(false, Effect.Smooth, 500);
            }
            else
            {
                SetPower(true, Effect.Smooth, 500);
            }
        }
        public void TogglePowerNotSafe()
        {
            var command = $"{{\"id\":{this.Id},\"method\":\"toggle\",\"params\":[]}}\r\n";
            byte[] commandBuffer = Encoding.UTF8.GetBytes(command);
            if (IsMusicModeOn)
            {
                AcceptedClient.Send(commandBuffer);
            }
            SendCommand(commandBuffer);
            IsPowered = !IsPowered;
        }
        public void SetNormalLight(int colorTemperature, int brightness)
        {
            var command = $"{{\"id\":{this.Id},\"method\":\"set_scene\",\"params\":[\"ct\", {colorTemperature}, {brightness}]}}\r\n";
            byte[] commandBuffer = Encoding.UTF8.GetBytes(command);
            if (IsMusicModeOn)
            {
                AcceptedClient.Send(commandBuffer);
                IsPowered = true;
            }
            else
            {
                SendCommand(commandBuffer);
                IsPowered = true;
            }
        }
        public void SetScene()
        {

        }
        private void SendCommand(byte[] buffer)
        {
            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                IAsyncResult result = client.BeginConnect(this.Ip, this.Port, null, null);

                bool success = result.AsyncWaitHandle.WaitOne(1000, true);

                if (client.Connected)
                {
                    client.EndConnect(result);
                    client.Send(buffer);
                }
            }
        }
        private bool SendCommandAndConfirm(byte[] buffer)
        {
            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                IAsyncResult result = client.BeginConnect(this.Ip, this.Port, null, null);

                bool success = result.AsyncWaitHandle.WaitOne(1000, true);

                if (client.Connected)
                {
                    client.EndConnect(result);
                    client.Send(buffer);

                    byte[] recBuffer = new byte[100];
                    client.Receive(recBuffer);
                    string responce = Encoding.UTF8.GetString(recBuffer);

                    if (responce.Contains("\"ok\""))
                        return true;
                    else
                        return false;
                }
                throw new Exception($"the device id: {this.Id} doesn't respond");
            }
        }
        public override string ToString()
        {
            if (Name == "")
                return $"ID: {Id}";
            else
                return Name;
        }
        ~BulbColor()
        {
            AcceptedClient?.Dispose();
        }
    }
}
