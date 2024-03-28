using System.Management.Automation.Runspaces;
using ActiveDirectoryQuerier.PowerShell;
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertConstructorToMemberInitializers

namespace ActiveDirectoryQuerier.Tests;

public class PSExecutorTests
{
    private readonly PSExecutor _psExecutor;

    public PSExecutorTests()
    {
        // Arrange
        _psExecutor = new PSExecutor();
    }

    [Theory]
    [InlineData("Get-Command", "Module", "ActiveDirectory")]
    [InlineData("Get-Process", "Name", "explorer")]
    public void Execute_WhenGivenValidCommand_ReturnsExpectedOutput(string command,
                                                                    string parameter,
                                                                    string parameterValue)
    {
        // Arrange
        Command psCommand = new(command);
        psCommand.Parameters.Add(parameter, parameterValue);

        // Act
        PSOutput result = _psExecutor.Execute(psCommand);

        // Assert
        Assert.False(result.HadErrors);
        Assert.Empty(result.StdErr);
        Assert.NotEmpty(result.StdOut);
    }

    [Fact]
    public void Execute_CheckIfOutputChanged_ReturnsDifferentOutput()
    {
        // Arrange
        Command command = new("Get-Command");
        command.Parameters.Add("Module", "ActiveDirectory");
        Command command2 = new("Get-Process");
        command.Parameters.Add("Name", "explorer");

        // Act
        PSOutput result = _psExecutor.Execute(command);
        PSOutput result2 = _psExecutor.Execute(command2);

        // Assert
        Assert.NotEqual(result, result2);
    }

    [Theory]
    [InlineData("Get-Command", "Module", "ActiveDirectory")]
    [InlineData("Get-Process", "Name", "explorer")]
    public async Task ExecuteAsync_WhenGivenValidCommand_ReturnsExpectedOutput(string command,
                                                                               string parameter,
                                                                               string parameterValue)
    {
        // Arrange
        Command psCommand = new(command);
        psCommand.Parameters.Add(parameter, parameterValue);

        // Act
        PSOutput result = await _psExecutor.ExecuteAsync(psCommand);

        // Assert
        Assert.False(result.HadErrors);
        Assert.Empty(result.StdErr);
        Assert.NotEmpty(result.StdOut);
    }

    [Theory]
    [InlineData("Get-ADUser", "InvalidParameter", "*")]
    [InlineData("InvalidCommand", "Filter", "*")]
    public void Execute_WhenGivenInvalidCommand_ReturnsExpectedOutput(string command,
                                                                      string parameter,
                                                                      string parameterValue)
    {
        // Arrange
        Command psCommand = new(command);
        psCommand.Parameters.Add(parameter, parameterValue);

        // Act
        PSOutput result = _psExecutor.Execute(psCommand);

        // Assert
        Assert.True(result.HadErrors);
        Assert.NotEmpty(result.StdErr);
        Assert.Empty(result.StdOut);
    }

    [Theory]
    [InlineData("Get-ADUser", "InvalidParameter", "*")]
    [InlineData("InvalidCommand", "Filter", "*")]
    public async Task ExecuteAsync_WhenGivenInvalidCommand_ReturnsExpectedOutput(string command,
                                                                                 string parameter,
                                                                                 string parameterValue)
    {
        // Arrange
        Command psCommand = new(command);
        psCommand.Parameters.Add(parameter, parameterValue);

        // Act
        PSOutput result = await _psExecutor.ExecuteAsync(psCommand);

        // Assert
        Assert.True(result.HadErrors);
        Assert.NotEmpty(result.StdErr);
        Assert.Empty(result.StdOut);
    }

    [Theory]
    [InlineData("Get-Command", "Module", "ActiveDirectory")]
    [InlineData("Get-Process", "Name", "explorer")]
    public async Task ExecuteAsync_ExecuteToCsv_ReturnsExpectedOutput(string command,
                                                                      string parameter,
                                                                      string parameterValue)
    {
        // Arrange
        Command psCommand = new(command);
        psCommand.Parameters.Add(parameter, parameterValue);

        // Act
        PSOutput result = await _psExecutor.ExecuteAsync(psCommand, OutputFormat.Csv);

        // Assert
        Assert.False(result.HadErrors);
        Assert.Empty(result.StdErr);
        Assert.NotEmpty(result.StdOut);
    }
}
