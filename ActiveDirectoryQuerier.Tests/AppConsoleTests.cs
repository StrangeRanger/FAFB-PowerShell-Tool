using System.Management.Automation.Runspaces;
using ActiveDirectoryQuerier.PowerShell;

namespace ActiveDirectoryQuerier.Tests;

public class AppConsoleTests : IDisposable
{
    // TODO: Make sure this is how I should perform cleanups...
    public void Dispose()
    {
        if (File.Exists("output.txt"))
        {
            File.Delete("output.txt");
        }

        if (File.Exists("output.csv"))
        {
            File.Delete("output.csv");
        }

        GC.SuppressFinalize(this);
    }

    private static async Task<(AppConsole, ReturnValues)> ExecuteCommandAsync(Command command)
    {
        PowerShellExecutor powerShellExecutor = new();
        AppConsole appConsole = new();
        ReturnValues result = await powerShellExecutor.ExecuteAsync(command);

        appConsole.Append(result.HadErrors ? result.StdErr : result.StdOut);

        return (appConsole, result);
    }

    [Fact]
    public async Task ClearConsole_ClearsConsole_SuccessfullyCleared()
    {
        // Arrange
        Command command = new("Get-Process");
        command.Parameters.Add("Name", "explorer");
        var (appConsole, _) = await ExecuteCommandAsync(command);

        // Act
        appConsole.Clear();

        // Assert
        Assert.Empty(appConsole.ConsoleOutput);
    }

    [Fact]
    public void Append_AppendsStringToConsole_SuccessfullyAppended()
    {
        // Arrange
        AppConsole appConsole = new();
        const string output = "Output";

        // Act
        appConsole.Append(output);

        // Assert
        Assert.Equal(output, appConsole.ConsoleOutput);
    }

    [Fact]
    public async Task ExportToText_ExportToText_SuccessfullyExported()
    {
        // Arrange
        Command command = new("Get-Process");
        command.Parameters.Add("Name", "explorer");
        var (appConsole, returnValues) = await ExecuteCommandAsync(command);

        // Act
        appConsole.ExportToText();
        string fileContents = await File.ReadAllTextAsync("output.txt");

        // Assert
        Assert.True(File.Exists("output.txt"));

        Assert.Equal(returnValues.HadErrors ? string.Join(Environment.NewLine, returnValues.StdErr)
                                            : string.Join(Environment.NewLine, returnValues.StdOut),
                     fileContents);
    }
}
