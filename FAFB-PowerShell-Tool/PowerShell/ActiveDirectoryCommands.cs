using System.Collections.ObjectModel;

namespace FAFB_PowerShell_Tool.PowerShell;

public static class ActiveDirectoryCommands
{
    public static async Task<ObservableCollection<Command>> GetActiveDirectoryCommands()
    {
        PowerShellExecutor powerShellExecutor = new();
        ObservableCollection<Command> commandList = new();
        List<string> commandListTemp = await powerShellExecutor.ExecuteAsync("Get-Command -Module ActiveDirectory");
        
        foreach (var command in commandListTemp)
        {
            commandList.Add(new Command(command));
        }

        return commandList;
    }
}