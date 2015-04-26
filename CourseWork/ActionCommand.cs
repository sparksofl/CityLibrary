using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CourseWork
{
    public class ActionCommand : ICommand
    {
        // Класс команды без параметров
        private Action _action;
        private bool _isExecutable;

        public bool IsExecutable
        {
            get { return _isExecutable; }
            set
            {
                _isExecutable = value;
                if (CanExecuteChanged == null)
                    return;
                CanExecuteChanged(this, new EventArgs());
            }
        }

        public ActionCommand(Action action)
        {
            _action = action;
        }

        // Предикат показывает можно ли запускать команды при заданном аргументе.
        public bool CanExecute(object parameter)
        {
            return IsExecutable;
        }

        // Что должна выполнять команда
        public void Execute(object parameter)
        {
            _action();
        }

        public event EventHandler CanExecuteChanged;
    }
}
