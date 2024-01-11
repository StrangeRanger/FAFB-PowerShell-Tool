using FAFB_PowerShell_Tool.PowerShell;

namespace FAFB_PowerShell_Tool.Tests;

[TestClass]
public class GuiCommandTest
{
    [TestMethod]
    public void CommandNameIsCorrect()
    {
        GuiCommand command = new("Get-ADUser");
        Assert.AreEqual("Get-ADUser",  command.CommandName);
    }

    [TestMethod]
    public void CommandNameThrowsArgumentExceptionWhenNull()
    {
        Assert.ThrowsException<ArgumentException>(() => new GuiCommand(null!));
    }

    [TestMethod]
    public void CommandNameThrowsArgumentExceptionWhenWhitespace()
    {
        Assert.ThrowsException<ArgumentException>(() => new GuiCommand(""));
        Assert.ThrowsException<ArgumentException>(() => new GuiCommand(" "));
        Assert.ThrowsException<ArgumentException>(() => new GuiCommand(string.Empty));
    }

    [TestMethod]
    public void PossibleParametersThrowsInvalidOperationExceptionWhenEmpty()
    {
        GuiCommand command = new("Get-ADUser");
        Assert.ThrowsException<InvalidOperationException>(() => command.PossibleParameters);
    }
    
    [TestMethod]
    public async void PossibleParametersReturnsCorrectly()
    {
        GuiCommand command = new("Get-ADUser");
        await command.LoadCommandParametersAsync();
        Assert.IsTrue(command.PossibleParameters.Contains("-Identity"));
        Assert.IsTrue(command.PossibleParameters.Count > 0);
    }
    
    [TestMethod]
    public async void PossibleParametersReturnsCorrectlyWhenCalledTwice()
    {
        GuiCommand command = new("Get-ADUser");
        await command.LoadCommandParametersAsync();
        await command.LoadCommandParametersAsync();
        Assert.IsTrue(command.PossibleParameters.Contains("-Identity"));
        Assert.IsTrue(command.PossibleParameters.Count > 0);
        Assert.AreEqual(command.PossibleParameters.Count, command.PossibleParameters.Distinct().Count());
    }
    
    [TestMethod]
    public void TestParameters()
    {
        GuiCommand command = new("Get-ADUser")
        {
            Parameters = new[] {"-Identity", "FAFB-Admin"}
        };
        Assert.AreEqual(command.Parameters[0], "-Identity");
        Assert.AreEqual(command.Parameters[1], "FAFB-Admin");
    }

    [TestMethod]
    public void ThrowExceptionWhenLoadCommandParametersAsyncIsNotCalled()
    {
        GuiCommand command = new("Get-ADUser");
        Assert.ThrowsException<InvalidOperationException>(() => command.PossibleParameters);
    }
}
