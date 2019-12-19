using System;
using System.Collections.Generic;
using System.Windows;
using System.Threading;
using System.Windows.Media;
using VerySmartHome.MainController;
using VerySmartHome.Tools;

namespace VerySmartHome
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SSDPDiscoverer discoverer = new SSDPDiscoverer(
                "M-SEARCH * HTTP/1.1\r\n" +
                "HOST: 239.255.255.250:1982\r\n" +
                "MAN: \"ssdp:discover\"\r\n" +
                "ST: wifi_bulb");
            List<string> deviceResponses = discoverer.GetDeviceResponses();
            foreach(var response in deviceResponses)
            {
                MainConsole.Text += response + "\n";
            }
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

            Thread thread = new Thread(new ThreadStart(StartAmbilight));
            thread.Start();
        }

        void StartAmbilight()
        {
            ScreenColorAnalyzer analyzer = new ScreenColorAnalyzer();
            while (true)
            {
                VideoColor.Dispatcher.Invoke(new Action(() =>
                {

                    VideoColor.Fill = new SolidColorBrush(analyzer.GetAvgScreenColor());
                    Thread.Sleep(20);

                }));
                Thread.Sleep(20);
            }
        }
    }
}
