using System;
using System.Windows.Input;
using Graphics.Model;

namespace Graphics.ViewModel
{
    public class ParamsViewModel : BaseViewModel
    {
        public ParamsViewModel(ICommand increase, ICommand decrease, Parameter parameter)
        {
            if (increase == null || decrease == null)
                throw new ArgumentNullException(nameof(increase));

            Increase = increase;
            Decrease = decrease;
            _parameter = parameter;
        }

        private Parameter _parameter;

        public Parameter Parameter
        {
            get { return _parameter; }
            set
            {
                _parameter = value;
                OnPropertyChanged("Parameter");
            }
        }

        public ICommand Increase { get; private set; }
        public ICommand Decrease { get; private set; }
    }
}
