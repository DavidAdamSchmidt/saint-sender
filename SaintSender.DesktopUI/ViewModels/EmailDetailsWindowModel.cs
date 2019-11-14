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
        public DelegateCommand<Button> CloseButtonClickCommand { get; private set; }

        public DelegateCommand<Button> DeleteButtonClickCommand { get; private set; }

        public EmailDetailsWindowModel(CustoMail mail)
        {
            Email = mail;
            SetCommands();
        }

        public CustoMail Email { get; set; }

        private void SetCommands()
        {
            CloseButtonClickCommand = new DelegateCommand<Button>(CloseEmailDetailsWindow_Execute);
            DeleteButtonClickCommand = new DelegateCommand<Button>(DeleteCurrentEmail_Execute);
        }

        private void DeleteCurrentEmail_Execute(Button button)
        {
            GmailService.DeleteEmail(Email.MessageNumber);
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
