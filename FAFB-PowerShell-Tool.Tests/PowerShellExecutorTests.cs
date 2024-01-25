using System.Management.Automation.Runspaces;
using FAFB_PowerShell_Tool.PowerShell;
using ArgumentException = System.ArgumentException;

namespace FAFB_PowerShell_Tool.Tests;

public class PowerShellExecutorTest
{
    [Fact]
    public void ReturnValues_WithGoodCommand_NoErrorsAndStdOut()
    {
        PowerShellExecutor powerShell = new();
        ReturnValues values = powerShell.Execute(new Command("Get-Process"));
        Assert.False(values.HadErrors);
        Assert.NotEmpty(values.StdOut);
        Assert.Empty(values.StdErr);
    }

    [Fact]
    public void ReturnValues_WithBadCommand_HadErrorsAndStdErr()
    {
        PowerShellExecutor powerShell = new();
        ReturnValues values = powerShell.Execute(new Command("BadCommand"));
        Assert.True(values.HadErrors);
        Assert.Empty(values.StdOut);
        Assert.NotEmpty(values.StdErr);
    }

    /*[Fact]
    public void Execute_BadCommand_ThrowsArgumentException()
    {
        PowerShellExecutor powerShell = new();
        Assert.Throws<ArgumentException>(() => powerShell.Execute(new Command("")));
        Assert.Throws<ArgumentException>(() => powerShell.Execute(new Command(" ")));
        Assert.Throws<ArgumentException>(() => powerShell.Execute(new Command(null!)));
        Assert.Throws<ArgumentException>(() => powerShell.Execute(new Command(string.Empty)));
    }*/
}
