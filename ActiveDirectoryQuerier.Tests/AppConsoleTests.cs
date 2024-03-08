using System.Management.Automation.Runspaces;
using ActiveDirectoryQuerier.PowerShell;

namespace ActiveDirectoryQuerier.Tests;

public class AppConsoleTests
{
    [Fact]
    public void ClearConsole_ClearsConsole_SuccessfullyCleared()
    {
        // Arrange
        PowerShellExecutor powerShellExecutor = new();
        Command command = new("Get-Process");
        AppConsole appConsole = new();

        // Act
        command.Parameters.Add("Name", "explorer");
        // TODO: Maybe implement an interface of PowerShellExecutor and use a mock object to test to prevent execution
        // errors.
        var result = powerShellExecutor.Execute(command);

        if (result.HadErrors)
        {
            throw new Exception("Error occurred while executing command");
        }

        appConsole.Append(result.StdOut);
        appConsole.ClearConsole();

        // Assert
        Assert.Empty(appConsole.ConsoleOutput);
    }

    [Fact]
    public void Append_AppendsStringToConsole_SuccessfullyAppended()
    {
        // Arrange
        AppConsole appConsole = new();
        string output = "Output";

        // Act
        appConsole.Append(output);

        // Assert
        Assert.Equal(output, appConsole.ConsoleOutput);
    }
}
