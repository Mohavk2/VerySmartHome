using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VerySmartHome.MainController;

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
            discoverer.GetDeviceCollectiveResponse();
            string []responses = discoverer.SplitCollectiveResponse();
            foreach(var response in responses)
            {
                MainConsole.Text += response;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SSDPDiscoverer discoverer = new SSDPDiscoverer(
                "M-SEARCH * HTTP/1.1\r\n" +
                "HOST: 239.255.255.250:1982\r\n" +
                "MAN: \"ssdp:discover\"\r\n" +
                "ST: wifi_bulb");
            discoverer.GetDeviceCollectiveResponse();
            string[] responses = discoverer.SplitCollectiveResponse();
            foreach (var response in responses)
            {
                MainConsole.Text += response;
            }
        }
    }
}
