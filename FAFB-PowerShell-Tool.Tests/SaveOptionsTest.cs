using FAFB_PowerShell_Tool.PowerShell.Commands;
using FAFB_PowerShell_Tool;
using FAFB_PowerShell_Tool.PowerShell;

namespace FAFB_PowerShell_Tool.Tests;

public class SaveOptionsTest
{
    [Fact]
    public void SaveToCSVAppendsParameter()
    {
        var saveOptions = new PSSaveOptions();
        InternalCommand command = new("Get-ADUser", new[] { "-Identity", "Test" });

        InternalCommand returnedCommand = saveOptions.OutputToCSV(command);


        Assert.Equal("Get-ADUser -Identity Test | Export-CSV ..\\..\\..\\SavedOutput\\output.csv", returnedCommand.CommandString);
    }
    [Fact]
    public void ExecuteOutputToCSV()
    {
        PSSaveOptions pssave = new PSSaveOptions();
        PowerShellExecutor executor = new PowerShellExecutor();
        InternalCommand test_command = new("get-process");
        ReturnValues temprv = executor.Execute(pssave.OutputToCSV(test_command));

        //Check if a csv was made 
        Assert.True(File.Exists("..\\..\\..\\SavedOutput\\output.csv"));
        //File.Delete("..\\..\\..\\FAFB-PowerShell-Tool\\SavedOutput\\output.csv");
    }




}