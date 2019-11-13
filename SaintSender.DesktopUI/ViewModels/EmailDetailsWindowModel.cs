using SaintSender.Core.Entities;
using System.Windows;
using System.Windows.Controls;

namespace SaintSender.DesktopUI.ViewModels
{
    public class EmailDetailsWindowModel : ViewModelBase
    {
        public DelegateCommand<Button> CloseButtonClickCommand { get; private set; }

        public EmailDetailsWindowModel(CustoMail mail)
        {
            Email = mail;
            SetCommands();
        }

        public CustoMail Email { get; set; }

        private void SetCommands()
        {
            CloseButtonClickCommand = new DelegateCommand<Button>(CloseEmailDetailsWindow_Execute);
        }

        private void CloseEmailDetailsWindow_Execute(Button button)
        {
            var parentWindow = Window.GetWindow(button);
            parentWindow?.Close();
        }
    }
}
