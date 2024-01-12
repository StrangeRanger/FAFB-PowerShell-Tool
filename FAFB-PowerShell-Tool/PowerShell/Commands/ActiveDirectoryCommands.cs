using System.Collections.ObjectModel;
using System.Management.Automation;

namespace FAFB_PowerShell_Tool.PowerShell.Commands;

public static class ActiveDirectoryCommands
{
    public static async Task<ObservableCollection<GuiCommand>> GetActiveDirectoryCommands()
    {
        PowerShellExecutor powerShellExecutor = new();
        ObservableCollection<GuiCommand> commandList = new();
        InternalCommand commandString = new("Get-Command", new[] { "-Module", "ActiveDirectory" });
        ReturnValues commandListTemp = await powerShellExecutor.ExecuteAsync(commandString);

        if (commandListTemp.HadErrors)
        {
            MessageBoxOutput.Show(string.Join(" ", commandListTemp.StdErr), MessageBoxOutput.OutputType.Error);
            throw new InvalidPowerShellStateException();  // TODO: Make exception output more info...
        }

        foreach (var command in commandListTemp.StdOut)
        {
            commandList.Add(new GuiCommand(command));
        }

        return commandList;
    }
}
