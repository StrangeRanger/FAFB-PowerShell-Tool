using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace ActiveDirectoryQuerier.PowerShell;

/// <summary>
/// Manages and provides the parameters available for a given PowerShell command.
/// </summary>
public class CommandParameters : ICommandParameters
{
    private readonly ObservableCollection<string> _possibleParameters = new();

    /// <summary>
    /// Gets the collection of possible parameters for a command.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the collection has not been populated yet.</exception>
    /// <note>
    /// I realize that this isn't a very clean way of doing things, but it's the best I could come up with at the time.
    /// </note>
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
    /// Retrieves the parameters available for a given command asynchronously.
    /// </summary>
    /// <param name="commandObject">The command object to retrieve parameters for.</param>
    public async Task LoadCommandParametersAsync(Command? commandObject)
    {
        await LoadCommandParametersInternal(commandObject, true);
    }

    /// <summary>
    /// Retrieves the parameters available for a given command synchronously.
    /// </summary>
    /// <param name="commandObject">The command object to retrieve parameters for.</param>
    void ICommandParameters.LoadCommandParameters(Command? commandObject)
    {
        LoadCommandParametersInternal(commandObject, false).Wait();
    }

    /// <summary>
    /// Retrieves the parameters available for a given command.
    /// </summary>
    /// <param name="commandObject">The command object to retrieve parameters for.</param>
    /// <param name="isAsync">Determines whether the operation should be asynchronous or not.</param>
    private async Task LoadCommandParametersInternal(Command? commandObject, bool isAsync)
    {
        // commandObject can be null if the user attempts to select an ActiveDirectory command that doesn't exist.
        // More specifically, if the entered command doesn't exist in the ActiveDirectoryCommandsList in
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

            if (isAsync)
            {
                PSDataCollection<PSObject> result = await powerShell.InvokeAsync();
                foreach (PSObject cmd in result)
                {
                    _possibleParameters.Add($"-{cmd}");
                }
            }
            else
            {
                Collection<PSObject> result = powerShell.Invoke();
                foreach (PSObject cmd in result)
                {
                    _possibleParameters.Add($"-{cmd}");
                }
            }
        }
    }
}
