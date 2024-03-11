using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Windows;

namespace ActiveDirectoryQuerier.PowerShell;

// ReSharper disable once InconsistentNaming
public static class ADCommandsFetcher
{
    // TODO: Possibly rename this method to GetADCommandList...
    public static async Task<ObservableCollection<Command>> GetActiveDirectoryCommands()
    {
        Command psCommand = new("Get-Command");
        psCommand.Parameters.Add("Module", "ActiveDirectory");
        PSExecutor psExecutor = new();
        ObservableCollection<Command> adCommands = new();
        PSOutput psOutput = await psExecutor.ExecuteAsync(psCommand);

        // This is more of an internal error catch, as even through this command shouldn't fail, it's possible that it
        // could. If this is the case, we want to know about it.
        if (psOutput.HadErrors)
        {
            string errorMessage = "Internal Error: An error occurred while retrieving the Active Directory commands: " +
                                  $"({string.Join(" ", psOutput.StdErr)})";
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw new InvalidPowerShellStateException(errorMessage);
        }

        foreach (string command in psOutput.StdOut)
        {
            adCommands.Add(new Command(command));
        }

        return adCommands;
    }
}