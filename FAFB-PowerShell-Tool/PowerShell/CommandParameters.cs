using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;

namespace FAFB_PowerShell_Tool.PowerShell;

public class CommandParameters
{
    private readonly ObservableCollection<string> _possibleParameters = new();
    public ObservableCollection<string> PossibleParameters
    {
        get {
            if (_possibleParameters.Count != 0)
            {
                return _possibleParameters;
            }
            
            throw new InvalidOperationException("PossibleParameters has not been populated via 'LoadCommandParametersAsync'.");
        }
    }
    
    /// <summary>
    /// Loads the possible parameters for the selected command into the '_possibleParameters' collection.
    /// </summary>
    /// <returns>A collection of parameter names for 'CommandName'</returns>
    public async Task LoadCommandParametersAsync(Command commandObject)
    {
        string commandString = $"Get-Command {commandObject.CommandText} | Select -ExpandProperty Parameters | ForEach-Object {{ $_.Keys }}";

        if (_possibleParameters.Count == 0)
        {
            PowerShellExecutor powerShellExecutor = new();
            ReturnValues tmpList = await powerShellExecutor.ExecuteAsync(commandString);

            foreach (var command in tmpList.StdOut)
            {
                _possibleParameters.Add($"-{command}");
            }
        }
    }
}