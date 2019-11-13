﻿using SaintSender.Core.Entities;
using SaintSender.Core.Services;
using SaintSender.DesktopUI.Views;
using System.Windows;
using System.Windows.Controls;

namespace SaintSender.DesktopUI.ViewModels
{
    public class MainWindowModel : ViewModelBase
    {
        private string _email;

        public MainWindowModel()
        {
            GetMails();
            SetCommands();
        }

        public AsyncObservableCollection<CustoMail> Emails { get; } = new AsyncObservableCollection<CustoMail>();

        public string UserEmail { get => _email; set => SetProperty(ref _email, value); }

        public DelegateCommand<Button> LogoutButtonClickCommand { get; private set; }

        public DelegateCommand<string> SendNewButtonClickCommand { get; private set; }

        public DelegateCommand<string> NextPageButtonCommand { get; private set; }

        public DelegateCommand<string> PreviousPageButtonCommand { get; private set; }

        public DelegateCommand<CustoMail> ReadDoubleClickedEmail { get; private set; }

        public DelegateCommand<string> ExitProgramCommand { get; private set; }

        private async void GetMails()
        {
            await EmailService.FillEmailCollection(Emails);
            UserEmail = EmailService.Email;
        }

        private void SetCommands()
        {
            LogoutButtonClickCommand = new DelegateCommand<Button>(Logout_Execute);
            ExitProgramCommand = new DelegateCommand<string>(Exit_Execute);
            SendNewButtonClickCommand = new DelegateCommand<string>(SendNew_Execute);
            NextPageButtonCommand = new DelegateCommand<string>(NextPageShow_Execute);
            PreviousPageButtonCommand = new DelegateCommand<string>(PreviousPageShow_Execute);
            ReadDoubleClickedEmail = new DelegateCommand<CustoMail>(ReadEmail_Execute);
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
             var emailDetailsDialog = new EmailDetailsWindow
             {
                 DataContext = new EmailDetailsWindowModel(email)
             };
             emailDetailsDialog.ShowDialog();
        }
    }
}
