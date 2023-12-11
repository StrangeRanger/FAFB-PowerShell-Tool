namespace FAFB_PowerShell_Tool.Tests;

[TestClass]
public class PowerShellExecutorTest
{
    private readonly PowerShellExecutor _powerShellExecutor = PowerShellExecutor.Instance;
    
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