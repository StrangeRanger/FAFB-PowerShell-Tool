using System.IO;
using System.Management.Automation;
using System.Windows;

namespace FAFB_PowerShell_Tool;

// TODO: Should this be made into a singleton rather than a static class?
public class PowerShellExecutor
{
    public static List<string> Execute(string commandText)
    {
        using PowerShell ps = PowerShell.Create();
        List<string> returnValues = new List<string>();
        string filePath = @"../../../../FAFB-PowerShell-Tool-Output.txt"; // For testing purposes only.

        // TODO: Use a MessageBox to show errors to the user.
        ThrowExceptionIfCommandTextIsNullOrWhiteSpace(commandText);
        ps.AddScript(commandText);

        var results = ps.Invoke();

        if (ps.HadErrors)
        {
            foreach (var error in ps.Streams.Error)
            {
                File.WriteAllText(filePath, "Error: " + error.ToString()); // For testing purposes only.
                returnValues.Add("Error: " + error.ToString());
            }
        }
        else
        {
            foreach (var result in results)
            {
                File.WriteAllText(filePath, result.ToString()); // For testing purposes only.
                returnValues.Add(result.ToString());
            }
        }

        return returnValues;
    }

    private static void ThrowExceptionIfCommandTextIsNullOrWhiteSpace(string commandText)
    {
        if (commandText is null)
        {
            throw new ArgumentNullException("Command text cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(commandText))
        {
            throw new ArgumentException("Command text cannot be null or whitespace.");
        }
    }

    private static void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
