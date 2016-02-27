using System;
using System.ComponentModel;

namespace Graphics.ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        public virtual string DisplayName { get; protected set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected BaseViewModel()
        {
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler == null)
                return;
            var e = new PropertyChangedEventArgs(propertyName);
            handler(this, e);
        }

        public void Dispose()
        {
            OnDispose();
        }

        protected virtual void OnDispose()
        {
        }

        public virtual void OnKeyDown(object sender)
        {
            throw new Exception(sender.ToString());
        }
    }
}
