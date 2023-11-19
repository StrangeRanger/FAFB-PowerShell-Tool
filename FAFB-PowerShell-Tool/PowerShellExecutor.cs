using System.Management.Automation;

namespace FAFB_PowerShell_Tool;

internal class PowerShellExecutor
{
    public static void ExecutePowerShellCommand(string commandText)
    {
        //Create a powershell env and then add the command given to this method
        using PowerShell ps = PowerShell.Create();
        ps.AddScript(commandText);

        // run the script
        var results = ps.Invoke();

        // error checking if the powershell command fails 
        if (ps.HadErrors)
        {
            foreach (var error in ps.Streams.Error)
            {
                Console.WriteLine("Error: " + error.ToString());
            }
        }
        else
        {
            foreach (var result in results)
            {
                Console.WriteLine(result.ToString());
            }
        }
    }
}

