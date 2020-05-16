using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using SmartBulbColor.ViewModels;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows.Controls;
using SmartBulbColor.Models;
using System.Collections.ObjectModel;

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

        private void Window_Closed(object sender, EventArgs e)
        {
            var viewModel = (MainWindowViewModel)DataContext;
            viewModel.Dispose();
        }

        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            AddGoupPopup.IsOpen = true;
        }

        private void RenameGroupButton_Click(object sender, RoutedEventArgs e)
        {
            RenameGoupPopup.IsOpen = true;
        }
    }
}
