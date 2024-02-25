using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace FAFB_PowerShell_Tool.PowerShell;

/// <summary>
/// Manages and provides the parameters available for a given PowerShell command.
/// </summary>
public class CommandParameters
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
                    "PossibleParameters has not been populated via 'LoadCommandParametersAsync'.");
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
        // Check if commandObject is null
        if (commandObject is null)
        {
            Trace.WriteLine("Error: commandObject is null");
            _possibleParameters.Add("No valid command provided");
            return;
        }

        if (_possibleParameters.Count == 0)
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                // The command exists, get its parameters
                string commandString =
                    $"Get-Command {commandObject.CommandText} | Select -ExpandProperty Parameters | ForEach-Object {{ $_.Keys }}";

                ps.Commands.Clear();
                ps.AddScript(commandString);
                PSDataCollection<PSObject> result = await ps.InvokeAsync();

                foreach (PSObject cmd in result)
                {
                    _possibleParameters.Add($"-{cmd}");
                }
            }
        }
    }

    public void LoadCommandParameters(Command? commandObject)
    {
        // Check if commandObject is null
        if (commandObject is null)
        {
            Trace.WriteLine("Error: commandObject is null");
            _possibleParameters.Add("No valid command provided");
            return;
        }

        if (_possibleParameters.Count == 0)
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                // The command exists, get its parameters
                string commandString =
                    $"Get-Command {commandObject.CommandText} | Select -ExpandProperty Parameters | ForEach-Object {{ $_.Keys }}";

                ps.Commands.Clear();
                ps.AddScript(commandString);
                Collection<PSObject> result = ps.Invoke();

                foreach (PSObject cmd in result)
                {
                    _possibleParameters.Add($"-{cmd}");
                }
            }
        }
    }
}
