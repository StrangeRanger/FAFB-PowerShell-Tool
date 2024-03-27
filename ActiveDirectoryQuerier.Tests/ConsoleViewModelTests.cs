using System.Management.Automation.Runspaces;
using ActiveDirectoryQuerier.PowerShell;
using ActiveDirectoryQuerier.ViewModels;

namespace ActiveDirectoryQuerier.Tests;

public class ConsoleViewModelTests : IDisposable
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

    private static async Task<(ConsoleViewModel, PSOutput)> ExecuteCommandAsync(Command command)
    {
        PSExecutor psExecutor = new();
        ConsoleViewModel consoleViewModel = new();
        PSOutput result = await psExecutor.ExecuteAsync(command);

        consoleViewModel.Append(result.HadErrors ? result.StdErr : result.StdOut);

        return (consoleViewModel, result);
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
        Assert.Empty(appConsole.GetConsoleOutput);
    }

    [Fact]
    public void Append_AppendsStringToConsole_SuccessfullyAppended()
    {
        // Arrange
        ConsoleViewModel consoleViewModel = new();
        const string output = "Output";

        // Act
        consoleViewModel.Append(output);

        // Assert
        Assert.Equal(output, consoleViewModel.GetConsoleOutput);
    }

    [Fact]
    public async Task ExportToText_ExportToText_SuccessfullyExported()
    {
        // Arrange
        Command command = new("Get-Process");
        command.Parameters.Add("Name", "explorer");
        var (appConsole, returnValues) = await ExecuteCommandAsync(command);

        // Act
        appConsole.ExportToTextFile();
        string fileContents = await File.ReadAllTextAsync("output.txt");

        // Assert
        Assert.True(File.Exists("output.txt"));

        Assert.Equal(returnValues.HadErrors ? string.Join(Environment.NewLine, returnValues.StdErr)
                                            : string.Join(Environment.NewLine, returnValues.StdOut),
                     fileContents);
    }
}
