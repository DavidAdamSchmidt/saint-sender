using SaintSender.Core.Entities;
using SaintSender.Core.Services;
using SaintSender.DesktopUI.Views;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SaintSender.DesktopUI.ViewModels
{
    public class EmailDetailsWindowModel : Base
    {
        private bool _isDeletingEmail = false;

        public EmailDetailsWindowModel(CustoMail mail)
        {
            Email = mail;
            SetCommands();
        }

        public CustoMail Email { get; set; }

        public bool IsDeletingEmail
        {
            get => _isDeletingEmail;
            set => SetProperty(ref _isDeletingEmail, value);
        }

        public DelegateCommand<Button> CloseButtonClickCommand { get; private set; }

        public DelegateCommand<Button> DeleteButtonClickCommand { get; private set; }

        public DelegateCommand<Button> SaveToFileClickCommand { get; private set; }

        private void SetCommands()
        {
            CloseButtonClickCommand = new DelegateCommand<Button>(CloseEmailDetailsWindow_Execute);
            DeleteButtonClickCommand = new DelegateCommand<Button>(DeleteCurrentEmail_Execute);
            SaveToFileClickCommand = new DelegateCommand<Button>(SaveToFile_Execute);
        }

        private void SaveToFile_Execute(Button button)
        {
            GmailService.SaveEmailToFile(Email);
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

        private void CloseEmailDetailsWindow_Execute(Button button)
        {
            CloseParentWindow(button);
        }

        private void CloseParentWindow(Button button)
        {
            var parentWindow = Window.GetWindow(button);
            parentWindow?.Close();
        }
    }
}
