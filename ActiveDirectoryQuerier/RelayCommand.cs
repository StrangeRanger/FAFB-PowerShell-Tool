using System.Windows.Input;

namespace ActiveDirectoryQuerier;

/// <summary>
/// A command whose sole purpose is to relay its functionality to other objects by invoking delegates.
/// </summary>
/// <note>
/// I'm unsure if I'm dealing with the nullability warnings correctly, in this situation...
/// </note>
/// TODO: Remove any unused methods, after some investigation.
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Predicate<object?>? _canExecute;

    /// <summary>
    /// Initializes a new instance of the RelayCommand class.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    public RelayCommand(Action<object> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    /// <summary>
    /// Determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
    /// </param>
    /// <returns>true if this command can be executed; otherwise, false.</returns>
    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
    /// </param>
    /// TODO: Perform null checking before calling the _execute property.
    public void Execute(object? parameter)
    {
        // For now, we are ignoring the warning about the parameter being null.
        _execute(parameter!);
    }

    /// <summary>
    /// Occurs when changes occur that affect whether the command should execute.
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    /// Method to call when the command's ability to execute has changed.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}
