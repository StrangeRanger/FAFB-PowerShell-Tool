using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Windows;

namespace ActiveDirectoryQuerier.PowerShell;

public static class ActiveDirectoryCommandFetcher
{
    public static async Task<ObservableCollection<Command>> GetActiveDirectoryCommands()
    {
        Command powerShellCommand = new("Get-Command");
        powerShellCommand.Parameters.Add("Module", "ActiveDirectory");
        PowerShellExecutor powerShellExecutor = new();
        ObservableCollection<Command> activeDirectoryCommands = new();
        ReturnValues powerShellOutput = await powerShellExecutor.ExecuteAsync(powerShellCommand);

        // This is more of an internal error catch, as even through this command shouldn't fail, it's possible that it
        // could. If this is the case, we want to know about it.
        if (powerShellOutput.HadErrors)
        {
            string errorMessage = "Internal Error: An error occurred while retrieving the Active Directory commands: " +
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
