using CommonLibrary;
using SmartBulbColor.Models;
using SmartBulbColor.PluginApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace SmartBulbColor.ViewModels
{
    internal class GroupsViewModel : ViewModelBase
    {
        AppCore Controller;

        BulbRepository Repository;

        public DispatchedCollection<GroupViewModel> GroupVMs { get; set; } = new DispatchedCollection<GroupViewModel>();

        List<string> _GroupNames;
        public List<string> GroupNames
        {
            get { return Repository.GetUserGroupNames().ToList(); }
            set
            {
                _GroupNames = value;
                OnPropertyChanged("GroupNames");
            }
        }
        string _selectedGroupName;
        public string SelectedGroupName
        {
            get => _selectedGroupName;
            set
            {
                _selectedGroupName = value;
                OnPropertyChanged("SelectedGroupName");
            }
        }
        GroupViewModel _selectedGroupVM;
        public GroupViewModel SelectedGroupVM
        {
            get { return _selectedGroupVM; }
            set
            {
                _selectedGroupVM = value;
                NameToInsert = _selectedGroupVM.GroupName;
                OnPropertyChanged("SelectedGroupVM");
            }
        }
        string _nameToInsert;
        public string NameToInsert
        {
            get => _nameToInsert;
            set
            {
                _nameToInsert = value;
                OnPropertyChanged("NameToInsert");
            }
        }
        public GroupsViewModel(AppCore controller, BulbRepository repository)
        {
            Controller = controller;
            Repository = repository;
        }

        public ICommand CreateNewGroup
        {
            get { return new ControllerCommand(ExecuteCreateNewGroup, CanExecuteCreateNewGroup); }
        }
        void ExecuteCreateNewGroup(object parametr)
        {
            GroupVMs.AddSafe(new GroupViewModel(NameToInsert, Controller, Repository));
            Repository.AddGroup(NameToInsert);
        }
        bool CanExecuteCreateNewGroup(object parametr)
        {
            bool NotExists = true;
            foreach (var group in GroupVMs)
            {
                if (group.GroupName == NameToInsert)
                    NotExists = false;
            }
            return (GroupVMs.Count < 20 && NameToInsert != "" && NotExists);
        }
        public ICommand RenameGroup
        {
            get { return new ControllerCommand(ExecuteRenameGroup, CanExecuteRenameGroup); }
        }
        void ExecuteRenameGroup(object parametr)
        {
            if (SelectedGroupVM.RenameGroup.CanExecute(NameToInsert))
                SelectedGroupVM.RenameGroup.Execute(NameToInsert);
        }
        bool CanExecuteRenameGroup(object parametr)
        {
            bool NotExists = true;
            foreach (var group in GroupVMs)
            {
                if (group.GroupName == NameToInsert)
                    NotExists = false;
            }
            return (NotExists && NameToInsert != "");
        }
        public void RefreshGroupNames(string[] groupNames)
        {
            GroupNames = groupNames.ToList();
        }

    }
}
