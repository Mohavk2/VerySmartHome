using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using SmartBulbColor.ViewModels;
using SmartBulbColor.Tools;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Threading;

namespace SmartBulbColor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ImagePixelColorPicker ImageColorPicker = new ImagePixelColorPicker("Resources/ColorPicker.jpg");
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainConsole_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            MainConsole.ScrollToEnd();
        }

        private void ColorPicker_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var position = e.GetPosition(ColorPicker);

                var Width = ImageColorPicker.GetImageWidth();
                var Height = ImageColorPicker.GetImageHeight();

                double x = position.X * (Width / ColorPicker.ActualWidth);
                double y = position.Y * (Height / ColorPicker.ActualHeight);

                if (x < 0) x = 0;
                if (x >= Width) x = Width - 1;
                if (y < 0) y = 0;
                if (y >= Height) y = Height - 1;

                var color = ImageColorPicker.GetMediaColor((int)x, (int)y);
                PickedColor.Fill = new SolidColorBrush(color);
                Thread.Sleep(100);
            }
        }

        private void HideConsole_Click(object sender, RoutedEventArgs e)
        {
            if (MainConsole.Visibility == Visibility.Visible)
                MainConsole.Visibility = Visibility.Collapsed;
            else
                MainConsole.Visibility = Visibility.Visible;
        }
    }
}
