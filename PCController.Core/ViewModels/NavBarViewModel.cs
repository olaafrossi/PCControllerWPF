// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 11
// by Olaaf Rossi

using System.Collections.ObjectModel;

using MvvmCross.Commands;
using MvvmCross.ViewModels;

using PCController.Core.Models;

namespace PCController.Core.ViewModels
{
    public class NavBarViewModel : MvxViewModel
    {
        private string _firstName;

        private string _lastName;

        private ObservableCollection<LogModel> _loggy = new();

        public NavBarViewModel()
        {
            this.AddLogCommand = new MvxCommand(this.AddLoggyLog);
        }

        public IMvxCommand AddLogCommand { get; set; }

        public bool CanAddLog => this.FirstName?.Length > 0 && this.LastName?.Length > 0;

        public string FirstName
        {
            get
            {
                return this._firstName;
            }
            set
            {
                this.SetProperty(ref this._firstName, value);
                this.RaisePropertyChanged(() => this.FullName);
                this.RaisePropertyChanged(() => this.CanAddLog);
            }
        }

        public string FullName => $"{this.FirstName} {this.LastName}";

        public string LastName
        {
            get
            {
                return this._lastName;
            }
            set
            {
                this.SetProperty(ref this._lastName, value);
                this.RaisePropertyChanged(() => this.FullName);
                this.RaisePropertyChanged(() => this.CanAddLog);
            }
        }

        public ObservableCollection<LogModel> Loggy
        {
            get
            {
                return this._loggy;
            }
            set
            {
                this.SetProperty(ref this._loggy, value);
            }
        }

        public void AddLoggyLog()
        {
            LogModel l = new LogModel { FirstName = this.FirstName, LastName = this.LastName };

            this.FirstName = string.Empty;
            this.LastName = string.Empty;

            this.Loggy.Add(l);
        }
    }
}