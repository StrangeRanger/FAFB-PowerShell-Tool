using System.Management.Automation;

namespace FAFB_PowerShell_Tool;

class PowerShellExecutor
{
    public static List<string> Execute(string commandText)
    {
        List<string> returnValues = new List<string>();
        // Create a powershell env and then add the command given to this method.
        using PowerShell ps = PowerShell.Create();
        ps.AddScript(commandText);

        // Run the script.
        var results = ps.Invoke();

        // Error checking if the powershell command fails.
        if (ps.HadErrors)
        {
            foreach (var error in ps.Streams.Error)
            {
                returnValues.Add("Error: " + error.ToString());
                //Console.WriteLine("Error: " + error.ToString());
            }
        }
        else
        {
            foreach (var result in results)
            {
                returnValues.Add(result.ToString());
                //Console.WriteLine(result.ToString());
            }
        }

        return returnValues;
    }
}

