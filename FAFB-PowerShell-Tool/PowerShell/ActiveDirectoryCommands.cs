using System.Collections.ObjectModel;
using System.Management.Automation;

namespace FAFB_PowerShell_Tool.PowerShell;

public static class ActiveDirectoryCommands
{
    public static async Task<ObservableCollection<Command>> GetActiveDirectoryCommands()
    {
        PowerShellExecutor powerShellExecutor = new();
        ObservableCollection<Command> commandList = new();
        ExecuteReturnValues commandListTemp = await powerShellExecutor.ExecuteAsync("Get-Command -Module ActiveDirectory");
        
         if (commandListTemp.HadErrors)
         {
             MessageBoxOutput.Show(string.Join(" ", commandListTemp.StdOut), MessageBoxOutput.OutputType.Error);
             throw new InvalidPowerShellStateException(); // TODO: ???
         }
        
        foreach (var command in commandListTemp.StdOut)
        {
            commandList.Add(new Command(command));
        }

        return commandList;
    }
}