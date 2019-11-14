using System.Windows;
using System.Windows.Controls;
using SaintSender.Core.Entities;
using SaintSender.Core.Services;
using SaintSender.DesktopUI.Views;

namespace SaintSender.DesktopUI.ViewModels
{
    public class MainWindowModel : ViewModelBase
    {
        private string _email;
        private bool _loadingEmails;
        private bool _loggingOut;

        public MainWindowModel()
        {
            SetCommands();
            GetMails();
        }

        public AsyncObservableCollection<CustoMail> Emails { get; } = new AsyncObservableCollection<CustoMail>();

        public string UserEmail
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public bool IsLoadingEmails
        {
            get => _loadingEmails;
            set
            {
                SetProperty(ref _loadingEmails, value);
                SendNewButtonClickCommand.RaiseCanExecuteChanged();
                PreviousPageButtonCommand.RaiseCanExecuteChanged();
                NextPageButtonCommand.RaiseCanExecuteChanged();
                LogoutButtonClickCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsLoggingOut
        {
            get => _loggingOut;
            set => SetProperty(ref _loggingOut, value);
        }

        public DelegateCommand<Button> LogoutButtonClickCommand { get; private set; }

        public DelegateCommand<string> SendNewButtonClickCommand { get; private set; }

        public DelegateCommand<string> NextPageButtonCommand { get; private set; }

        public DelegateCommand<string> PreviousPageButtonCommand { get; private set; }

        public DelegateCommand<CustoMail> ReadDoubleClickedEmail { get; private set; }

        public DelegateCommand<string> ExitProgramCommand { get; private set; }

        private async void GetMails()
        {
            UserEmail = EmailService.Email;

            IsLoadingEmails = true;

            await EmailService.FillEmailCollection(Emails);

            IsLoadingEmails = false;
        }

        private void SetCommands()
        {
            LogoutButtonClickCommand = new DelegateCommand<Button>(Logout_Execute, Logout_CanExecute);
            ExitProgramCommand = new DelegateCommand<string>(Exit_Execute);
            SendNewButtonClickCommand = new DelegateCommand<string>(SendNew_Execute, SendNew_CanExecute);
            NextPageButtonCommand = new DelegateCommand<string>(NextPageShow_Execute, NextPageShow_CanExecute);
            PreviousPageButtonCommand =
                new DelegateCommand<string>(PreviousPageShow_Execute, PreviousPageShow_CanExecute);
            ReadDoubleClickedEmail = new DelegateCommand<CustoMail>(ReadEmail_Execute);
        }

        private async void Exit_Execute(string throwAway)
        {
            UserEmail = string.Empty;
            IsLoggingOut = true;
            await EmailService.Flush(Emails);
        }

        private void PreviousPageShow_Execute(string throwAway)
        {
            MessageBox.Show("");
        }

        private bool PreviousPageShow_CanExecute(string throwAway)
        {
            return !_loadingEmails;
        }

        private void NextPageShow_Execute(string throwAway)
        {
        }

        private bool NextPageShow_CanExecute(string throwAway)
        {
            return !_loadingEmails;
        }

        private void SendNew_Execute(string throwAway)
        {
            new ComposeWindow().ShowDialog();
        }

        private bool SendNew_CanExecute(string throwAway)
        {
            return !_loadingEmails;
        }

        private async void Logout_Execute(Button button)
        {
            UserEmail = string.Empty;
            IsLoggingOut = true;
            await EmailService.Flush(Emails);

            var loginWindow = new LoginWindow();

            var parentWindow = Window.GetWindow(button);
            parentWindow?.Close();

            loginWindow.Show();
        }

        private bool Logout_CanExecute(Button button)
        {
            return !_loadingEmails;
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
