using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace FAFB_PowerShell_Tool.PowerShell;

/// <summary>
/// Provides functionalities related to the ActiveDirectory PowerShell module.
/// </summary>
public static class ActiveDirectoryCommands
{
    /// <summary>
    /// Retrieves a list of commands available in the ActiveDirectory PowerShell module.
    /// </summary>
    /// <returns>
    /// An ObservableCollection of Command objects representing each command in the ActiveDirectory module.
    /// </returns>
    /// <exception cref="InvalidPowerShellStateException">
    /// Thrown if an error occurs during the execution of the PowerShell command.
    /// </exception>
    public static async Task<ObservableCollection<Command>> GetActiveDirectoryCommands()
    {
        string commandString = "Get-Command -Module ActiveDirectory";
        PowerShellExecutor powerShellExecutor = new();
        ObservableCollection<Command> commandList = new();
        ReturnValues commandListTemp = await powerShellExecutor.ExecuteAsync(commandString);

        // TODO: Enhance exception handling with more detailed information.
        if (commandListTemp.HadErrors)
        {
            MessageBoxOutput.Show(string.Join(" ", commandListTemp.StdErr), MessageBoxOutput.OutputType.Error);
            throw new InvalidPowerShellStateException();
        }

        foreach (var command in commandListTemp.StdOut)
        {
            commandList.Add(new Command(command));
        }

        return commandList;
    }
}
