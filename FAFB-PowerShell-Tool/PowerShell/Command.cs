using System.Collections.ObjectModel;

namespace FAFB_PowerShell_Tool.PowerShell;

public class Command
{
    private readonly string _commandName = null!;
    public string CommandName
    {
        get => _commandName;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Command name cannot be null or whitespace.", nameof(CommandName));
            }
            
            _commandName = value.Trim();
        }
    }

    // TODO: Figure out where the 'Parameters' property is used and if it is needed.
    public string[]? Parameters { get; set; }

    private readonly ObservableCollection<string> _possibleParameters = new();
    public ObservableCollection<string> PossibleParameters
    {
        get
        {
            if (_possibleParameters.Count != 0) { return _possibleParameters; }
            throw new InvalidOperationException("PossibleParameters has not been populated via 'LoadCommandParametersAsync'.");
        }
    }
    
    public Command(string commandName, string[]? parameters = null)
    {
        CommandName = commandName;
        Parameters = parameters;
    }
    
    /// <summary>
    /// Retrieves a collection of parameter names for '_commandName'.
    /// </summary>
    /// <returns>A collection of parameter names for '_commandName'</returns>
    public async Task LoadCommandParametersAsync()
    {
        if (_possibleParameters.Count == 0)
        {
            PowerShellExecutor powerShellExecutor = new();
            ExecuteReturnValues tmpList = await powerShellExecutor.ExecuteAsync("Get-Command " + _commandName + " | Select -ExpandProperty Parameters | ForEach-Object { $_.Keys }");
                    
            foreach (var command in tmpList.StdOut)
            {
                _possibleParameters.Add("-" + command);
            }
        }
        
    }
}
