using FAFB_PowerShell_Tool.PowerShell;

namespace FAFB_PowerShell_Tool.Tests;

[TestClass]
public class CommandTest
{
    [TestMethod]
    public void CommandNameIsCorrect()
    {
        Command command = new("Get-ADUser");
        Assert.AreEqual("Get-ADUser", command.CommandName);
    }
    
    [TestMethod]
    public void CommandNameIsTrimmed()
    {
        Command command = new(" Get-ADUser ");
        Assert.AreEqual("Get-ADUser", command.CommandName);
    }
    
    [TestMethod]
    public void CommandNameThrowsArgumentExceptionWhenNull()
    {
        Assert.ThrowsException<ArgumentException>(() => new Command(null!));
    }
    
    [TestMethod]
    public void CommandNameThrowsArgumentExceptionWhenWhitespace()
    {
        Assert.ThrowsException<ArgumentException>(() => new Command(""));
        Assert.ThrowsException<ArgumentException>(() => new Command(" "));
        Assert.ThrowsException<ArgumentException>(() => new Command(string.Empty));
    }
    
    [TestMethod]
    public void PossibleParametersThrowsInvalidOperationExceptionWhenEmpty()
    {
        Command command = new("Get-ADUser");
        Assert.ThrowsException<InvalidOperationException>(() => command.PossibleParameters);
    }
}