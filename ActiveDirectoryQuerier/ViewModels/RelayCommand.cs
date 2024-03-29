using System.Windows.Input;

namespace ActiveDirectoryQuerier.ViewModels;

/// <summary>
/// This class is a simple implementation of the ICommand interface and a traditional RelayCommand, with an added
/// thread-safe event handler for CanExecuteChanged.
/// </summary>
public class RelayCommand
(Action<object?> execute, Predicate<object?>? canExecute = null) : ICommand
{
    private readonly object _lock = new();

    public bool CanExecute(object? parameter)
    {
        return canExecute?.Invoke(parameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        execute(parameter);
    }

    public event EventHandler ? CanExecuteChanged
    {
        add
        {
            lock (_lock)
            {
                CommandManager.RequerySuggested += value;
            }
        }
        remove
        {
            lock (_lock)
            {
                CommandManager.RequerySuggested -= value;
            }
        }
    }

    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}
