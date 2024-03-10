using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace ActiveDirectoryQuerier.PowerShell;

/// <summary>
/// Manages and provides the parameters available for a given PowerShell command.
/// </summary>
public class CommandParameters
{
    private readonly ObservableCollection<string> _possibleParameters = new();

    /// <summary>
    /// Gets the collection of possible parameters for a command.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the collection has not been populated yet.</exception>
    /// <important>
    /// This property should not be accessed before the collection has been populated via LoadCommandParametersAsync, to
    /// ensure asynchronous loading of parameters.
    /// </important>
    public ObservableCollection<string> PossibleParameters
    {
        get {
            if (_possibleParameters.Count == 0)
            {
                Debug.WriteLine("Warning: LoadCommandParametersAsync should be called before accessing " +
                                "PossibleParameters, to ensure asynchronous loading of parameters.");
                LoadCommandParameters(null);
            }

            return _possibleParameters;
        }
    }

    /// <summary>
    /// Retrieves the parameters available for a given command asynchronously.
    /// </summary>
    /// <param name="powerShellCommand">The command object to retrieve parameters for.</param>
    public async Task LoadCommandParametersAsync(Command? powerShellCommand)
    {
        await LoadCommandParametersInternal(powerShellCommand, true);
    }

    /// <summary>
    /// Retrieves the parameters available for a given command synchronously.
    /// </summary>
    /// <param name="powerShellCommand">The command to retrieve parameters for.</param>
    public void LoadCommandParameters(Command? powerShellCommand)
    {
        LoadCommandParametersInternal(powerShellCommand, false).Wait();
    }

    /// <summary>
    /// Retrieves the parameters available for a given command.
    /// </summary>
    /// <param name="powerShellCommand">The command to retrieve parameters for.</param>
    /// <param name="isAsync">Determines whether the operation should be asynchronous or not.</param>
    private async Task LoadCommandParametersInternal(Command? powerShellCommand, bool isAsync)
    {
        // powerShellCommand can be null if the user attempts to select an ActiveDirectory command that doesn't exist.
        // More specifically, if the entered command doesn't exist in the ActiveDirectoryCommandsList in
        // MainWindowViewModel.cs, powerShellCommand will be null, causing an exception to be thrown, crashing the
        // program.
        if (powerShellCommand is null)
        {
            Trace.WriteLine("Error: command is null");
            _possibleParameters.Add("No valid command provided");
            return;
        }

        if (_possibleParameters.Count == 0)
        {
            using var powerShell = System.Management.Automation.PowerShell.Create();
            string commandString =
                $"Get-Command {powerShellCommand.CommandText} | Select -ExpandProperty Parameters | ForEach-Object {{ $_.Keys }}";

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
