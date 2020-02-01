using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using VerySmartHome.MainController;
using SmartBulbColor.Tools;

namespace SmartBulbColor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);
        BulbController SmartBulbController = new BulbController();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void FindDevicesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SmartBulbController.ConnectBulbs_MusicMode();
                var reports = SmartBulbController.GetDeviceReports();
                foreach (var report in reports)
                {
                    MainConsole.AppendText(report + "\n");
                }
                BulbList.Items.Clear();
                foreach (var bulb in SmartBulbController.Bulbs)
                {
                    BulbList.Items.Add(bulb);
                }
                MainConsole.AppendText(SmartBulbController.DeviceCount + " bulbs found\n");
                MainConsole.ScrollToEnd();
            }
            catch (Exception NoDeviceException)
            {
                MainConsole.AppendText(NoDeviceException.Message + "\n");
                MainConsole.ScrollToEnd();
            }
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            MainConsole.AppendText(string.Empty);
        }
        private void AmbientLightButton_Click(object sender, RoutedEventArgs e)
        {
            if (SmartBulbController.DeviceCount != 0)
            {
                try
                {
                    if (SmartBulbController.IsAmbientLightON)
                    {
                        SmartBulbController.AmbientLight_OFF();
                        MainConsole.AppendText("Ambient Light is OFF\r\n");
                        MainConsole.ScrollToEnd();
                    }
                    else
                    {
                        SmartBulbController.AmbientLight_ON();
                        MainConsole.AppendText("Ambient Light is ON\r\n");
                        MainConsole.ScrollToEnd();
                    }
                }
                catch (Exception MusicModeFailedException)
                {
                    MainConsole.AppendText(MusicModeFailedException.Message.ToString() + "\n");
                    MainConsole.ScrollToEnd();
                }
            }
            else
            {
                MainConsole.AppendText("There is no found bulbs yet, please use \"Find Devices\" first\r\n");
                MainConsole.ScrollToEnd();
            }
        }

        private void NormalLightButton_Click(object sender, RoutedEventArgs e)
        {
            SmartBulbController.NormalLight_ON();
            MainConsole.AppendText("Ambient Light is OFF\r\n");
            MainConsole.ScrollToEnd();
        }
        private void BulbList_GotFocus(object sender, RoutedEventArgs e)
        {

        }
    }
}
