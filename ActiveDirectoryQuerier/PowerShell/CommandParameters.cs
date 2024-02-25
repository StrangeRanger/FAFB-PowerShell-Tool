using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace ActiveDirectoryQuerier.PowerShell;

/// <summary>
/// Manages and provides the parameters available for a given PowerShell command.
/// TODO: Figure out what to do with the Async and non-Async methods! Too much duplicate code..
/// </summary>
public class CommandParameters : ICommandParameters
{
    private readonly ObservableCollection<string> _possibleParameters = new();

    /// <summary>
    /// Gets the collection of possible parameters for a command.
    /// </summary>
    /// <note>
    /// I realize that this isn't a very clean way of doing things, but it's the best I could come up with at the time.
    /// </note>
    /// <exception cref="InvalidOperationException">Thrown when the collection has not been populated yet.</exception>
    public ObservableCollection<string> PossibleParameters
    {
        get {
            if (_possibleParameters.Count == 0)
            {
                throw new InvalidOperationException(
                    "'PossibleParameters' has not been populated via 'LoadCommandParametersAsync'.");
            }

            return _possibleParameters;
        }
    }

    /// <summary>
    /// Asynchronously loads the possible parameters for the specified command into the '_possibleParameters'
    /// collection.
    /// </summary>
    /// <param name="commandObject">The PowerShell command object whose parameters are to be loaded.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task LoadCommandParametersAsync(Command? commandObject)
    {
        // commandObject can be null if the user attempts to select an ActiveDirectory command that doesn't exist.
        // More specifically, if the entered command doesn't exist in the ActiveDirectoryCommandList in
        // MainWindowViewModel.cs, commandObject will be null, causing an exception to be thrown, crashing the program.
        if (commandObject is null)
        {
            Trace.WriteLine("Error: commandObject is null");
            _possibleParameters.Add("No valid command provided");
            return;
        }

        if (_possibleParameters.Count == 0)
        {
            using var powerShell = System.Management.Automation.PowerShell.Create();
            string commandString =
                $"Get-Command {commandObject.CommandText} | Select -ExpandProperty Parameters | ForEach-Object {{ $_.Keys }}";

            powerShell.Commands.Clear();
            powerShell.AddScript(commandString);
            PSDataCollection<PSObject> result = await powerShell.InvokeAsync();

            foreach (PSObject cmd in result)
            {
                _possibleParameters.Add($"-{cmd}");
            }
        }
    }

    /// <summary>
    /// Loads the possible parameters for the specified command into the '_possibleParameters' collection.
    /// </summary>
    /// <param name="commandObject">The PowerShell command object whose parameters are to be loaded.</param>
    void ICommandParameters.LoadCommandParameters(Command? commandObject)
    {
        // commandObject can be null if the user attempts to select an ActiveDirectory command that doesn't exist.
        // More specifically, if the entered command doesn't exist in the ActiveDirectoryCommandList in
        // MainWindowViewModel.cs, commandObject will be null, causing an exception to be thrown, crashing the program.
        if (commandObject is null)
        {
            Trace.WriteLine("Error: commandObject is null");
            _possibleParameters.Add("No valid command provided");
            return;
        }

        if (_possibleParameters.Count == 0)
        {
            using var powerShell = System.Management.Automation.PowerShell.Create();
            string commandString =
                $"Get-Command {commandObject.CommandText} | Select -ExpandProperty Parameters | ForEach-Object {{$_.Keys }}";

            powerShell.Commands.Clear();
            powerShell.AddScript(commandString);
            Collection<PSObject> result = powerShell.Invoke();

            foreach (PSObject cmd in result)
            {
                _possibleParameters.Add($"-{cmd}");
            }
        }
    }
}
