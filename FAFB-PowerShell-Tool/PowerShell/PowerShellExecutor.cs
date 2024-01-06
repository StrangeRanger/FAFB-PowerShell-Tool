using System.IO;

namespace FAFB_PowerShell_Tool.PowerShell;

// TODO: Refactor Execute methods to reduce code duplication.
// TODO: Modify to ensure it works new method of execution.
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

    public ExecuteReturnValues Execute(string commandText)
    {
        ExecuteReturnValues returnValues = new();
        const string filePath = "FAFB-PowerShell-Tool-Output.txt"; // For testing purposes only.

        if (string.IsNullOrWhiteSpace(commandText))
        {
            throw new ArgumentException("Command text cannot be null or whitespace.", nameof(commandText));
        }

        _powerShell.AddScript(commandText);

        var results = _powerShell.Invoke();

        // TODO: Possibly include STDERR, STDOUT, etc., streams in the return values...
        if (_powerShell.HadErrors)
        {
            foreach (var error in _powerShell.Streams.Error)
            {
                File.WriteAllText(filePath, "Error: " + error); // For testing purposes only.
                returnValues.StdOut.Add("Error: " + error);
            }
        }
        else
        {
            foreach (var result in results)
            {
                File.WriteAllText(filePath, result.ToString()); // For testing purposes only.
                returnValues.StdOut.Add(result.ToString());
            }
        }

        return returnValues;
    }

    public async Task<ExecuteReturnValues> ExecuteAsync(string commandText)
    {
        ExecuteReturnValues executeReturnValues = new();
        const string filePath = "FAFB-PowerShell-Tool-Output.txt"; // For testing purposes only.

        if (string.IsNullOrWhiteSpace(commandText))
        {
            throw new ArgumentException("Command text cannot be null or whitespace.", nameof(commandText));
        }

        _powerShell.AddScript(commandText);

        var results = await _powerShell.InvokeAsync().ConfigureAwait(false);

        if (_powerShell.HadErrors)
        {
            executeReturnValues.HadErrors = true;
            foreach (var error in _powerShell.Streams.Error)
            {
                await File.WriteAllTextAsync(filePath, "Error: " + error); // For testing purposes only.
                executeReturnValues.StdOut.Add("Error: " + error);
            }
        }
        else
        {
            executeReturnValues.HadErrors = false;
            foreach (var result in results)
            {
                await File.WriteAllTextAsync(filePath, result.ToString()); // For testing purposes only.
                executeReturnValues.StdOut.Add(result.ToString());
            }
        }

        return executeReturnValues;
    }
}

