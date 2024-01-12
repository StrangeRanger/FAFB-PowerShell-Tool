using FAFB_PowerShell_Tool.PowerShell.Commands;

namespace FAFB_PowerShell_Tool.Tests;

public class InternalCommandTest
{
    [Fact]
    public void CommandNameIsCorrect()
    {
        InternalCommand command = new("Get-ADUser");
        Assert.Equal("Get-ADUser",  command.CommandName);
    }
    
    [Fact]
    public void CommandStringGetIsCorrectWhenParametersAreSet()
    {
        InternalCommand command = new("Get-ADUser", new[] {"-Identity", "Test"});
        Assert.Equal("Get-ADUser -Identity Test", command.CommandString);
    }
}