using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using FAFB_PowerShell_Tool.PowerShell;

namespace FAFB_PowerShell_Tool.Tests;

public class ActiveDirectoryCommandsTests
{
    [Fact]
    public async Task GetActiveDirectoryCommands_ReturnsCommandList_IsNotEmpty()
    {
        // Act
        ObservableCollection<Command> commandList = await ActiveDirectoryCommands.GetActiveDirectoryCommands();

        // Assert
        Assert.NotEmpty(commandList);
    }

    [Theory]
    [InlineData("Get-ADUser")]
    [InlineData("Get-ADGroup")]
    [InlineData("Get-ADComputer")]
    public async Task GetActiveDirectoryCommands_ReturnsCommandList_ContainsCommand(string commandName)
    {
        // Act
        ObservableCollection<Command> commandList = await ActiveDirectoryCommands.GetActiveDirectoryCommands();

        // Assert
        Assert.Contains(commandList, cmd => cmd.CommandText == commandName);
    }
}
