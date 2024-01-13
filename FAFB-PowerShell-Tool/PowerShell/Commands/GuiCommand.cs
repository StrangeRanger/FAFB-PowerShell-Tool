using System.Collections.ObjectModel;

namespace FAFB_PowerShell_Tool.PowerShell.Commands;

/// <summary>
/// Commands that are intended to be selected and used via the GUI.
/// </summary>
/// <remarks>
/// In addition to the base class, this class also contains a collection of possible parameters for the selected
/// command.
/// </remarks>
/// <param name="commandName">The selected command.</param>
/// <param name="parameters">Selected parameters for 'commandName'.</param>
public class GuiCommand(string commandName, string[]? parameters = null) : InternalCommand(commandName, parameters)
{
    private readonly ObservableCollection<string> _possibleParameters = new();
    // NOTE: Calling 'LoadCommandParametersAsync' from this property's getter will cause the application to hang. This
    //       is why the property throws an exception if the collection is empty, and requires the user to call the
    //       'LoadCommandParametersAsync' method before accessing the collection.
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
    /// Loads the possible parameters for the selected command into the '_possibleParameters' collection.
    /// </summary>
    /// <returns>A collection of parameter names for 'CommandName'</returns>
    public async Task LoadCommandParametersAsync()
    {
        GuiCommand guiCommandString = new("Get-Command", new[] { CommandName, "| Select -ExpandProperty Parameters | ForEach-Object { $_.Keys }" });

        if (_possibleParameters.Count == 0)
        {
            PowerShellExecutor powerShellExecutor = new();
            ReturnValues tmpList = await powerShellExecutor.ExecuteAsync(guiCommandString);

            foreach (var command in tmpList.StdOut)
            {
                _possibleParameters.Add($"-{command}");
            }
        }
    }
}
