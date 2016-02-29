using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Graphics.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        public ICommand KeyCommand { get; set; }

        private ReadOnlyCollection<TaskViewModel> _tasks; 

        public ReadOnlyCollection<TaskViewModel> Tasks => _tasks ?? (_tasks = new ReadOnlyCollection<TaskViewModel>(CreateTasks()));

        private List<TaskViewModel> CreateTasks()
        {
            var tasks = Enumerable.Range(1, 3)
                .Select(
                    x =>
                        new TaskViewModel($"Task {x}",
                            new RelayCommand(o => CurrentViewModel = new ChartViewModel($"Task{x}")))).ToList();
            tasks.Add(new TaskViewModel("Task 4", new RelayCommand(o => CurrentViewModel = new PolyViewModel("Task4"))));
            tasks.AddRange(Enumerable.Range(5, 2).Select(x => new TaskViewModel($"Task {x}", new RelayCommand(o => CurrentViewModel = new RendererViewModel($"Task{x}")))));
            return tasks;
        }

        private BaseViewModel _currentViewModel = new ChartViewModel("Task1");
        
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
