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
}