using System.Collections.ObjectModel;

namespace FAFB_PowerShell_Tool;

public class Command
{
    public string CommandName { get; }
    public string[]? Parameters { get; set; }
    private int? _parameterCount;
    
    public Command(string commandName, string[]? parameters = null)
    {
        CommandName = commandName;
        Parameters = parameters;
        _parameterCount = parameters?.Length;
    }

    public static ObservableCollection<Command> GetActiveDirectoryCommands()
    {
        PowerShellExecutor powerShellExecutor = new();
        ObservableCollection<Command> commandList = new();
        List<string> commandListTemp = powerShellExecutor.Execute("Get-Command -Module ActiveDirectory");
        
        foreach (var command in commandListTemp)
        {
            commandList.Add(new Command(command));
        }

        return commandList;
    }
    
    public static string[] GetCommandParameters(string commandName)
    {
        // TODO: Implement this method.
        return null!;
    }
}
