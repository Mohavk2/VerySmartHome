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
        bool IsAmbientLightActive = false;
        ScreenColorAnalyzer analyzer = new ScreenColorAnalyzer();
        DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);
        BulbController SmartBulbController = new BulbController();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SmartBulbController.DiscoverForBulbs();
                var reports = SmartBulbController.GetDeviceReports();
                foreach (var report in reports)
                {
                    MainConsole.Text += report + "\n";
                }
                ConsoleScrollViewer.ScrollToEnd();
            }
            catch (Exception NoDeviceException)
            {
                MainConsole.Text += NoDeviceException.Message + "\n";
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ToggleAmbilight();
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            MainConsole.Text = string.Empty;
        }
        void ToggleAmbilight()
        {
            if (!IsAmbientLightActive)
            {
                timer.Tick += new EventHandler(SwitchAmbientLightFrame);
                timer.Interval = new TimeSpan(0, 0, 0, 0, 5);
                timer.Start();
                IsAmbientLightActive = true;
                VideoButton.Foreground = Brushes.Red;
                MainConsole.Text += "Ambient Light ENABLED.. \n";
            }
            else
            {
                timer.Tick -= SwitchAmbientLightFrame;
                timer.Stop();
                IsAmbientLightActive = false;
                var converter = new System.Windows.Media.BrushConverter();
                VideoButton.Foreground = (Brush)converter.ConvertFromString("#FF00F92D");
                MainConsole.Text += "Ambient Light DISABLED.. \n";
            }
        }

        void SwitchAmbientLightFrame(object obj, EventArgs e)
        {
            VideoColor.Fill = new SolidColorBrush(analyzer.GetAvgScreenColor());
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
                    MainConsole.Text += MusicModeFailedException.Message.ToString();
                }
            }
            else MainConsole.Text += "There is no found bulbs yet, please use \"Find Devices\" first\r\n";
        }

        private void LightButton_Click(object sender, RoutedEventArgs e)
        {
            SmartBulbController.Light_ON();
        }
    }
}
