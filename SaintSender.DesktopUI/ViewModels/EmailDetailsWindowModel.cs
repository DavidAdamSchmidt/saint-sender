using SaintSender.Core.Entities;
using SaintSender.Core.Services;
using SaintSender.DesktopUI.Views;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SaintSender.DesktopUI.ViewModels
{
    public class EmailDetailsWindowModel : Base
    {
        private bool _deletingEmail;
        private bool _savingEmail;

        public EmailDetailsWindowModel(CustoMail mail)
        {
            Email = mail;
            SetCommands();
        }

        public CustoMail Email { get; set; }

        public bool IsDeletingEmail
        {
            get => _deletingEmail;
            set => SetProperty(ref _deletingEmail, value);
        }

        public bool IsSavingEmail
        {
            get => _savingEmail;
            set => SetProperty(ref _savingEmail, value);
        }

        public DelegateCommand<Button> CloseButtonClickCommand { get; private set; }

        public DelegateCommand<Button> DeleteButtonClickCommand { get; private set; }

        public DelegateCommand<Button> SaveToFileClickCommand { get; private set; }

        protected override void SetProperty<T>(ref T storage, T value, string propertyName = null)
        {
            base.SetProperty(ref storage, value, propertyName);
            CloseButtonClickCommand.RaiseCanExecuteChanged();
            DeleteButtonClickCommand.RaiseCanExecuteChanged();
            SaveToFileClickCommand.RaiseCanExecuteChanged();
        }

        private void SetCommands()
        {
            CloseButtonClickCommand = new DelegateCommand<Button>(CloseEmailDetailsWindow_Execute, CloseEmailDetailsWindow_CanExecute);
            DeleteButtonClickCommand = new DelegateCommand<Button>(DeleteCurrentEmail_Execute, DeleteCurrentEmail_CanExecute);
            SaveToFileClickCommand = new DelegateCommand<Button>(SaveToFile_Execute, SaveToFile_CanExecute);
        }

        private async void SaveToFile_Execute(Button button)
        {
            IsSavingEmail = true;

            var overwritten = await GmailService.SaveEmailToFile(Email);

            IsSavingEmail = false;

            var message = $"Your E-Mail has been successfully {(overwritten ? "overwritten" : "saved")}!";

            MessageBox.Show(message, "E-Mail Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool SaveToFile_CanExecute(Button button)
        {
            return !IsWorking();
        }

        private async void DeleteCurrentEmail_Execute(Button button)
        {
            IsDeletingEmail = true;

            await GmailService.DeleteEmail(Email);

            IsDeletingEmail = false;

            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            var windowModel = (MainWindowModel)mainWindow.DataContext;
            windowModel.Emails.Remove(Email);
            CloseParentWindow(button);
        }

        private bool DeleteCurrentEmail_CanExecute(Button button)
        {
            return !IsWorking();
        }

        private void CloseEmailDetailsWindow_Execute(Button button)
        {
            CloseParentWindow(button);
        }

        private bool CloseEmailDetailsWindow_CanExecute(Button button)
        {
            return !IsWorking();
        }

        private void CloseParentWindow(Button button)
        {
            var parentWindow = Window.GetWindow(button);
            parentWindow?.Close();
        }

        private bool IsWorking()
        {
            return _savingEmail || _deletingEmail;
        }
    }
}
