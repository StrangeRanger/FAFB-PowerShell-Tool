using FAFB_PowerShell_Tool.PowerShell;
using FAFB_PowerShell_Tool.PowerShell.Commands;
using ArgumentException = System.ArgumentException;

namespace FAFB_PowerShell_Tool.Tests;

public class PowerShellExecutorTest
{
    [Fact]
    public void ExecuteCommandReturnsAreCorrect()
    {
        PowerShellExecutor powerShell = new();
        ReturnValues values = powerShell.Execute(new InternalCommand("Get-Process"));
        Assert.False(values.HadErrors);
        Assert.NotEmpty(values.StdOut);
        Assert.Empty(values.StdErr);
    }
    
    [Fact]
    public void ExecuteBadCommandReturnsAreCorrect()
    {
        PowerShellExecutor powerShell = new();
        ReturnValues values = powerShell.Execute(new InternalCommand("BadCommand"));
        Assert.True(values.HadErrors);
        Assert.Empty(values.StdOut);
        Assert.NotEmpty(values.StdErr);
    }
    
    [Fact]
    public void ExecuteBadInternalCommandThrowsInvalidOperationException()
    {
        PowerShellExecutor powerShell = new();
        Assert.Throws<ArgumentException>(() => powerShell.Execute(new InternalCommand("")));
        Assert.Throws<ArgumentException>(() => powerShell.Execute(new InternalCommand(" ")));
        Assert.Throws<ArgumentException>(() => powerShell.Execute(new InternalCommand(null!)));
        Assert.Throws<ArgumentException>(() => powerShell.Execute(new InternalCommand(string.Empty)));
    }

}
