using System.Windows;
using System.Windows.Controls;
using SaintSender.Core.Entities;
using SaintSender.Core.Services;
using SaintSender.DesktopUI.Views;

namespace SaintSender.DesktopUI.ViewModels
{
    public class MainWindowModel : Base
    {
        private string _email;
        private bool _loadingEmails;
        private bool _loggingOut;
        private bool _maxRefreshCapacityReached;

        public MainWindowModel()
        {
            SetCommands();

            UserEmail = GmailService.Email;
            SetEmails();
        }

        public AsyncObservableCollection<CustoMail> Emails { get; } = new AsyncObservableCollection<CustoMail>();

        public string UserEmail
        {
            get => _email;
            set => SetProperty(ref _email, string.IsNullOrEmpty(value) ? value : $"Hello, {value}");
        }

        public bool IsLoadingEmails
        {
            get => _loadingEmails;
            set => SetProperty(ref _loadingEmails, value);
        }

        public bool IsLoggingOut
        {
            get => _loggingOut;
            set => SetProperty(ref _loggingOut, value);

        }

        public DelegateCommand<Button> LogoutButtonClickCommand { get; private set; }

        public DelegateCommand<string> SendNewButtonClickCommand { get; private set; }

        public DelegateCommand<CustoMail> ReadDoubleClickedEmail { get; private set; }

        public DelegateCommand<string> ExitProgramCommand { get; private set; }

        public DelegateCommand<string> RefreshButtonClickCommand { get; private set; }

        protected override void SetProperty<T>(ref T storage, T value, string propertyName = null)
        {
            base.SetProperty(ref storage, value, propertyName);

            SendNewButtonClickCommand.RaiseCanExecuteChanged();
            LogoutButtonClickCommand.RaiseCanExecuteChanged();
            RefreshButtonClickCommand.RaiseCanExecuteChanged();
        }

        private async void SetEmails()
        {
            IsLoadingEmails = true;

            _maxRefreshCapacityReached = !await GmailService.FillEmailCollection(Emails);

            IsLoadingEmails = false;

            //if (_maxRefreshCapacityReached && 1 == 2)
            //{
            //    MessageBox.Show("Reached maximum e-mail limit. The refresh functionality will be disabled.",
            //        "Free limited-key alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //}
        }

        private void SetCommands()
        {
            LogoutButtonClickCommand = new DelegateCommand<Button>(Logout_Execute, Logout_CanExecute);
            ExitProgramCommand = new DelegateCommand<string>(Exit_Execute);
            SendNewButtonClickCommand = new DelegateCommand<string>(SendNew_Execute, SendNew_CanExecute);
            ReadDoubleClickedEmail = new DelegateCommand<CustoMail>(ReadEmail_Execute, ReadEmail_CanExecute);
            RefreshButtonClickCommand = new DelegateCommand<string>(RefreshEmails_Execute, RefreshEmails_CanExecute);
        }

        private async void Exit_Execute(string throwAway)
        {
            UserEmail = string.Empty;
            IsLoggingOut = true;
            await GmailService.Flush(Emails);
        }

        private void SendNew_Execute(string throwAway)
        {
            new ComposeWindow().ShowDialog();
        }

        private bool SendNew_CanExecute(string throwAway)
        {
            return SetCommandAvailability();
        }

        private async void Logout_Execute(Button button)
        {
            UserEmail = string.Empty;
            IsLoggingOut = true;
            await GmailService.Flush(Emails);

            var loginWindow = new LoginWindow();

            var parentWindow = Window.GetWindow(button);
            parentWindow?.Close();

            loginWindow.Show();
        }

        private bool Logout_CanExecute(Button button)
        {
            return SetCommandAvailability();
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

        private bool ReadEmail_CanExecute(CustoMail email)
        {
            return SetCommandAvailability();
        }

        private void RefreshEmails_Execute(string throwAway)
        {
            Emails.Clear();
            SetEmails();
        }

        private bool RefreshEmails_CanExecute(string throwAway)
        {
            return !_maxRefreshCapacityReached && SetCommandAvailability();
        }

        private bool SetCommandAvailability()
        {
            return !_loadingEmails && !_loggingOut;
        }
    }
}
