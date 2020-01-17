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
        bool IsColorTest = false;
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
                MainConsole.Text += SmartBulbController.DeviceCount + " bulbs found\n";
                ConsoleScrollViewer.ScrollToEnd();
            }
            catch (Exception NoDeviceException)
            {
                MainConsole.Text += NoDeviceException.Message + "\n";
            }
        }
        private void ScreenColorTestButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleColorTest();
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            MainConsole.Text = string.Empty;
        }
        void ToggleColorTest()
        {
            if (!IsColorTest)
            {
                timer.Tick += new EventHandler(GetColorTestFrame);
                timer.Interval = new TimeSpan(0, 0, 0, 0, 5);
                timer.Start();
                IsColorTest = true;
                ScreenColorTestButton.Foreground = Brushes.Red;
                MainConsole.Text += "Debag mode ENABLED.. \n";
            }
            else
            {
                timer.Tick -= GetColorTestFrame;
                timer.Stop();
                IsColorTest = false;
                var converter = new System.Windows.Media.BrushConverter();
                ScreenColorTestButton.Foreground = (Brush)converter.ConvertFromString("#FF00F92D");
                MainConsole.Text += "Debag mode DISABLED.. \n";
            }
        }
        void GetColorTestFrame(object obj, EventArgs e)
        {
            ScreenColor.Fill = new SolidColorBrush(SmartBulbController.ColorAnalyzer.GetColorBuffer());
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
