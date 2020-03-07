using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VerySmartHome.MainController;
using SmartBulbColor.Tools;
using System.Collections.ObjectModel;
using System.Windows.Threading;

//MM - MusicMode

namespace SmartBulbColor.Models
{
    sealed class BulbController : DeviceController, IDisposable
    {
        public LinkedList<BulbColor> Bulbs { get; private set; } = new LinkedList<BulbColor>();
        public override int DeviceCount
        {
            get
            {
                return Bulbs.Count;
            }
        }

        public delegate void BulbCollectionNotifier();
        public event BulbCollectionNotifier BulbCollectionChanged;

        readonly Thread BulbsRefreshThread;
        readonly ManualResetEvent BulbsRefresherTrigger;
        object Locker = new object();

        public bool IsMusicModeON { get; private set; } = false;

        private readonly AmbientLightStreamer AmbientLight;
        public bool IsAmbientLightON { get; private set;} = false;

        public BulbController()
        {
            AmbientLight = new AmbientLightStreamer();
            BulbsRefreshThread = new Thread(new ThreadStart(RefreshingBulbs));
            BulbsRefreshThread.IsBackground = true;
            BulbsRefresherTrigger = new ManualResetEvent(true);
        }
        public override LinkedList<Device> GetDevices()
        {
            if (Bulbs.Count != 0)
            {
                LinkedList<Device> devices = new LinkedList<Device>(Bulbs);
                return devices;
            }
            else return new LinkedList<Device>();
        }
        public LinkedList<BulbColor> GetBulbs()
        {
            if (Bulbs.Count != 0)
            {
                return Bulbs;
            }
            else
            {
                ConnectBulbs_MusicMode();
                return Bulbs;
            }
        }
        public void ConnectBulbs_MusicMode()
        {
            lock(Locker)
            {
                try
                {
                    Bulbs = BulbColor.DiscoverBulbs();
                    MusicMode_ON();
                }
                catch (Exception NoResponseException)
                {
                    throw NoResponseException;
                }
            }
        }
        public void MusicMode_ON()
        {
            if (Bulbs.Count != 0)
            {
                try
                {
                    foreach (var bulb in Bulbs)
                    {
                        bulb.IsMusicModeEnabled = true;
                    }
                    IsMusicModeON = true;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message + "\r\nCan't turn Music Mode ON becouse of some connection problem");
                }
            }
            else throw new Exception("Can't turn Music Mode ON becouse there is no found bulbs yet");
        }
        void MusicMode_OFF()
        {
            if (Bulbs.Count != 0)
            {
                foreach (var bulb in Bulbs)
                {
                    bulb.IsMusicModeEnabled = false;
                }
                IsMusicModeON = false;
            }
            else throw new Exception("Can't turn Music Mode OFF becouse there is no found bulbs yet");
        }
        public void TogglePower(BulbColor bulb)
        {
            bulb.TogglePower();
        }
        public void NormalLight_ON()
        {
            if(IsAmbientLightON)
            {
                AmbientLight_OFF();
            }
            Thread.Sleep(500);
            if (Bulbs.Count != 0)
            {
                foreach (var bulb in Bulbs)
                {
                    bulb.SetNormalLight(5400, 100);
                }
            }
            else
            {
                try
                {
                    ConnectBulbs_MusicMode();
                }
                catch (Exception NoResponseException)
                {
                    throw NoResponseException;
                }
            }
        }
        public void AmbientLight_ON()
        {
            try
            {
                SetColorMode(2);
                MusicMode_ON();
                StopBulbsRefreshing();
                AmbientLight.SetBulbsForStreaming(Bulbs);
                AmbientLight.StartSreaming();
                IsAmbientLightON = true;
            }
            catch (Exception MusicModeFailedException)
            {
                throw MusicModeFailedException;
            }
        }
        public void AmbientLight_OFF()
        {
            if(IsAmbientLightON)
            {
                AmbientLight.StopStreaming();
                IsAmbientLightON = false;
                StartBulbsRefreshing();
            }
        }
        void ChangeColor_Bulb()
        {

        }

        /// <summary>
        /// Sets color mode
        /// </summary>
        /// <param name="value"> 1 - CT mode, 2 - RGB mode , 3 - HSV mode</param>
        void SetColorMode(int value)
        {
            if (value > 0 & value <= 3)
            {
                if (Bulbs.Count != 0)
                {
                    foreach (var bulb in Bulbs)
                    {
                        bulb.SetColorMode(value);
                    }
                }
                else throw new Exception("Can't turn Music Mode ON becouse there is no found bulbs yet");
            }
        }
        public List<String> GetDeviceReports()
        {
            List<String> reports = new List<string>();
            if (Bulbs.Count != 0)
            {
                foreach (BulbColor bulb in Bulbs)
                {
                    reports.Add(bulb.GetReport());
                }
                return reports;
            }
            else
            {
                reports.Add("No color bulbs found yet!!!");
                return reports;
            }
        }
        private void OnBulbConnecionChanged()
        {
            BulbCollectionChanged?.Invoke();
        }
        public void StartBulbsRefreshing()
        {
            if (BulbsRefreshThread.IsAlive)
            {
                BulbsRefresherTrigger.Set();
            }
            else
            {
                BulbsRefreshThread.Start();
            }
        }
        public void StopBulbsRefreshing()
        {
            BulbsRefresherTrigger.Reset();
        }
        private void RefreshingBulbs()
        {
            while (true)
            {
                BulbsRefresherTrigger.WaitOne(Timeout.Infinite);
                if(CheckBulbsOnlineChanged())
                {
                    ConnectBulbs_MusicMode();
                    OnBulbConnecionChanged();
                }
                Thread.Sleep(3000);
            }
        }
        private bool CheckBulbsOnlineChanged()
        {
            var foundBulbs = BulbColor.DiscoverBulbs();

            if (foundBulbs.Count == 0 && Bulbs.Count == 0)
            {
                return false;
            }
            else if(foundBulbs.Count != Bulbs.Count)
            {
                return true;
            }
            else
            {
                var bulbsIdSum = 0; 
                var foundBulbsIdSum = 0;
                foreach(var bulb in Bulbs)
                {
                    bulbsIdSum += bulb.Id;
                }
                foreach(var bulb in foundBulbs)
                {
                    foundBulbsIdSum += bulb.Id;
                }
                if (bulbsIdSum == foundBulbsIdSum)
                {
                    return false;
                }
                else return true;
            }
        }

        public void SetSceneHSV(BulbColor bulb, HSBColor color)
        {
            bulb.SetSceneHSV(color.Hue, color.Saturation, color.Brightness);
        }
        public void Dispose()
        {

        }
    }
}
