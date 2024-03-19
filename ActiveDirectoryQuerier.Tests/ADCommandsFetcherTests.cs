using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;
using ActiveDirectoryQuerier.ActiveDirectory;

namespace ActiveDirectoryQuerier.Tests;

// ReSharper disable once InconsistentNaming
public class ADCommandsFetcherTests
{
    [Fact]
    public async Task GetADCommands_ReturnsCommandList_IsNotEmpty()
    {
        // Act
        ObservableCollection<Command> adCommands = await ADCommandsFetcher.GetADCommands();

        // Assert
        Assert.NotEmpty(adCommands);
    }

    [Theory]
    [InlineData("Get-ADUser")]
    [InlineData("Get-ADGroup")]
    [InlineData("Get-ADComputer")]
    public async Task GetADCommands_ReturnsCommandList_ContainsCommand(string commandName)
    {
        // Act
        ObservableCollection<Command> adCommands = await ADCommandsFetcher.GetADCommands();

        // Assert
        Assert.Contains(adCommands, command => command.CommandText == commandName);
    }
}
