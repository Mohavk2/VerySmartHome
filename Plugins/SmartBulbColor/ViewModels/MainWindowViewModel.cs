using SmartBulbColor.Domain;
using CommonLibrary;
using System;
using SmartBulbColor.PluginApp;

namespace SmartBulbColor.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase, IDisposable
    {
        AppMediator Mediator;

        public AllBulbsViewModel AllBulbsVM { get; }

        public GroupsViewModel GroupsVM { get; }

        public string MainGroupName { get; } = "AllBulbs";

        public MainWindowViewModel()
        {
            Mediator = new AppMediator();

            AllBulbsVM = new AllBulbsViewModel(Mediator);
            GroupsVM = new GroupsViewModel(Mediator);
        }

        public void Dispose()
        {
            Mediator.Dispose();
        }
    }
}
