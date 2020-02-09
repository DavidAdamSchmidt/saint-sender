using System;
using System.Threading.Tasks;
using System.Windows;
using SaintSender.Core.Entities;
using SaintSender.Core.Exceptions;
using SaintSender.Core.Services;

namespace SaintSender.DesktopUI.ViewModels
{
    public abstract class ViewModelBase : ObservableBase
    {
        protected ViewModelBase()
            : this(new EmailService("gmail.com"))
        {
        }

        protected ViewModelBase(EmailService emailService)
        {
            EmailService = emailService;
        }

        protected EmailService EmailService { get; }

        protected async Task TryAsyncOperation(Func<Task> asyncOp, Action onError)
        {
            try
            {
               await asyncOp();
            }
            catch (DataRetrievalException e)
            {
                HandleDataRetrievalError(onError, e);
            }
        }

        protected async Task<T> TryAsyncOperation<T>(Func<Task<T>> asyncOp, Action onError)
        {
            try
            {
                return await asyncOp();
            }
            catch (DataRetrievalException e)
            {
                HandleDataRetrievalError(onError, e);

                return default;
            }
        }

        private void HandleDataRetrievalError(Action onError, DataRetrievalException e)
        {
            onError();

            MessageBox.Show($"Could not retrieve user credentials. Reason: {e.Message}.", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);

            Environment.Exit(Environment.ExitCode);
        }
    }
}
