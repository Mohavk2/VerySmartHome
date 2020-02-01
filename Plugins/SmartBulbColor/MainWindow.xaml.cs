using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using VerySmartHome.MainController;
using VerySmartHome.Tools;

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
                    MainConsole.Text += report + "\n";
                }
                BulbList.Items.Clear();
                foreach (var bulb in SmartBulbController.Bulbs)
                {
                    BulbList.Items.Add(bulb.Id);
                }
                MainConsole.Text += SmartBulbController.DeviceCount + " bulbs found\n";
                ConsoleScrollViewer.ScrollToEnd();
            }
            catch (Exception NoDeviceException)
            {
                MainConsole.Text += NoDeviceException.Message + "\n";
            }
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            MainConsole.Text = string.Empty;
        }
        private void AmbientLightButton_Click(object sender, RoutedEventArgs e)
        {
            if (SmartBulbController.DeviceCount != 0)
            {
                try
                {
                    if(SmartBulbController.IsAmbientLightON)
                    {
                        SmartBulbController.AmbientLight_OFF();
                        MainConsole.Text += "Ambient Light is OFF\r\n";
                    }
                    else
                    {
                        SmartBulbController.AmbientLight_ON();
                        MainConsole.Text += "Ambient Light is ON\r\n";
                    }
                }
                catch (Exception MusicModeFailedException)
                {
                    MainConsole.Text += MusicModeFailedException.Message.ToString() + "\n";
                }
            }
            else MainConsole.Text += "There is no found bulbs yet, please use \"Find Devices\" first\r\n";
        }

        private void NormalLightButton_Click(object sender, RoutedEventArgs e)
        {
            SmartBulbController.NormalLight_ON();
        }
    }
}
