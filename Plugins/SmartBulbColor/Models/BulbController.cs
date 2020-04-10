﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VerySmartHome.MainController;

//MM - MusicMode

namespace SmartBulbColor.Models
{
    sealed class BulbController : DeviceController, IDisposable
    {
        public delegate void BulbCollectionNotifier();
        public delegate void BulbStatusChangedHandler(ColorBulb bulb);
        public event BulbStatusChangedHandler BulbFound;
        public event BulbStatusChangedHandler BulbLost;

        public CollectionThreadSafe<ColorBulb> Bulbs { get; } = new CollectionThreadSafe<ColorBulb>();
        public CollectionThreadSafe<ColorBulb> BulbsForAmbientLight { get; private set; } = new CollectionThreadSafe<ColorBulb>();
        public override int DeviceCount
        {
            get
            {
                return Bulbs.Count;
            }
        }
        readonly static BulbDiscoverer Discoverer = new BulbDiscoverer(ColorBulb.SSDPMessage);
        private readonly AmbientLightStreamer AmbientLight = new AmbientLightStreamer();

        public bool IsMusicModeON { get; private set; } = false;

        public bool IsAmbientLightON { get; private set; } = false;

        public BulbController()
        {
            DeviceDiscoverer.DeviceFound += OnDeviceFound;
            DeviceDiscoverer.DeviceLost += OnDeviceLost;
            Discoverer.StartDiscover();
        }
        public override List<Device> GetDevices()
        {
            return new List<Device>();
        }
        public CollectionThreadSafe<ColorBulb> GetBulbs()
        {
            if (Bulbs.Count != 0)
            {
                return Bulbs;
            }
            else
            {
                return Bulbs;
            }
        }
        public void DiscoverBulbs()
        {
            Discoverer.FindDevices();
        }
        public void StartBulbsRefreshing()
        {
            Discoverer.StartDiscover();
        }
        public void TogglePower(ColorBulb bulb)
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
                foreach (var bulb in Bulbs)
                {
                    AmbientLight.AddBulbForStreaming(bulb);
                }
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
        public void SetSceneHSV(ColorBulb bulb, HSBColor color)
        {
            bulb.SetSceneHSV(color.Hue, color.Saturation, color.Brightness);
        }
        private void OnDeviceFound(Device foundDevice)
        {
            Task task = new Task(() =>
            {
                var bulb = (ColorBulb)foundDevice;
                Bulbs.Add(bulb);
                OnBulbFound(bulb);
            });
            task.Start();
        }
        private void OnDeviceLost(Device lostDevice)
        {
            Task task = new Task(() =>
            {
                var bulb = (ColorBulb)lostDevice;
                Bulbs.Remove(bulb);
                OnBulbLost(bulb);
            });
            task.Start();
        }
        private void OnBulbFound(ColorBulb bulb)
        {
            BulbFound?.Invoke(bulb);
        }
        private void OnBulbLost(ColorBulb bulb)
        {
            BulbLost?.Invoke(bulb);
        }
        public void Dispose()
        {

        }
    }
}
