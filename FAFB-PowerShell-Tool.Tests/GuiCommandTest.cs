using FAFB_PowerShell_Tool.PowerShell;

namespace FAFB_PowerShell_Tool.Tests;

[TestClass]
public class GuiCommandTest
{
    [TestMethod]
    public void CommandNameIsCorrect()
    {
        InternalCommand internalCommand = new("Get-ADUser");
        Assert.AreEqual("Get-ADUser",  internalCommand.CommandName);
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
        GuiCommand guiCommand = new("Get-ADUser");
        Assert.ThrowsException<InvalidOperationException>(() => guiCommand.PossibleParameters);
    }
    
    [TestMethod]
    public void PossibleParametersReturnsCorrectly()
    {
        GuiCommand guiCommand = new("Get-ADUser");
        guiCommand.LoadCommandParametersAsync().Wait();
        Assert.IsTrue(guiCommand.PossibleParameters.Contains("-Identity"));
        Assert.IsTrue(guiCommand.PossibleParameters.Count > 0);
    }
    
    [TestMethod]
    public void PossibleParametersReturnsCorrectlyWhenCalledTwice()
    {
        GuiCommand guiCommand = new("Get-ADUser");
        guiCommand.LoadCommandParametersAsync().Wait();
        guiCommand.LoadCommandParametersAsync().Wait();
        Assert.IsTrue(guiCommand.PossibleParameters.Contains("-Identity"));
        Assert.IsTrue(guiCommand.PossibleParameters.Count > 0);
        Assert.AreEqual(guiCommand.PossibleParameters.Count, guiCommand.PossibleParameters.Distinct().Count());
    }
    
    [TestMethod]
    public void TestParameters()
    {
        GuiCommand guiCommand = new("Get-ADUser")
        {
            Parameters = new[] {"-Identity", "FAFB-Admin"}
        };
        Assert.AreEqual(guiCommand.Parameters[0], "-Identity");
        Assert.AreEqual(guiCommand.Parameters[1], "FAFB-Admin");
    }
}
