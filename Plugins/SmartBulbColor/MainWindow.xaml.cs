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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SSDPDiscoverer discoverer = new SSDPDiscoverer(
                "M-SEARCH * HTTP/1.1\r\n" +
                "HOST: 239.255.255.250:1982\r\n" +
                "MAN: \"ssdp:discover\"\r\n" +
                "ST: wifi_bulb");
            List<string> deviceResponses = discoverer.GetDeviceResponses();
            foreach (var response in deviceResponses)
            {
                MainConsole.Text += response + "\n";
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ToggleAmbilight();
        }

        void ToggleAmbilight()
        {
            if (!IsAmbientLightActive)
            {
                timer.Tick += new EventHandler(SwitchAmbientLightFrame);
                timer.Interval = new TimeSpan(0, 0, 0, 0, 5);
                timer.Start();
                IsAmbientLightActive = true;
            }
            else
            {
                timer.Tick -= SwitchAmbientLightFrame;
                timer.Stop();
                IsAmbientLightActive = false;
            }
        }

        void SwitchAmbientLightFrame(object obj, EventArgs e)
        {
            VideoColor.Fill = new SolidColorBrush(analyzer.GetAvgScreenColor());
        }
    }
}
