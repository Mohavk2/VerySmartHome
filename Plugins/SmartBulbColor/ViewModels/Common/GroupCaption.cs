using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SmartBulbColor.ViewModels.Common
{
    public class GroupCaption : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        string _id;
        public string Id 
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }
        string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
