using FAFB_PowerShell_Tool.PowerShell;

namespace FAFB_PowerShell_Tool.Tests;

[TestClass]
public class PowerShellExecutorTest
{
    [TestMethod]
    public void CommandNameIsCorrect()
    {
        InternalCommand command = new("Get-ADUser");
        Assert.AreEqual("Get-ADUser",  command.CommandName);
    }

    [TestMethod]
    public void CommandNameThrowsArgumentExceptionWhenNull()
    {
        Assert.ThrowsException<ArgumentException>(() => new InternalCommand(null!));
    }

    [TestMethod]
    public void CommandNameThrowsArgumentExceptionWhenWhitespace()
    {
        Assert.ThrowsException<ArgumentException>(() => new InternalCommand(""));
        Assert.ThrowsException<ArgumentException>(() => new InternalCommand(" "));
        Assert.ThrowsException<ArgumentException>(() => new InternalCommand(string.Empty));
    }
}
