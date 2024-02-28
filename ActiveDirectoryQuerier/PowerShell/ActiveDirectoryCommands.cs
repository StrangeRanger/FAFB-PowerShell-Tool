using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Windows;

namespace ActiveDirectoryQuerier.PowerShell;

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
        Command command = new("Get-Command");
        command.Parameters.Add("Module", "ActiveDirectory");
        PowerShellExecutor powerShellExecutor = new();
        ObservableCollection<Command> commandList = new();
        ReturnValues commandListTemp = await powerShellExecutor.ExecuteAsync(command);

        // NOTE: This is more of an internal error...
        // TODO: Provide a more detailed error message??? Maybe log the error to a file???
        if (commandListTemp.HadErrors)
        {
            MessageBox.Show(string.Join(" ", commandListTemp.StdErr),
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
            throw new InvalidPowerShellStateException(
                "An error occurred while retrieving the ActiveDirectory commands.");
        }

        foreach (var cmd in commandListTemp.StdOut)
        {
            commandList.Add(new Command(cmd));
        }

        return commandList;
    }
}
