using System;
using System.Windows.Input;

namespace Graphics.ViewModel
{
    public sealed class TaskViewModel : BaseViewModel
    {
        public TaskViewModel(string displayName, ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            DisplayName = displayName;
            Command = command;
        }

        public ICommand Command { get; private set; }
    }
}
