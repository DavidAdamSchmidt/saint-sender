using GemBox.Email;
using SaintSender.Core.Entities;
using SaintSender.Core.Services;
using SaintSender.DesktopUI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SaintSender.DesktopUI.ViewModels
{
    class MainWindowModel : ViewModelBase
    {
        private bool _isRead;

        public MainWindowModel()
        {
            GetMails();
            SetCommands();
        }

        public string userEmail { get; private set; } = EmailService.Email;
        public ObservableCollection<MailMessage> Emails { get; private set; } = new ObservableCollection<MailMessage>();
        public bool EmailIsRead { 
            get => _isRead; 
            set => SetProperty(ref _isRead, value); 
        }


        private void GetMails()
        {
            Emails = EmailService.GetEmails(1, 20);
        }

        public DelegateCommand<Button> LogoutButtonClickCommand { get; private set; }
        
        public DelegateCommand<string> SendNewButtonClickCommand { get; private set; }

        public DelegateCommand<string> NextPageButtonCommand { get; private set; }

        public DelegateCommand<string> PreviousPageButtonCommand { get; private set; }

        public DelegateCommand<MailMessage> ReadDoubleClickedEmail { get; private set; }

        public DelegateCommand<string> CheckIfEmailWasRead { get; private set; }

        private void SetCommands()
        {
            LogoutButtonClickCommand = new DelegateCommand<Button>(Logout_Execute);
            SendNewButtonClickCommand = new DelegateCommand<string>(SendNew_Execute);
            NextPageButtonCommand = new DelegateCommand<string>(NextPageShow_Execute);
            PreviousPageButtonCommand = new DelegateCommand<string>(PreviousPageShow_Execute);
            ReadDoubleClickedEmail = new DelegateCommand<MailMessage>(ReadEmail_Execute);
            CheckIfEmailWasRead = new DelegateCommand<string>(CheckMail);
        }

        private void CheckMail(string html)
        {
            if (html.Contains("<span hidden='hidden'>SEEN</span>"))
            {
               EmailIsRead = false;
            }
            EmailIsRead = true;
        }

        private void PreviousPageShow_Execute(string throwAway)
        {
        }

        private void NextPageShow_Execute(string throwAway)
        {
        }

        private void SendNew_Execute(string throwAway)
        {
            //new ComposeWindow();
        }

        private void Logout_Execute(Button button)
        {
            EmailService.Email = string.Empty;
            var loginWindow = new LoginWindow();
            loginWindow.Show();

            var parentWindow = Window.GetWindow(button);
            parentWindow?.Close();
        }

        private void ReadEmail_Execute(MailMessage email)
        {
            //new ReadEmailWindow();
        }
    }
}
