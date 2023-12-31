using FAFB_PowerShell_Tool.PowerShell;

namespace FAFB_PowerShell_Tool.Tests;

[TestClass]
public class PowerShellExecutorTest
{
    private readonly PowerShellExecutor _powerShellExecutor = new();
    
    [TestMethod]
    public void ThrowArgumentNullExceptionWhenCommandTextIsNull()
    {
        Assert.ThrowsException<ArgumentNullException>(() => _powerShellExecutor.Execute(null!));
    }

    [TestMethod]
    public void ThrowArgumentExceptionWhenCommandTextIsWhitespace()
    {
        Assert.ThrowsException<ArgumentException>(() => _powerShellExecutor.Execute(""));
        Assert.ThrowsException<ArgumentException>(() => _powerShellExecutor.Execute(" "));
    }
}