using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using SmartBulbColor.ViewModels;
using SmartBulbColor.Tools;
using System.Windows.Forms;

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

        private void ColorPicker_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var position = e.GetPosition(ColorPicker);

        }
    }
}
