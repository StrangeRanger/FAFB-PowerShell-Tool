using System.Management.Automation.Runspaces;
using FAFB_PowerShell_Tool.PowerShell;

namespace FAFB_PowerShell_Tool.Tests;

public class PowerShellExecutorTests
{
    [Theory]
    [InlineData("Get-Command", "Module", "ActiveDirectory")]
    [InlineData("Get-Process", "Name", "powershell")]
    public void Execute_WhenGivenValidCommand_ReturnsExpectedOutput(string cmd, string paramName, string paramValue)
    {
        // Arrange
        Command command = new(cmd);
        command.Parameters.Add(paramName, paramValue);
        PowerShellExecutor powerShellExecutor = new();

        // Act
        ReturnValues result = powerShellExecutor.Execute(command);

        // Assert
        Assert.False(result.HadErrors);
        Assert.Empty(result.StdErr);
        Assert.NotEmpty(result.StdOut);
    }

    [Theory]
    [InlineData("Get-Command", "Module", "ActiveDirectory")]
    [InlineData("Get-Process", "Name", "powershell")]
    public async void ExecuteAsync_WhenGivenValidCommand_ReturnsExpectedOutput(string cmd,
                                                                               string paramName,
                                                                               string paramValue)
    {
        // Arrange
        Command command = new(cmd);
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
        Command command = new(cmd);
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
        Command command = new(cmd);
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
