﻿using SaintSender.Core.Entities;
using SaintSender.DesktopUI.Views;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SaintSender.DesktopUI.ViewModels
{
    public class MainWindowModel : ViewModelBase
    {
        private string _emailAddress;
        private bool _loadingEmails;

        public MainWindowModel()
        {
            SetCommands();
        }

        public AsyncObservableCollection<ObservableEmail> Emails { get; } = new AsyncObservableCollection<ObservableEmail>();

        public string EmailAddress
        {
            get => _emailAddress;
            set => SetProperty(ref _emailAddress, string.IsNullOrEmpty(value) ? value : $"Hello, {value}");
        }

        public bool IsLoadingEmails
        {
            get => _loadingEmails;
            set => SetProperty(ref _loadingEmails, value);
        }

        public DelegateCommand<string> WindowLoadedCommand { get; private set; }

        public DelegateCommand<string> SendNewButtonClickCommand { get; private set; }

        public DelegateCommand<ObservableEmail> ReadDoubleClickedEmail { get; private set; }

        public DelegateCommand<string> RefreshButtonClickCommand { get; private set; }

        public DelegateCommand<Button> LogoutButtonClickCommand { get; private set; }

        protected override void SetProperty<T>(ref T storage, T value, string propertyName = null)
        {
            base.SetProperty(ref storage, value, propertyName);

            SendNewButtonClickCommand.RaiseCanExecuteChanged();
            RefreshButtonClickCommand.RaiseCanExecuteChanged();
            LogoutButtonClickCommand.RaiseCanExecuteChanged();
        }

        private async void SetEmails()
        {
            IsLoadingEmails = true;

            await FillEmailCollection();

            IsLoadingEmails = false;
        }

        private async Task FillEmailCollection()
        {
            await TryAsyncOperation(
                async () => await EmailService.UpdateAsync(Emails),
                () => IsLoadingEmails = false);
        }

        private void SetCommands()
        {
            WindowLoadedCommand = new DelegateCommand<string>(LoadEmails_Execute);
            SendNewButtonClickCommand = new DelegateCommand<string>(SendNew_Execute, SendNew_CanExecute);
            ReadDoubleClickedEmail = new DelegateCommand<ObservableEmail>(ReadEmail_Execute, ReadEmail_CanExecute);
            RefreshButtonClickCommand = new DelegateCommand<string>(RefreshEmails_Execute, RefreshEmails_CanExecute);
            LogoutButtonClickCommand = new DelegateCommand<Button>(Logout_Execute, Logout_CanExecute);
        }

        private void LoadEmails_Execute(string throwAway)
        {
            EmailAddress = Core.Services.EmailService.EmailAddress;
            SetEmails();
        }

        private void SendNew_Execute(string throwAway)
        {
            new ComposeWindow().ShowDialog();
        }

        private bool SendNew_CanExecute(string throwAway)
        {
            return !_loadingEmails;
        }

        private void ReadEmail_Execute(ObservableEmail email)
        {
            if (!email.IsRead)
            {
                _ = TryAsyncOperation(
                    () => EmailService.MarkAsReadAsync(email),
                    () => IsLoadingEmails = false);

                email.IsRead = true;
            }

            var emailDetailsDialog = new EmailDetailsWindow
            {
                DataContext = new EmailDetailsWindowModel(email)
            };

            emailDetailsDialog.ShowDialog();
        }

        private bool ReadEmail_CanExecute(ObservableEmail email)
        {
            return !_loadingEmails;
        }

        private async void RefreshEmails_Execute(string throwAway)
        {
            IsLoadingEmails = true;

            Emails.Clear();
            await FillEmailCollection();

            IsLoadingEmails = false;
        }

        private bool RefreshEmails_CanExecute(string throwAway)
        {
            return !_loadingEmails;
        }

        private void Logout_Execute(Button button)
        {
            var loginWindow = new LoginWindow();

            var parentWindow = Window.GetWindow(button);
            parentWindow?.Close();

            loginWindow.Show();
        }

        private bool Logout_CanExecute(Button button)
        {
            return !_loadingEmails;
        }
    }
}
