using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SaintSender.Core.Entities
{
    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        private AsyncOperation _asyncOp;

        public AsyncObservableCollection()
        {
            CreateAsyncOp();
        }

        public AsyncObservableCollection(IEnumerable<T> list)
            : base(list)
        {
            CreateAsyncOp();
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            _asyncOp.Post(RaiseCollectionChanged, e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            _asyncOp.Post(RaisePropertyChanged, e);
        }

        private void RaiseCollectionChanged(object param)
        {
            base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
        }

        private void RaisePropertyChanged(object param)
        {
            base.OnPropertyChanged((PropertyChangedEventArgs)param);
        }

        private void CreateAsyncOp()
        {
            _asyncOp = AsyncOperationManager.CreateOperation(null);
        }
    }
}
