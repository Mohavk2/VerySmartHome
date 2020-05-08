using CommonLibrary;
using SmartBulbColor.Infrastructure;
using SmartBulbColor.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace SmartBulbColor.ViewModels
{
    internal class GroupViewModel : ViewModelBase
    {
        private BulbController Controller;

        string _groupName;
        public string GroupName
        {
            get => _groupName;
            set
            {
                _groupName = value;
                OnPropertyChanged("GroupName");
            }
        }

        public DispatchedCollection<ColorBulbViewModel> ColorBulbVMs { get; set; } = new DispatchedCollection<ColorBulbViewModel>();

        List<ColorBulbViewModel> _selectedBulbVMs;
        public List<ColorBulbViewModel> SelectedBulbVMs
        {
            get { return _selectedBulbVMs; }
            set
            {
                _selectedBulbVMs = value;
                OnPropertyChanged("SelectedBulbVMs");
            }
        }
        public GroupViewModel(BulbController controller, string groupName, DispatchedCollection<ColorBulbViewModel> group)
        {
            Controller = controller;
            GroupName = groupName;
        }
        public ICommand RenameGroup
        {
            get { return new ControllerCommand(ExecuteRenameGroup, CanExecuteRenameGroup); }
        }
        void ExecuteRenameGroup(object parametr)
        {
            GroupName = parametr as string;
        }
        bool CanExecuteRenameGroup(object parametr)
        {
            var name = parametr as string;
            return (name != null && name != "");
        }
        public void AddBulbVM(ColorBulbViewModel bulbVM)
        {
            ColorBulbVMs.AddSafe(bulbVM);
        }
    }
}
