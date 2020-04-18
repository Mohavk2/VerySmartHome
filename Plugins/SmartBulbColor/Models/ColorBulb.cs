using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using VerySmartHome.MainController;

namespace SmartBulbColor.Models
{
    enum Effect { Sudden = 0, Smooth = 1 }
    internal sealed class ColorBulb : Device
    {
        public const string DeviceType = "MiBulbColor";
        public const string SSDPMessage = ("M-SEARCH * HTTP/1.1\r\n" + "HOST: 239.255.255.250:1982\r\n" + "MAN: \"ssdp:discover\"\r\n" + "ST: wifi_bulb");

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
                var response = BulbResponse.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var responseLine in response)
                {
                    string[] responseParams = responseLine.Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);

                    if(responseParams.Length == 2)
                    {
                        KeyValuePair<string, string> pair = new KeyValuePair<string, string>(responseParams[0].Trim(), responseParams[1].Trim());
                        switch (pair.Key)
                        {
                            case "Location":
                                Uri url = new Uri(pair.Value);
                                Ip = url.Host;
                                Port = url.Port;
                                break;
                            case "id": Id = Convert.ToInt32(pair.Value, 16); break;
                            case "model": Model = pair.Value; break;
                            case "fw_ver": FwVer = int.Parse(pair.Value); break;
                            case "power": IsPowered = pair.Value == "on" ? true : false; break;
                            case "bright": Brightness = int.Parse(pair.Value); break;
                            case "color_mode": ColorMode = int.Parse(pair.Value); break;
                            case "ct": ColorTemperature = int.Parse(pair.Value); break;
                            case "rgb": Rgb = int.Parse(pair.Value); break;
                            case "hue": Hue = int.Parse(pair.Value); break;
                            case "sat": Saturation = int.Parse(pair.Value); break;
                            case "name": Name = pair.Value; break;
                        }
                    }
                }
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
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == "")
                {
                    value = "Bulb";
                }
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        private string _name = "";
        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }
        private int _id = 0;
        public string Ip
        {
            get { return _ip; }
            set
            {
                _ip = value;
                OnPropertyChanged("Ip");
            }
        }
        private string _ip = "";
        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged("Port");
            }
        }
        private int _port = 0;
        public bool IsPowered
        {
            get { return _isPowered; }
            set
            {
                if (value != _isPowered)
                {
                    _isPowered = value;
                    OnPropertyChanged("IsPowered");
                }
            }
        }
        private bool _isPowered = false;
        public string StateIconPath
        {
            get { return _stateIconPath; }
            set
            {
                _stateIconPath = value;
                OnPropertyChanged("StateIconPath");
            }
        }
        private string _stateIconPath = "";
        public bool IsOnline
        {
            get { return _isOnline; }
            private set
            {
                _isOnline = value;
                OnPropertyChanged("IsMusicModeEnabled");
            }
        }
        private bool _isOnline = true;
        public string Model
        {
            get { return _model; }
            set
            {
                _model = value;
                OnPropertyChanged("Model");
            }
        }
        private string _model = "";
        public int FwVer
        {
            get { return _fwVer; }
            set
            {
                _fwVer = value;
                OnPropertyChanged("FwVer");
            }
        }
        private int _fwVer = 0;
        public int Brightness
        {
            get { return _brightness; }
            set
            {
                _brightness = value;
                OnPropertyChanged("Brightness");
            }
        }
        private int _brightness = 0;
        public int ColorMode
        {
            get { return _colorMode; }
            set
            {
                _colorMode = value;
                OnPropertyChanged("ColorMode");
            }
        }
        private int _colorMode = 0;
        public int ColorTemperature
        {
            get { return _colorTemperature; }
            set
            {
                _colorTemperature = value;
                OnPropertyChanged("ColorTemperature");
            }
        }
        private int _colorTemperature = 0;
        public int Rgb
        {
            get { return _rgb; }
            set
            {
                _rgb = value;
                OnPropertyChanged("Rgb");
            }
        }
        private int _rgb = 0;
        public int Hue
        {
            get { return _hue; }
            set
            {
                _hue = value;
                OnPropertyChanged("_hue");
            }
        }
        private int _hue = 0;
        public int Saturation
        {
            get { return _saturation; }
            set
            {
                _saturation = value;
                OnPropertyChanged("Saturation");
            }
        }
        private int _saturation = 0;

        private void SendCommand(string command)
        {
            lock (CommandLocker)
            {
                if (Client == null)
                {
                    ReconnectMusicMode();
                }
                bool success = true;
                byte[] comandBuffer = Encoding.UTF8.GetBytes(command);
                try
                {
                    Client?.Send(comandBuffer);
                }
                catch //TODO: Add logging
                {
                    success = false;
                }
                if (success == true)
                    return;
                else
                    WaitForOnline();
            }
        }
        private bool TrySendStraightCommand(string command)
        {
            lock (CommandLocker)
            {
                int attemps = 0;
                do
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
                                attemps++;
                        }
                        attemps++;
                    }
                } while (attemps < 3);
                DisconnectMusicMode();
                IsOnline = false;
                return false;
            }
        }
        private void ReconnectMusicMode()
        {
            bool success = false;
            try
            {
                DisconnectMusicMode();
                success = SendRequestForConnection();
                if (success)
                    Client = TcpServer.Accept();
            }
            catch //TODO: Add logging
            {
                success = false;
            }
            IsOnline = success;
        }
        public override void DisconnectMusicMode()
        {
            Client?.Dispose();
            Client = null;
        }
        private bool SendRequestForConnection()
        {
            var command = $"{{\"id\":{Id},\"method\":\"set_music\",\"params\":[1, \"{LocalIP}\", {LocalPort}]}}\r\n";
            return TrySendStraightCommand(command);
        }

        private void WaitForOnline()
        {
            //TODO: add 
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
                DisconnectMusicMode();
            }
            else
            {
                SetPower(true, Effect.Smooth, 500);
                ReconnectMusicMode();
            }
        }
        public bool SetPower(bool power, Effect effect, int duration)
        {
            var pow = power ? "on" : "off";

            var dur = (duration < 30) ? 30 : duration;

            string eff = (effect == Effect.Smooth) ? "smooth" : "sudden";

            var command = $"{{\"id\":{this.Id},\"method\":\"set_power\",\"params\":[\"{pow}\", \"{eff}\", {dur}]}}\r\n";

            if (TrySendStraightCommand(command))
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
            if (TrySendStraightCommand(command))
            {
                IsPowered = true;
            }
        }
        public void SetSceneHSV(int hue, float saturation, float value)
        {
            string command = $"{{\"id\":{Id},\"method\":\"set_scene\",\"params\":[\"hsv\", {hue}, {saturation}, {value}]}}\r\n";
            SendCommand(command);
            IsPowered = true;
        }
        public void SetNormalLight(int colorTemperature, int brightness)
        {
            var command = $"{{\"id\":{this.Id},\"method\":\"set_scene\",\"params\":[\"ct\", {colorTemperature}, {brightness}]}}\r\n";
            if (TrySendStraightCommand(command))
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
