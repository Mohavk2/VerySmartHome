using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using VerySmartHome.MainController;
using System.Threading;

namespace SmartBulbColor.Models
{
    enum Effect { Sudden = 0, Smooth = 1 }
    internal sealed class ColorBulb : Device
    {
        public const string DeviceType = "MiBulbColor";
        public const string SSDPMessage = ("M-SEARCH * HTTP/1.1\r\n"+"HOST: 239.255.255.250:1982\r\n"+"MAN: \"ssdp:discover\"\r\n"+"ST: wifi_bulb");
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

        private const int ReconnectDelay = 160;
        private const int ReconnectionAttemptsCount = 3;

        readonly static IPAddress LocalIP = DeviceDiscoverer.GetLocalIP();
        readonly static int LocalPort = 19446;
        readonly static Socket TcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private Socket Client = null;

        Object CommandLocker = new Object();

        static ColorBulb()
        {
            TcpServer.Bind(new IPEndPoint(LocalIP, LocalPort));
            TcpServer.Listen(10);
        }

        public ColorBulb(string BulbResponse)
        {
            Parse(BulbResponse);
        }
        private void Parse(string BulbResponse)
        {
            if (BulbResponse.Length != 0)
            {
                var response = BulbResponse.Split(new[] { "\r\n" }, StringSplitOptions.None);

                var ipPort = response[4].Substring(LOCATION.Length);
                var temp = ipPort.Split(':');
                Ip = temp[0];
                Port = int.Parse(temp[1]);

                Id = Convert.ToInt32(response[6].Substring(ID.Length), 16);
                Model = response[7].Substring(MODEL.Length);
                FwVer = int.Parse(response[8].Substring(FW_VER.Length));
                IsPowered = response[10].Substring(POWER.Length) == "on" ? true : false;
                Brightness = int.Parse(response[11].Substring(BRIGHT.Length));
                ColorMode = int.Parse(response[12].Substring(COLOR_MODE.Length));
                ColorTemperature = int.Parse(response[13].Substring(CT.Length));
                Rgb = int.Parse(response[14].Substring(RGB.Length));
                Hue = int.Parse(response[15].Substring(HUE.Length));
                Saturation = int.Parse(response[16].Substring(SAT.Length));
                Name = response[17].Substring(NAME.Length);
            }
        }
        public override int GetId()
        {
            return Id;
        }

        public override string GetName()
        {
            return Name;
        }

        public override string GetIP()
        {
            return Ip;
        }

        public override int GetPort()
        {
            return Port;
        }
        private string _name = "";
        public string Name 
        {
            get { return _name; } 
            set 
            {
                if(value == "")
                {
                    value = "Bulb";
                }
                if(_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            } 
        }
        private int _id = 0;
        public int Id 
        {
            get { return _id; } 
            set 
            {
                _id = value;
                OnPropertyChanged("Id");
            } 
        }
        private string _ip = "";
        public string Ip
        {
            get { return _ip; }
            set
            {
                _ip = value;
                OnPropertyChanged("Ip");
            }
        }
        private int _port = 0;
        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged("Port");
            }
        }
        private bool _isPowered = false;
        public bool IsPowered 
        {
            get { return _isPowered; }
            set
            {
                _isPowered = value;
                OnPropertyChanged("IsPowered");
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
        private bool _isMusicModeEnabled = false;
        public bool IsConnected
        {
            get { return _isMusicModeEnabled; }
            private set
            {
                _isMusicModeEnabled = value;
                OnPropertyChanged("IsMusicModeEnabled");
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
        public void Reconnect()
        {
            Client?.Dispose();

            int attempts = 0;
            do
            {
                try
                {
                    SendRequestForConnection();
                    Client = TcpServer.Accept();
                    IsConnected = true;
                    return;
                }
                catch (Exception)
                {
                    attempts++;
                }
                Thread.Sleep(ReconnectDelay);

            } while (attempts <= ReconnectionAttemptsCount);

            OnNoResponseFromDevice();
        }
        public override void Disconnect()
        {
            if (Client != null) Client.Dispose();
        }
        private void SendCommand(string command)
        {
            lock (CommandLocker)
            {
                byte[] comandBuffer = Encoding.UTF8.GetBytes(command);

                var connectionLost = false;

                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        if (Client == null || connectionLost)
                        {
                            Reconnect();
                        }
                        Client.Send(comandBuffer);

                        if (connectionLost == false)
                        {
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        connectionLost = true;                       
                    }
                }
            }
        }
        private bool SendStraightCommandAndConfirm(string command)
        {
            var attemts = 0;
            do
            {
                try
                {
                    var result = TrySendStraightCommand(command);
                    return result;
                }
                catch
                {
                    attemts++;
                }
            } while (attemts <= ReconnectionAttemptsCount);
            OnNoResponseFromDevice();
            return false;
        }
        private bool TrySendStraightCommand(string command)
        {
            lock (CommandLocker)
            {
                byte[] comandBuffer = Encoding.UTF8.GetBytes(command);
                using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    IAsyncResult result = client.BeginConnect(this.Ip, this.Port, null, null);

                    bool success = result.AsyncWaitHandle.WaitOne(1000, true);

                    if (client.Connected)
                    {
                        client.EndConnect(result);
                        client.Send(comandBuffer);

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
        }
        private bool SendRequestForConnection()
        {
            var command = $"{{\"id\":{Id},\"method\":\"set_music\",\"params\":[1, \"{LocalIP}\", {LocalPort}]}}\r\n";
            return TrySendStraightCommand(command);
        }
        public void SetName(string name)
        {
            var command = $"{{\"id\":{this.Id},\"method\":\"set_name\",\"params\":[\"{name}\"]}}\r\n";
            SendCommand(command);
            Name = name;
        }
        public void TogglePower()
        {
            if (IsPowered)
            {
                SetPower(false, Effect.Smooth, 500);
            }
            else
            {
                SetPower(true, Effect.Smooth, 500);
                Reconnect();
            }
        }
        public bool SetPower(bool power, Effect effect, int duration)
        {
            var pow = power ? "on" : "off";

            var dur = (duration < 30) ? 30 : duration;

            string eff = (effect == Effect.Smooth) ? "smooth" : "sudden";

            var command = $"{{\"id\":{this.Id},\"method\":\"set_power\",\"params\":[\"{pow}\", \"{eff}\", {dur}]}}\r\n";

            if (SendStraightCommandAndConfirm(command))
            {
                IsPowered = power;
                return true;
            }
            else
            {
                return false;
            }
        }
        public void SetColorMode(int value)
        {
            var command = $"{{\"id\":{this.Id},\"method\":\"set_power\",\"params\":[\"on\", \"smooth\", 30, {value}]}}\r\n";
            if (SendStraightCommandAndConfirm(command))
            {
                IsPowered = true;
            }
        }
        public void SetSceneHSV(int hue, float saturation, float value)
        {
            string command = $"{{\"id\":{Id},\"method\":\"set_scene\",\"params\":[\"hsv\", {hue}, {saturation}, {value}]}}\r\n";
            SendCommand(command);
        }
        public void SetNormalLight(int colorTemperature, int brightness)
        {
            var command = $"{{\"id\":{this.Id},\"method\":\"set_scene\",\"params\":[\"ct\", {colorTemperature}, {brightness}]}}\r\n";
            if(SendStraightCommandAndConfirm(command))
            {
                IsPowered = true;
            }
        }
        public override string ToString()
        {
            if (Name == "")
                return $"ID: {Id}";
            else
                return Name;
        }
        public override void Dispose()
        {
            Client?.Dispose();
        }
    }
}
