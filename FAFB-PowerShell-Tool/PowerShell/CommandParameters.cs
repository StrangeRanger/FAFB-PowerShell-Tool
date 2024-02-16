using System.Collections.ObjectModel;
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
    public async Task LoadCommandParametersAsync(Command commandObject)
    {
        string commandString =
            $"Get-Command {commandObject.CommandText} | Select -ExpandProperty Parameters | ForEach-Object {{ $_.Keys }}";

        if (_possibleParameters.Count == 0)
        {
            PowerShellExecutor powerShellExecutor = new();
            ReturnValues tmpList = await powerShellExecutor.ExecuteAsync(commandString);

            foreach (string command in tmpList.StdOut)
            {
                _possibleParameters.Add($"-{command}");
            }
        }
    }
}
