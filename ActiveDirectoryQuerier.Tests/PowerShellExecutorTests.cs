using System.Management.Automation.Runspaces;
using ActiveDirectoryQuerier.PowerShell;

namespace ActiveDirectoryQuerier.Tests;

public class PowerShellExecutorTests
{
    [Theory]
    [InlineData("Get-Command", "Module", "ActiveDirectory")]
    [InlineData("Get-Process", "Name", "explorer")]
    public void Execute_WhenGivenValidCommand_ReturnsExpectedOutput(string cmd, string paramName, string paramValue)
    {
        // Arrange
        Command? command = new(cmd);
        command.Parameters.Add(paramName, paramValue);
        PowerShellExecutor powerShellExecutor = new();

        // Act
        ReturnValues result = powerShellExecutor.Execute(command);

        // Assert
        Assert.False(result.HadErrors);
        Assert.Empty(result.StdErr);
        Assert.NotEmpty(result.StdOut);
    }

    [Fact]
    public void Execute_CheckIfOutputChanged_ReturnsDifferentOutput()
    {
        // Arrange
        Command? command = new("Get-Command");
        command.Parameters.Add("Module", "ActiveDirectory");
        Command? command2 = new("Get-Process");
        command.Parameters.Add("Name", "explorer");
        PowerShellExecutor powerShellExecutor = new();

        // Act
        ReturnValues result = powerShellExecutor.Execute(command);
        ReturnValues result2 = powerShellExecutor.Execute(command2);

        // Assert
        Assert.NotEqual(result, result2);
    }

    [Theory]
    [InlineData("Get-Command", "Module", "ActiveDirectory")]
    [InlineData("Get-Process", "Name", "explorer")]
    public async void ExecuteAsync_WhenGivenValidCommand_ReturnsExpectedOutput(string cmd,
                                                                               string paramName,
                                                                               string paramValue)
    {
        // Arrange
        Command? command = new(cmd);
        command.Parameters.Add(paramName, paramValue);
        PowerShellExecutor powerShellExecutor = new();

        // Act
        ReturnValues result = await powerShellExecutor.ExecuteAsync(command);

        // Assert
        Assert.False(result.HadErrors);
        Assert.Empty(result.StdErr);
        Assert.NotEmpty(result.StdOut);
    }

    [Theory]
    [InlineData("Get-ADUser", "InvalidParameter", "*")]
    [InlineData("InvalidCommand", "Filter", "*")]
    public void Execute_WhenGivenInvalidCommand_ReturnsExpectedOutput(string cmd, string paramName, string paramValue)
    {
        // Arrange
        Command? command = new(cmd);
        command.Parameters.Add(paramName, paramValue);
        PowerShellExecutor powerShellExecutor = new();

        // Act
        ReturnValues result = powerShellExecutor.Execute(command);

        // Assert
        Assert.True(result.HadErrors);
        Assert.NotEmpty(result.StdErr);
        Assert.Empty(result.StdOut);
    }

    [Theory]
    [InlineData("Get-ADUser", "InvalidParameter", "*")]
    [InlineData("InvalidCommand", "Filter", "*")]
    public async void ExecuteAsync_WhenGivenInvalidCommand_ReturnsExpectedOutput(string cmd,
                                                                                 string paramName,
                                                                                 string paramValue)
    {
        // Arrange
        Command? command = new(cmd);
        command.Parameters.Add(paramName, paramValue);
        PowerShellExecutor powerShellExecutor = new();

        // Act
        ReturnValues result = await powerShellExecutor.ExecuteAsync(command);

        // Assert
        Assert.True(result.HadErrors);
        Assert.NotEmpty(result.StdErr);
        Assert.Empty(result.StdOut);
    }
}
