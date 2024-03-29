using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;
using ActiveDirectoryQuerier.ActiveDirectory;
// ReSharper disable InconsistentNaming

namespace ActiveDirectoryQuerier.Tests;

public class ADCommandsFetcherTests
{
    [Fact]
    public async Task GetADCommands_ReturnsCommandList_IsNotEmpty()
    {
        // Arrange
        ObservableCollection<Command> adCommands = await ADCommandsFetcher.GetADCommandsAsync();

        // Assert
        Assert.NotEmpty(adCommands);
    }

    [Theory]
    [InlineData("Get-ADUser")]
    [InlineData("Get-ADGroup")]
    [InlineData("Get-ADComputer")]
    public async Task GetADCommands_ReturnsCommandList_ContainsCommand(string commandName)
    {
        // Arrange
        ObservableCollection<Command> adCommands = await ADCommandsFetcher.GetADCommandsAsync();

        // Assert
        Assert.Contains(adCommands, command => command.CommandText == commandName);
    }
}
