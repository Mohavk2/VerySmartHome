using SmartBulbColor.Models;
using CommonLibrary;
using System;
using System.Windows.Input;
using SmartBulbColor.PluginApp;

namespace SmartBulbColor.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase, IDisposable
    {
        AppCore Controller;

        BulbRepository Repository;

        public AllBulbsViewModel AllBulbsVM { get; }

        public GroupsViewModel GroupsVM { get; }

        public string MainGroupName { get; } = "AllBulbs";

        public MainWindowViewModel()
        {
            Repository = new BulbRepository();
            Controller = new AppCore(Repository);

            AllBulbsVM = new AllBulbsViewModel(MainGroupName, Controller, Repository);
            GroupsVM = new GroupsViewModel(Controller, Repository);
        }

        public void Dispose()
        {
            Controller.Dispose();
            Repository.Dispose();
        }
    }
}
