using System.Windows.Input;

namespace ActiveDirectoryQuerier;

/// <summary>
/// A command whose sole purpose is to relay its functionality to other objects by invoking delegates.
/// </summary>
/// <note>
/// This is a very basic setup of the RelayCommand class. I imagine that this class can be expanded upon.
/// </note>
/// TODO: Place class into ViewModel folder.
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Predicate<object?>? _canExecute;
    
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    public RelayCommand(Action<object> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }
    
    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }
    
    // TODO: Perform null checking before calling the _execute property.
    public void Execute(object? parameter)
    {
        // For now, we are ignoring the warning about the parameter being null.
        _execute(parameter!);
    }
    
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
    
    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}
