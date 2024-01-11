using System.Collections.ObjectModel;

namespace FAFB_PowerShell_Tool.PowerShell.Commands;

public class GuiCommand : InternalCommand
{
    private readonly ObservableCollection<string> _possibleParameters = new();
    public ObservableCollection<string> PossibleParameters
    {
        get {
            if (_possibleParameters.Count != 0)
            {
                return _possibleParameters;
            }
            throw new InvalidOperationException(
                "PossibleParameters has not been populated via 'LoadCommandParametersAsync'.");
        }
    }
    
    /// <summary>
    /// Commands that are intended to be selected and used via the GUI.
    /// </summary>
    /// <param name="commandName">...</param>
    /// <param name="parameters">...</param>
    public GuiCommand(string commandName, string[]? parameters = null) : base(commandName, parameters)
    { }

    /// <summary>
    /// Retrieves a collection of parameter names for 'CommandName'.
    /// </summary>
    /// <returns>A collection of parameter names for 'CommandName'</returns>
    public async Task LoadCommandParametersAsync()
    {
        
        GuiCommand guiCommandString = new("Get-Command", new[] { CommandName, $"| Select -ExpandProperty Parameters | ForEach-Object {{ $_.Keys }}" });
        
        if (_possibleParameters.Count == 0)
        {
            PowerShellExecutor powerShellExecutor = new();
            ExecuteReturnValues tmpList = await powerShellExecutor.ExecuteAsync(guiCommandString);

            foreach (var command in tmpList.StdOut)
            {
                _possibleParameters.Add($"-{command}");
            }
        }
    }
}
