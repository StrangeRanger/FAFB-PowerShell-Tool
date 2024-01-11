using FAFB_PowerShell_Tool.PowerShell.Commands;

namespace FAFB_PowerShell_Tool.Tests;

[Collection("Internal Command Tests")]
public class InternalCommandTest
{
    [Fact]
    public void CommandNameIsCorrect()
    {
        InternalCommand command = new("Get-ADUser");
        Assert.Equal("Get-ADUser",  command.CommandName);
    }

    [Fact]
    public void CommandNameThrowsArgumentExceptionWhenNull()
    {
        Assert.Throws<ArgumentException>(() => new InternalCommand(null!));
    }

    [Fact]
    public void CommandNameThrowsArgumentExceptionWhenWhitespace()
    {
        Assert.Throws<ArgumentException>(() => new InternalCommand(""));
        Assert.Throws<ArgumentException>(() => new InternalCommand(" "));
        Assert.Throws<ArgumentException>(() => new InternalCommand(string.Empty));
    }
    
    [Fact]
    public void CommandStringGetIsCorrect()
    {
        InternalCommand command = new("Get-ADUser");
        Assert.Equal("Get-ADUser", command.CommandString);
    }
    
    [Fact]
    public void CommandStringGetIsCorrectWhenParametersAreSet()
    {
        InternalCommand command = new("Get-ADUser", new[] {"-Identity", "Test"});
        Assert.Equal("Get-ADUser -Identity Test", command.CommandString);
    }
}