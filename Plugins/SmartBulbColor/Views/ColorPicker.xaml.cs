using SmartBulbColor.Models;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SmartBulbColor.Views
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        public static readonly DependencyProperty MyPropertyProperty;

        static ColorPicker()
        {
            MyPropertyProperty = 
                DependencyProperty.Register("SelectedColor", typeof(SolidColorBrush), typeof(ColorPicker));
        }

        public SolidColorBrush SelectedColor
        {
            get { return (SolidColorBrush)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }

        readonly Bitmap ColorPickerImage = Properties.Resources.ColorPicker;

        public ColorPicker()
        {
            InitializeComponent();
        }
        private void Palette_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var position = e.GetPosition(Palette);

                var Width = ColorPickerImage.Width;
                var Height = ColorPickerImage.Height;

                Canvas.SetLeft(PickerMarker, position.X - 10);
                Canvas.SetTop(PickerMarker, position.Y - 10);

                double x = position.X * (Width / Palette.ActualWidth);
                double y = position.Y * (Height / Palette.ActualHeight);

                if (x < 0) x = 0;
                if (x >= Width) x = Width - 1;
                if (y < 0) y = 0;
                if (y >= Height) y = Height - 1;

                var pixel = ColorPickerImage.GetPixel((int)x, (int)y);
                var color = HSBColor.DrowingToMediaColor(pixel);
                SelectedColor = new SolidColorBrush(color);
                ColorMarker.Fill = SelectedColor;
                Thread.Sleep(30);
            }
        }
        private void Palette_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var position = e.GetPosition(Palette);

            var Width = ColorPickerImage.Width;
            var Height = ColorPickerImage.Height;

            Canvas.SetLeft(PickerMarker, position.X - 10);
            Canvas.SetTop(PickerMarker, position.Y - 10);

            double x = position.X * (Width / Palette.ActualWidth);
            double y = position.Y * (Height / Palette.ActualHeight);

            if (x < 0) x = 0;
            if (x >= Width) x = Width - 1;
            if (y < 0) y = 0;
            if (y >= Height) y = Height - 1;

            var pixel = ColorPickerImage.GetPixel((int)x, (int)y);
            var color = HSBColor.DrowingToMediaColor(pixel);
            SelectedColor = new SolidColorBrush(color);
            ColorMarker.Fill = SelectedColor;
        }
    }
}
