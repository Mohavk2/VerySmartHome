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
    internal delegate void BulbRefreshedHandler(ColorBulbProxy bulb);

    internal sealed class ColorBulbProxy : IDisposable
    {
        public static event BulbRefreshedHandler BulbRefreshed;

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
                            case "id": Id = pair.Value; break;
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
        #region Properties
        public string Name { get; set; } = "";
        public string Id { get; set; } = string.Empty;
        public string Ip { get; set; } = "";
        public int Port { get; set; } = 0;
        private bool _isPowered = false;
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
                }
            }
        }
        public string BelongsToGroup { get; set; }= "";
        public bool IsOnline { get; set; } = true;
        public bool IsMusicModeOn { get; set; } = false;
        public string Model { get; set; } = "";
        public int FwVer { get; set; } = 0;
        public int Brightness { get; set; } = 0;
        public int ColorMode { get; set; } = 0;
        public int ColorTemperature { get; set; } = 0;
        public bool Flowing { get; set; } = false;
        public int Delayoff { get; set; } = 0;
        public int Rgb { get; set; } = 0;
        public int Hue { get; set; } = 0;
        public int Saturation { get; set; } = 0;
        #endregion

        public void PushCommand(BulbCommand command)
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

        void OnBulbRefreshed()
        {
            BulbRefreshed?.Invoke(this);
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
