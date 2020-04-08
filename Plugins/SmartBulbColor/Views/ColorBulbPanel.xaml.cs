using SmartBulbColor.Models;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;


namespace SmartBulbColor.Views
{
    /// <summary>
    /// Interaction logic for ColorBulbPanel.xaml
    /// </summary>
    public partial class ColorBulbPanel : UserControl
    {
        ImagePixelColorPicker ImageColorPicker = new ImagePixelColorPicker("Resources/ColorPicker.jpg");
        public ColorBulbPanel()
        {
            InitializeComponent();
        }
        private void Palette_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var position = e.GetPosition(Palette);

                var Width = ImageColorPicker.GetImageWidth();
                var Height = ImageColorPicker.GetImageHeight();

                Canvas.SetLeft(PickerMarker, position.X - 10);
                Canvas.SetTop(PickerMarker, position.Y - 10);

                double x = position.X * (Width / Palette.ActualWidth);
                double y = position.Y * (Height / Palette.ActualHeight);

                if (x < 0) x = 0;
                if (x >= Width) x = Width - 1;
                if (y < 0) y = 0;
                if (y >= Height) y = Height - 1;

                var color = ImageColorPicker.GetMediaColor((int)x, (int)y);
                PickedColor.Fill = new SolidColorBrush(color);
                Thread.Sleep(30);
            }
        }
        private void Palette_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var position = e.GetPosition(Palette);

            var Width = ImageColorPicker.GetImageWidth();
            var Height = ImageColorPicker.GetImageHeight();

            Canvas.SetLeft(PickerMarker, position.X - 10);
            Canvas.SetTop(PickerMarker, position.Y - 10);

            double x = position.X * (Width / Palette.ActualWidth);
            double y = position.Y * (Height / Palette.ActualHeight);

            if (x < 0) x = 0;
            if (x >= Width) x = Width - 1;
            if (y < 0) y = 0;
            if (y >= Height) y = Height - 1;

            var color = ImageColorPicker.GetMediaColor((int)x, (int)y);
            PickedColor.Fill = new SolidColorBrush(color);
        }
    }
}
