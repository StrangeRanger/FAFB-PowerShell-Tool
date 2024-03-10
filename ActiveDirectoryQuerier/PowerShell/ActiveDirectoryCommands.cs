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
        Command powerShellCommand = new("Get-Command");
        powerShellCommand.Parameters.Add("Module", "ActiveDirectory");
        PowerShellExecutor powerShellExecutor = new();
        ObservableCollection<Command> activeDirectoryCommands = new();
        ReturnValues powerShellOutput = await powerShellExecutor.ExecuteAsync(powerShellCommand);

        // This is more of an internal error catch, as even through this command shouldn't fail, it's possible that it
        // could.
        if (powerShellOutput.HadErrors)
        {
            string errorMessage = "An error occurred while retrieving the Active Directory commands: " +
                                  $"({string.Join(" ", powerShellOutput.StdErr)})";
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw new InvalidPowerShellStateException(errorMessage);
        }

        foreach (string command in powerShellOutput.StdOut)
        {
            activeDirectoryCommands.Add(new Command(command));
        }

        return activeDirectoryCommands;
    }
}
