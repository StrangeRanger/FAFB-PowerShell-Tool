using System.IO;
using System.Management.Automation;
using System.Windows;

namespace FAFB_PowerShell_Tool;

public class PowerShellExecutor
{
    private static PowerShellExecutor _instance;
    private PowerShell _powerShell;

    // Private constructor.
    private PowerShellExecutor()
    {
        _powerShell = PowerShell.Create();
        //_powerShell.AddScript("Import-Module ActiveDirectory");
        //_powerShell.Invoke();
        //_powerShell.Commands.Clear();
    }

    // Public static method to get the instance.
    public static PowerShellExecutor Instance
    {
        get {
            if (_instance is null)
            {
                _instance = new PowerShellExecutor();
            }
            return _instance;
        }
    }

    public List<string> Execute(string commandText)
    {
        List<string> returnValues = new List<string>();
        string filePath = "../../../../FAFB-PowerShell-Tool-Output.txt"; // For testing purposes only.

        // TODO: Use a MessageBox to show errors to the user.
        ThrowExceptionIfCommandTextIsNullOrWhiteSpace(commandText);
        _powerShell.AddScript(commandText);

        var results = _powerShell.Invoke();

        if (_powerShell.HadErrors)
        {
            foreach (var error in _powerShell.Streams.Error)
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

    private void ThrowExceptionIfCommandTextIsNullOrWhiteSpace(string commandText)
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

    private void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

