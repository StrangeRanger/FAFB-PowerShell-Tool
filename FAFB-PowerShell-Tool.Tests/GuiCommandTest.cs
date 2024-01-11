using FAFB_PowerShell_Tool.PowerShell.Commands;

namespace FAFB_PowerShell_Tool.Tests;

public class GuiCommandTest
{
    [Fact]
    public void CommandNameIsCorrect()
    {
        GuiCommand command = new("Get-ADUser");
        Assert.Equal("Get-ADUser",  command.CommandName);
    }

    [Fact]
    public void CommandNameThrowsArgumentExceptionWhenNull()
    {
        Assert.Throws<ArgumentException>(() => new GuiCommand(null!));
    }

    [Fact]
    public void CommandNameThrowsArgumentExceptionWhenWhitespace()
    {
        Assert.Throws<ArgumentException>(() => new GuiCommand(""));
        Assert.Throws<ArgumentException>(() => new GuiCommand(" "));
        Assert.Throws<ArgumentException>(() => new GuiCommand(string.Empty));
    }

    [Fact]
    public void PossibleParametersThrowsInvalidOperationExceptionWhenEmpty()
    {
        GuiCommand command = new("Get-ADUser");
        Assert.Throws<InvalidOperationException>(() => command.PossibleParameters);
    }
    
    [Fact]
    public async Task PossibleParametersReturnsCorrectly()
    {
        GuiCommand command = new("Get-ADUser");
        await command.LoadCommandParametersAsync();
        Assert.Contains("-Identity", command.PossibleParameters);
        Assert.True(command.PossibleParameters.Count > 0);
    }
    
    [Fact]
    public async Task PossibleParametersReturnsCorrectlyWhenCalledTwice()
    {
        GuiCommand command = new("Get-ADUser");
        await command.LoadCommandParametersAsync();
        await command.LoadCommandParametersAsync();
        Assert.Contains("-Identity", command.PossibleParameters);
        Assert.True(command.PossibleParameters.Count > 0);
        Assert.Equal(command.PossibleParameters.Count, command.PossibleParameters.Distinct().Count());
    }
    
    [Fact]
    public void TestParameters()
    {
        GuiCommand command = new("Get-ADUser")
        {
            Parameters = new[] {"-Identity", "FAFB-Admin"}
        };
        Assert.Equal("-Identity", command.Parameters[0]);
        Assert.Equal("FAFB-Admin", command.Parameters[1]);
    }

    [Fact]
    public void ThrowExceptionWhenLoadCommandParametersAsyncIsNotCalled()
    {
        GuiCommand command = new("Get-ADUser");
        Assert.Throws<InvalidOperationException>(() => command.PossibleParameters);
    }
}
