using System.IO;

namespace FAFB_PowerShell_Tool.PowerShell;

public class PowerShellExecutor
{
    private readonly System.Management.Automation.PowerShell _powerShell;

    public PowerShellExecutor()
    {
        _powerShell = System.Management.Automation.PowerShell.Create();
        _powerShell.AddScript("Import-Module ActiveDirectory;");
        _powerShell.Invoke();
        _powerShell.Commands.Clear();
    }

    public List<string> Execute(string commandText)
    {
        List<string> returnValues = new List<string>();
        const string filePath = "FAFB-PowerShell-Tool-Output.txt"; // For testing purposes only.

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
    
    public async Task<List<string>> ExecuteAsync(string commandText)
    {
        List<string> returnValues = new List<string>();
        //const string filePath = "FAFB-PowerShell-Tool-Output.txt"; // For testing purposes only.

        ThrowExceptionIfCommandTextIsNullOrWhiteSpace(commandText);
        _powerShell.AddScript(commandText);

        var results = await _powerShell.InvokeAsync().ConfigureAwait(false);

        if (_powerShell.HadErrors)
        {
            foreach (var error in _powerShell.Streams.Error)
            {
                //await File.WriteAllTextAsync(filePath, "Error: " + error); // For testing purposes only.
                returnValues.Add("Error: " + error);
            }
        }
        else
        {
            foreach (var result in results)
            {
                //await File.WriteAllTextAsync(filePath, result.ToString()); // For testing purposes only.
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
            MessageBoxOutput.ShowMessageBox(exceptionMessageOne, MessageBoxOutput.OutputType.InternalError);
            throw new ArgumentNullException(exceptionMessageOne);
        }

        if (string.IsNullOrWhiteSpace(commandText))
        {
            MessageBoxOutput.ShowMessageBox(exceptionMessageTwo, MessageBoxOutput.OutputType.InternalError);
            throw new ArgumentException(exceptionMessageTwo);
        }
    }
}

