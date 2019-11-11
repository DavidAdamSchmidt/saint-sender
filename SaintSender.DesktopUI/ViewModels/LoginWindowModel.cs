using SaintSender.Core.Entities;

namespace SaintSender.DesktopUI.ViewModels
{
    public class LoginWindowModel : ViewModelBase
    {
        private string _email;
        private string _password;

        public LoginWindowModel()
        {
            SetCommands();
        }

        public string Email { get => _email; set => SetProperty(ref _email, value); }

        public string Password { get => _password; set => SetProperty(ref _password, value); }

        public DelegateCommand<string> CancelButtonClickCommand { get; private set; }

        private void SetCommands()
        {
            SetCancelButtonCommand();
        }

        private void SetCancelButtonCommand()
        {
            CancelButtonClickCommand = new DelegateCommand<string>(CancelLogin);
        }

        private void CancelLogin(string s)
        {
            Email = string.Empty;
            Password = string.Empty;
        }
    }
}
