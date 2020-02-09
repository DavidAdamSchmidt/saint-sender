﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SaintSender.Core.Entities;
using SaintSender.Core.Exceptions;
using SaintSender.Core.Services;
using SaintSender.DesktopUI.Views;

namespace SaintSender.DesktopUI.ViewModels
{
    public class LoginWindowModel : ViewModelBase
    {
        private string _email;
        private string _selectedUser;
        private bool _sending;
        private List<UserInfo> _userData;

        public LoginWindowModel()
        {
            SetCommands();
        }

        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
                CancelButtonClickCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsSending
        {
            get => _sending;
            set
            {
                SetProperty(ref _sending, value);
                CancelButtonClickCommand.RaiseCanExecuteChanged();
                SignInButtonClickCommand.RaiseCanExecuteChanged();
            }
        }

        public string SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public AsyncObservableCollection<string> EmailAddresses { get; } = new AsyncObservableCollection<string>();

        public DelegateCommand<string> WindowLoadedCommand { get; private set; }

        public DelegateCommand<string> DropDownOpenedCommand { get; private set; }

        public DelegateCommand<PasswordBox> CancelButtonClickCommand { get; private set; }

        public DelegateCommand<PasswordBox> SignInButtonClickCommand { get; private set; }

        public DelegateCommand<string> PasswordChangedCommand { get; private set; }

        public DelegateCommand<PasswordBox> SelectionChangedCommand { get; private set; }

        private void SetCommands()
        {
            WindowLoadedCommand = new DelegateCommand<string>(RetrieveUserData_Execute);
            CancelButtonClickCommand = new DelegateCommand<PasswordBox>(CancelLogin_Execute, CancelLogin_CanExecute);
            SignInButtonClickCommand = new DelegateCommand<PasswordBox>(AuthenticateLogin_Execute, AuthenticateLogin_CanExecute);
            PasswordChangedCommand = new DelegateCommand<string>(UpdateCancelLoginAvailability_Execute);
            SelectionChangedCommand = new DelegateCommand<PasswordBox>(AutoFillLoginDetails_Execute);
            DropDownOpenedCommand = new DelegateCommand<string>(ResetSelected_Execute);
        }

        private void RetrieveUserData_Execute(string throwAway)
        {
            try
            {
                _userData = new EncryptionService().RetrieveAllData();
            }
            catch (DataRetrievalException)
            {
                _userData = new List<UserInfo>();
            }

            _userData.ForEach(x => EmailAddresses.Add(x.Email));
        }

        private void AutoFillLoginDetails_Execute(PasswordBox passwordBox)
        {
            if (SelectedUser == null)
            {
                return;
            }

            Email = SelectedUser;

            foreach (var user in _userData.Where(user => user.Email.Equals(SelectedUser)))
            {
                passwordBox.Password = user.Password;
                break;
            }
        }

        private void ResetSelected_Execute(string throwAway)
        {
            SelectedUser = null;
        }

        private void CancelLogin_Execute(PasswordBox passwordBox)
        {
            Email = string.Empty;
            passwordBox.Password = string.Empty;
        }

        private bool CancelLogin_CanExecute(PasswordBox passwordBox)
        {
            return (!string.IsNullOrWhiteSpace(Email) ||
                    !string.IsNullOrWhiteSpace(passwordBox?.Password)) &&
                   !_sending; ;
        }

        private async void AuthenticateLogin_Execute(PasswordBox passwordBox)
        {
            IsSending = true;

            var result = await EmailService.AuthenticateAsync(_email, passwordBox.Password);

            if (result)
            {

                var mainWindow = new MainWindow();
                mainWindow.Show();

                var parentWindow = Window.GetWindow(passwordBox);
                parentWindow?.Close();
            }
            else
            {
                IsSending = false;

                MessageBox.Show("Wrong email or password provided.", "Credentials Alert", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }

        private bool AuthenticateLogin_CanExecute(PasswordBox passwordBox)
        {
            return !_sending;
        }

        private void UpdateCancelLoginAvailability_Execute(string s)
        {
            CancelButtonClickCommand.RaiseCanExecuteChanged();
        }
    }
}
