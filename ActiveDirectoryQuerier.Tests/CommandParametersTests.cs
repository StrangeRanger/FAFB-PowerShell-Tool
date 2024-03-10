using System.Management.Automation.Runspaces;
using ActiveDirectoryQuerier.PowerShell;

namespace ActiveDirectoryQuerier.Tests;

public class CommandParametersTests
{
    [Fact]
    public void PossibleParameters_LoadCommandParametersNotPopulated_NoValidCommandProvided()
    {
        // Arrange
        CommandParameters commandParameters = new();

        // Assert
        Assert.Contains("No valid command provided", commandParameters.PossibleParameters);
    }

    [Fact]
    public async Task LoadCommandParametersAsync_PopulatesPossibleParameters_IsNotEmpty()
    {
        // Arrange
        CommandParameters commandParameters = new();
        Command command = new("Get-Process");

        // Act
        await commandParameters.LoadCommandParametersAsync(command);

        // Assert
        Assert.NotEmpty(commandParameters.PossibleParameters);
    }

    [Fact]
    public async Task LoadCommandParametersAsync_CheckPossibleParameter_ContentIsCorrect()
    {
        // Arrange
        CommandParameters commandParameters = new();
        Command command = new("Get-Process");

        // Act
        await commandParameters.LoadCommandParametersAsync(command);

        // Assert
        Assert.Contains("-Name", commandParameters.PossibleParameters);
        Assert.Contains("-Id", commandParameters.PossibleParameters);
    }
}
