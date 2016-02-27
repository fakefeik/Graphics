using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Graphics.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        private ReadOnlyCollection<TaskViewModel> _tasks; 

        public ReadOnlyCollection<TaskViewModel> Tasks => _tasks ?? (_tasks = new ReadOnlyCollection<TaskViewModel>(CreateTasks()));

        private List<TaskViewModel> CreateTasks()
        {
            return Enumerable.Range(1, 6)
                .Select(
                    x =>
                        new TaskViewModel($"Task {x}",
                            new RelayCommand(o => CurrentViewModel = new ChartViewModel($"Task{x}")))).ToList();

        }

        private BaseViewModel _currentViewModel = new ChartViewModel("Start");

        public BaseViewModel CurrentViewModel
        {
            get { return _currentViewModel;}
            set
            {
                _currentViewModel = value;
                OnPropertyChanged("CurrentViewModel");
            }
        }
    }
}
