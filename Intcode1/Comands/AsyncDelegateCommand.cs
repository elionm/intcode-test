using System;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Input;

namespace Intcode1
{
    public sealed class AsyncDelegateCommand : ICommand
    {
        private int callRunning = 0;

        private readonly Func<object?, Task> _execute;
        private readonly Predicate<object?>? _canExecute;
        private readonly Action<Exception>? _executeFaultHandler;
        public event EventHandler? CanExecuteChanged;

        public AsyncDelegateCommand(Func<object?, Task> execute, Predicate<object?>? canExecute, Action<Exception>? executeFaultHandler)
        {
            this._execute = execute;
            this._canExecute = canExecute;
            this._executeFaultHandler = executeFaultHandler;
        }

        public AsyncDelegateCommand(Func<object?, Task> execute, Predicate<object?>? canExecute) : this(execute, canExecute, null)
        {
        }

        public AsyncDelegateCommand(Func<object?, Task> execute) : this(execute, null, null)
        {

        }

        public AsyncDelegateCommand(Func<object?, Task> execute, Action<Exception>? executeFaultHandler) : this(execute, null, executeFaultHandler)
        {

        }

        public bool CanExecute(object? parameter)
        {
            if (callRunning == 1) return false;

            if (_canExecute == null)
                return true;

            return _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            if (Interlocked.CompareExchange(ref callRunning, 1, 0) == 1)
            {
                return;
            }
            RaiseCanExecuteChanged();
            _execute(parameter).ContinueWith((task, _) => ExecuteFinished(task), null, TaskContinuationOptions.ExecuteSynchronously);
        }

        private void ExecuteFinished(Task task)
        {
            Interlocked.Exchange(ref callRunning, 0);
            if (task.IsFaulted && task.Exception != null)
            {
                _executeFaultHandler?.Invoke(task.Exception);
            }
            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
