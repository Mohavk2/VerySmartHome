using SmartBulbColor.ViewModels;
using System.Linq;
using System.Windows.Controls;

namespace SmartBulbColor.Views
{
    /// <summary>
    /// Interaction logic for AllBulbsView.xaml
    /// </summary>
    public partial class AllBulbsView : UserControl
    {
        public AllBulbsView()
        {
            InitializeComponent();
        }

        private void BulbList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = (AllBulbsViewModel)DataContext;
            viewModel.SelectedBulbVMs = BulbList.SelectedItems.Cast<ColorBulbViewModel>().ToList();
        }

        private void AddToGroupButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AddToGroupPopup.IsOpen = true;
        }
    }
}
