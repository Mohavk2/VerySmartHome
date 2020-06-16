using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CommonLibrary;
using System.Text.Json;
using System.ComponentModel;

namespace SmartBulbColor.Domain
{
    internal sealed class ColorBulbProxy : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        readonly static IPAddress LocalIP = HsdpDiscoverer.GetLocalIP();
        readonly static int LocalPort = 19446;

        readonly public static Socket TcpServer;
        private Socket Client = null;

        Object CommandLocker = new Object();

        static ColorBulbProxy()
        {
            TcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TcpServer.Bind(new IPEndPoint(LocalIP, LocalPort));
            TcpServer.Listen(10);
        }

        public ColorBulbProxy(string BulbResponse)
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

                    if (responseParams.Length == 2)
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
            private set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }
        private int _id = 0;
        public string Ip
        {
            get { return _ip; }
            private set
            {
                _ip = value;
                OnPropertyChanged("Ip");
            }
        }
        private string _ip = "";
        public int Port
        {
            get { return _port; }
            private set
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
                    if (_isPowered == true)
                        ReconnectMusicMode();
                    else
                        DisconnectMusicMode();
                    OnPropertyChanged("IsPowered");
                }
            }
        }
        private bool _isPowered = false;
        public string BelongToGroup
        {
            get { return _belongToGroup; }
            set
            {
                if (value != _belongToGroup)
                {
                    _belongToGroup = value;
                    OnPropertyChanged("BelongToGroup");
                }
            }
        }
        private string _belongToGroup = "";
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
                OnPropertyChanged("IsOnline");
            }
        }
        private bool _isOnline = true;
        public bool IsMusicModeOn
        {
            get { return _isMusicModeOn; }
            set
            {
                _isMusicModeOn = value;
                OnPropertyChanged("IsMusicModeOn");
            }
        }
        private bool _isMusicModeOn = false;
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
        public bool Flowing
        {
            get { return _flowing; }
            set
            {
                _flowing = value;
                OnPropertyChanged("Flowing");
            }
        }
        private bool _flowing = false;
        public int Delayoff
        {
            get { return _delayoff; }
            set
            {
                _delayoff = value;
                OnPropertyChanged("Delayoff");
            }
        }
        private int _delayoff = 0;
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

        public void ExecuteCommand(BulbCommand command)
        {
            lock (CommandLocker)
            {
                command.SetDeviceId(Id);
                var jsonCommand = JsonSerializer.Serialize(command, typeof(BulbCommand));
                string jsonResponse;
                switch (command.Mode)
                {
                    case CommandType.Stream:
                        SendCommand(jsonCommand);
                        break;
                    case CommandType.RefreshState:
                        jsonResponse = SendResponsiveCommand(jsonCommand);
                        if (jsonResponse.Contains("\"ok\""))
                            RefreshBulbState();
                        break;
                }
            }
        }
        private void SendCommand(string command)
        {

            if (Client == null)
            {
                ReconnectMusicMode();
            }
            bool success = true;
            byte[] comandBuffer = Encoding.UTF8.GetBytes(command + "\r\n");
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
        private string SendResponsiveCommand(string command)
        {
            int attemps = 0;
            do
            {
                byte[] comandBuffer = Encoding.UTF8.GetBytes(command + "\r\n");
                using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    IAsyncResult result = client.BeginConnect(this.Ip, this.Port, null, null);

                    bool success = result.AsyncWaitHandle.WaitOne(1000, true);

                    if (client.Connected)
                    {
                        client.EndConnect(result);
                        client.Send(comandBuffer);

                        byte[] recBuffer = new byte[100];
                        IAsyncResult recResult = client.BeginReceive(recBuffer, 0, recBuffer.Length, SocketFlags.None, null, null);

                        bool recSuccess = recResult.AsyncWaitHandle.WaitOne(2000);
                        if (recSuccess == false)
                            throw new Exception("Something went wrong while receiving response from the device #" + Id);
                        client.EndReceive(recResult);
                        string str = Encoding.UTF8.GetString(recBuffer);
                        var responce = str.Substring(0, str.IndexOf("\r\n"));
                        return responce;
                    }
                    attemps++;
                }
            } while (attemps < 3);
            DisconnectMusicMode();
            IsOnline = false;
            return string.Empty;
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
        public void DisconnectMusicMode()
        {
            Client?.Dispose();
            Client = null;
        }
        private bool SendRequestForConnection()
        {
            var musicModeCommand = BulbCommandBuilder.CreateSetMusicModeCommand(MusicModeAction.On, LocalIP.ToString(), LocalPort);
            musicModeCommand.SetDeviceId(Id);
            var jsonCommand = JsonSerializer.Serialize(musicModeCommand, typeof(BulbCommand));
            var response = SendResponsiveCommand(jsonCommand);
            if (response.Contains("\"ok\""))
                return true;
            else
                return false;
        }
        private void RefreshBulbState()
        {
            BulbProperties[] bulbProperties = new BulbProperties[]
            {
                BulbProperties.power,
                BulbProperties.bright,
                BulbProperties.ct,
                BulbProperties.rgb,
                BulbProperties.hue,
                BulbProperties.sat,
                BulbProperties.color_mode,
                BulbProperties.flowing,
                BulbProperties.delayoff,
                BulbProperties.music_on,
                BulbProperties.name,
            };
            var propertiesCommand = BulbCommandBuilder.CreateGetPropertiesCommand(bulbProperties);
            propertiesCommand.SetDeviceId(Id);
            var jsonCommand = JsonSerializer.Serialize(propertiesCommand, typeof(BulbCommand));
            var response = SendResponsiveCommand(jsonCommand);
            ParseJsonProperties(response);
        }
        private void ParseJsonProperties(string jsonResponse)
        {
            BulbResponse bulbJsonResponse = JsonSerializer.Deserialize<BulbResponse>(jsonResponse);

            if (bulbJsonResponse != null && bulbJsonResponse.result != null && bulbJsonResponse.result.Length != 0)
            {
                IsPowered = bulbJsonResponse.result[0] == "on" ? true : false;
                Brightness = int.Parse(bulbJsonResponse.result[1]);
                ColorTemperature = int.Parse(bulbJsonResponse.result[2]);
                Rgb = int.Parse(bulbJsonResponse.result[3]);
                Hue = int.Parse(bulbJsonResponse.result[4]);
                Saturation = int.Parse(bulbJsonResponse.result[5]);
                ColorMode = int.Parse(bulbJsonResponse.result[6]);
                Flowing = bulbJsonResponse.result[7] == "1" ? true : false;
                Delayoff = int.Parse(bulbJsonResponse.result[8]);
                IsMusicModeOn = bulbJsonResponse.result[9] == "1" ? true : false;
                Name = bulbJsonResponse.result[10];
            }
        }
        private void WaitForOnline()
        {
            //TODO: add 
        }

        void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public override string ToString()
        {
            if (Name == "")
                return $"ID: {Id}";
            else
                return Name;
        }
        public void Dispose()
        {
            DisconnectMusicMode();
        }
    }
}
