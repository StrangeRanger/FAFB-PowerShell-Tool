using System.IO;
using System.Management.Automation;
using FAFB_PowerShell_Tool.PowerShell.Commands;

namespace FAFB_PowerShell_Tool.PowerShell;

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

    public ReturnValues Execute<T>(T commandString) where T : ICommand
    {
        ValidateCommandString(commandString);
        _powerShell.AddScript(commandString.CommandString);
        var results = _powerShell.Invoke();
        return ProcessPowerShellResults(results);
    }

    public async Task<ReturnValues> ExecuteAsync<T>(T commandString) where T : ICommand
    {
        ValidateCommandString(commandString);
        _powerShell.AddScript(commandString.CommandString);
        var results = await _powerShell.InvokeAsync();
        return await ProcessPowerShellResultsAsync(results);
    }

    private void ValidateCommandString<T>(T commandString) where T : ICommand
    {
        if (string.IsNullOrWhiteSpace(commandString.CommandString))
        {
            MessageBoxOutput.Show("Command text cannot be null or whitespace.", MessageBoxOutput.OutputType.Error);
            throw new ArgumentException("Command text cannot be null or whitespace.", nameof(commandString));
        }
    }

    private ReturnValues ProcessPowerShellResults(IEnumerable<PSObject> results)
    {
        ReturnValues returnValues = new();

        if (_powerShell.HadErrors)
        {
            foreach (var error in _powerShell.Streams.Error)
            {
                returnValues.StdErr.Add($"Error: {error}");
            }
        }
        else
        {
            foreach (var result in results)
            {
                returnValues.StdOut.Add(result.ToString());
            }
        }

        return returnValues;
    }

    private Task<ReturnValues> ProcessPowerShellResultsAsync(IEnumerable<PSObject> results)
    {
        return Task.FromResult(ProcessPowerShellResults(results));
    }
}

