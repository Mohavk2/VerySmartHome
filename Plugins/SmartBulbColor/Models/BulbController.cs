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
        public delegate void BulbCollectionNotifier();
        public event BulbCollectionNotifier BulbCollectionChanged;

        public List<BulbColor> _bulbs = new List<BulbColor>();
        public List<BulbColor> Bulbs 
        {
            get 
            {
                return _bulbs;
            }
            private set
            {
                _bulbs = value;
                OnBulbCollectionChanged();
            } 
        }
        public List<BulbColor> BulbsForAmbientLight { get; private set; } = new List<BulbColor>();
        public override int DeviceCount
        {
            get
            {
                return Bulbs.Count;
            }
        }

        readonly static BulbDiscoverer Discoverer = new BulbDiscoverer(BulbColor.SSDPMessage);
        private readonly AmbientLightStreamer AmbientLight = new AmbientLightStreamer();

        public bool IsMusicModeON { get; private set; } = false;

        public bool IsAmbientLightON { get; private set;} = false;

        public BulbController()
        {
            Discoverer.DeviceCollectionChenged += RefreshBulbCollection;
            Discoverer.StartDiscover();
        }
        public override List<IDevice> GetDevices()
        {
            if (Bulbs.Count != 0)
            {
                List<IDevice> devices = new List<IDevice>(Bulbs);
                return devices;
            }
            else return new List<IDevice>();
        }
        public List<BulbColor> GetBulbs()
        {
            if (Bulbs.Count != 0)
            {
                return Bulbs;
            }
            else
            {
                DiscoverBulbs();
                return Bulbs;
            }
        }
        public void DiscoverBulbs()
        {
            try
            {
                Bulbs = Discoverer.DiscoverDevices().Cast<BulbColor>().ToList();
            }
            catch (Exception NoResponseException)
            {
                throw NoResponseException;
            }
        }
        public void StartBulbsRefreshing()
        {
            Discoverer.StartDiscover();
        }
        public void TogglePower(BulbColor bulb)
        {
            bulb.TogglePower();
        }
        public void NormalLight_ON()
        {

            AmbientLight_OFF();

            Thread.Sleep(500);
            if (Bulbs.Count != 0)
            {
                foreach (var bulb in Bulbs)
                {
                    bulb.SetNormalLight(5400, 100);
                    Console.WriteLine("Norm Light is On");
                }
            }
        }
        public void AmbientLight_ON()
        {
            try
            {
                SetColorMode(2);
                Discoverer.StopDiscover();
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
            AmbientLight.StopStreaming();
            IsAmbientLightON = false;
            Discoverer.StartDiscover();
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
        void RefreshBulbCollection(List<IDevice> foundBulbs)
        {
            Bulbs = foundBulbs.Cast<BulbColor>().ToList();
        }
        private void OnBulbCollectionChanged()
        {
            BulbCollectionChanged?.Invoke();
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
