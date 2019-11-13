using SaintSender.Core.Entities;
using SaintSender.Core.Interfaces;
using SaintSender.Core.Services;
using SaintSender.DesktopUI.Views;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SaintSender.DesktopUI.ViewModels
{
    class MainWindowModel : Base
    {
        public MainWindowModel()
        {
            GetMails();
            SetCommands();
        }
        private string _email;
        private ObservableCollection<CustoMail> _emails;
        public string UserEmail { get => _email; set => SetProperty(ref _email, value); }
        public ObservableCollection<CustoMail> Emails { get => _emails; set => SetProperty(ref _emails, value); }

        private void GetMails()
        {
            Emails = EmailService.GetEmails();
            UserEmail = EmailService.Email;
        }

        public DelegateCommand<Button> LogoutButtonClickCommand { get; private set; }

        public DelegateCommand<string> SendNewButtonClickCommand { get; private set; }

        public DelegateCommand<string> NextPageButtonCommand { get; private set; }

        public DelegateCommand<string> PreviousPageButtonCommand { get; private set; }

        public DelegateCommand<CustoMail> ReadDoubleClickedEmail { get; private set; }

        public DelegateCommand<string> ExitProgramCommand { get; private set; }

        public DelegateCommand<string> RefreshButtonClickCommand { get; private set; }

        private void SetCommands()
        {
            LogoutButtonClickCommand = new DelegateCommand<Button>(Logout_Execute);
            ExitProgramCommand = new DelegateCommand<string>(Exit_Execute);
            SendNewButtonClickCommand = new DelegateCommand<string>(SendNew_Execute);
            NextPageButtonCommand = new DelegateCommand<string>(NextPageShow_Execute);
            PreviousPageButtonCommand = new DelegateCommand<string>(PreviousPageShow_Execute);
            ReadDoubleClickedEmail = new DelegateCommand<CustoMail>(ReadEmail_Execute);
            RefreshButtonClickCommand = new DelegateCommand<string>(RefreshCurrent_Execute);
        }

        private void RefreshCurrent_Execute(string obj)
        {
            EmailService.Flush(Emails);
            Emails = EmailService.GetEmails();
        }

        private void Exit_Execute(string throwAway)
        {
            EmailService.Flush(Emails);
            UserEmail = string.Empty;
        }

        private void PreviousPageShow_Execute(string throwAway)
        {
        }

        private void NextPageShow_Execute(string throwAway)
        {
        }

        private void SendNew_Execute(string throwAway)
        {
            new ComposeWindow().ShowDialog();
        }

        private void Logout_Execute(Button button)
        {
            EmailService.Flush(Emails);
            UserEmail = string.Empty;

            var loginWindow = new LoginWindow();
            loginWindow.Show();

            var parentWindow = Window.GetWindow(button);
            parentWindow?.Close();

        }

        private void ReadEmail_Execute(CustoMail email)
        {
             email.IsRead = true;
             var emailDetailsDialog = new EmailDetailsWindow();
             emailDetailsDialog.DataContext = new EmailDetailsWindowModel(email);
             emailDetailsDialog.ShowDialog();
        }
    }
}
