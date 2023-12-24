using System.IO;
using System.Management.Automation;
using System.Windows;

namespace FAFB_PowerShell_Tool;

public class PowerShellExecutor
{
    private static PowerShellExecutor? _instance;
    private readonly PowerShell _powerShell;

    private PowerShellExecutor()
    {
        _powerShell = PowerShell.Create();
        _powerShell.AddScript("Import-Module ActiveDirectory;");
        _powerShell.Invoke();
        _powerShell.Commands.Clear();
    }

    // Public property to get the instance of the class. (Singleton)
    public static PowerShellExecutor Instance
    {
        get { return _instance ??= new PowerShellExecutor(); }
    }

    public List<string> Execute(string commandText)
    {
        List<string> returnValues = new List<string>();
        const string filePath = "../../../../FAFB-PowerShell-Tool-Output.txt"; // For testing purposes only.

        //returnValues.Clear();
        ThrowExceptionIfCommandTextIsNullOrWhiteSpace(commandText);
        _powerShell.AddScript(commandText);

        var results = _powerShell.Invoke();

        if (_powerShell.HadErrors)
        {
            foreach (var error in _powerShell.Streams.Error)
            {
                File.WriteAllText(filePath, "Error: " + error); // For testing purposes only.
                returnValues.Add("Error: " + error);
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
        const string exceptionMessageOne = "Command text cannot be null.";
        const string exceptionMessageTwo = "Command text cannot be null or whitespace.";

        if (commandText is null)
        {
            ShowError(exceptionMessageOne);
            throw new ArgumentNullException(exceptionMessageOne);
        }

        if (string.IsNullOrWhiteSpace(commandText))
        {
            ShowError(exceptionMessageTwo);
            throw new ArgumentException(exceptionMessageTwo);
        }
    }

    private static void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

