using System.Management.Automation.Runspaces;
using FAFB_PowerShell_Tool.PowerShell;

namespace FAFB_PowerShell_Tool.Tests;

public class CommandParametersTests
{
    [Fact]
    public void PossibleParameters_LoadCommandParametersAsyncNotPopulated_ThrowInvalidOperationException()
    {
        // Arrange
        CommandParameters commandParameters = new();

        // Assert
        Assert.Throws<InvalidOperationException>(() => commandParameters.PossibleParameters);
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
