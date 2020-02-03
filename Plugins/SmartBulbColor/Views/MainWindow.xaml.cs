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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainConsole_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            MainConsole.ScrollToEnd();
        }
    }
}
