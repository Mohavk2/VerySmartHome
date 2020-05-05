using CommonLibrary;
using SmartBulbColor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBulbColor.ViewModels
{
    internal class GroupViewModel : ViewModelBase
    {
        private BulbController Controller;

        public string GroupName { get; }
        public DispatchedCollection<ColorBulbViewModel> ColorBulbVMs { get; set; } = new DispatchedCollection<ColorBulbViewModel>();
        ColorBulbViewModel _selectedBulbVM;

        public ColorBulbViewModel SelectedBulbVM
        {
            get { return _selectedBulbVM; }
            set
            {
                _selectedBulbVM = value;
                OnPropertyChanged("SelectedBulbVM");
            }
        }
        public GroupViewModel(BulbController controller, string groupName, DispatchedCollection<ColorBulbViewModel> group)
        {
            Controller = controller;
            GroupName = groupName;
        }
        public void AddBulbVM(ColorBulbViewModel bulbVM)
        {
            ColorBulbVMs.AddSafe(bulbVM);
        }
    }
}
