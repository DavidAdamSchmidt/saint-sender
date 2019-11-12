using SaintSender.Core.Entities;
using SaintSender.Core.Services;
using SaintSender.DesktopUI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SaintSender.DesktopUI.ViewModels
{
    class MainWindowModel : ViewModelBase
    {
        public string userEmail { get; private set; } = string.Empty;
        public List<Email> Emails { get; private set; } = new List<Email>;

        public MainWindowModel()
        {
            userEmail = EmailService.Email;
            SetCommands();
        }


        public DelegateCommand<string> LogoutButtonClickCommand { get; private set; }
        
        public DelegateCommand<string> SendNewButtonClickCommand { get; private set; }

        public DelegateCommand<string> NextPageButtonCommand { get; private set; }

        public DelegateCommand<string> PreviousPageButtonCommand { get; private set; }

        public DelegateCommand<DependencyObject> ReadDoubleClickedEmail { get; private set; }

        private void SetCommands()
        {
            LogoutButtonClickCommand = new DelegateCommand<string>(Logout_Execute);
            SendNewButtonClickCommand = new DelegateCommand<string>(SendNew_Execute);
            NextPageButtonCommand = new DelegateCommand<string>(NextPageShow_Execute);
            PreviousPageButtonCommand = new DelegateCommand<string>(PreviousPageShow_Execute);
            ReadDoubleClickedEmail = new DelegateCommand<DependencyObject>(ReadEmail_Execute);
        }

        private void PreviousPageShow_Execute(string throwAway)
        {
            throw new NotImplementedException();
        }

        private void NextPageShow_Execute(string throwAway)
        {
            throw new NotImplementedException();
        }

        private void SendNew_Execute(string throwAway)
        {
            //new SendNewWindow();
        }

        private void Logout_Execute(Button button)
        {
            EmailService.Email = string.Empty;
            var loginWindow = new LoginWindow();
            loginWindow.Show();

            var parentWindow = Window.GetWindow(button);
            parentWindow?.Close();
        }

        private void ReadEmail_Execute(DependencyObject email)
        {
            throw new NotImplementedException();
        }
    }
}
