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
            
            _commandName = value;
        }
    }
    
    public ObservableCollection<string> PossibleParameters { get; } = new();
    // TODO: Figure out where the 'Parameters' property is used and if it is needed.
    public string[]? Parameters { get; set; }
    
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
        if (PossibleParameters.Count == 0)
        {
            PowerShellExecutor powerShellExecutor = new();
            List<string> tmpList = await powerShellExecutor.ExecuteAsync("Get-Command " + _commandName + " | Select -ExpandProperty Parameters | ForEach-Object { $_.Keys }");
                    
            foreach (var command in tmpList)
            {
                PossibleParameters.Add("-" + command);
            }
        }
        
    }
}
