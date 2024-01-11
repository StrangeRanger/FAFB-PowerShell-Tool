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

    public ExecuteReturnValues Execute<T>(T commandString) where T : ICommand
    {
        ValidateCommandString(commandString);
        _powerShell.AddScript(commandString.CommandString);
        var results = _powerShell.Invoke();
        return ProcessPowerShellResults(results, false);
    }

    public async Task<ExecuteReturnValues> ExecuteAsync<T>(T commandString) where T : ICommand
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
            MessageBoxOutput.Show("Command text cannot be null or whitespace.", MessageBoxOutput.OutputType.InternalError);
            throw new ArgumentException("Command text cannot be null or whitespace.", nameof(commandString));
        }
    }

    private ExecuteReturnValues ProcessPowerShellResults(IEnumerable<PSObject> results, bool isAsync)
    {
        ExecuteReturnValues returnValues = new();
        const string filePath = "FAFB-PowerShell-Tool-Output.txt"; // For testing purposes only.

        if (_powerShell.HadErrors)
        {
            foreach (var error in _powerShell.Streams.Error)
            {
                if (isAsync)
                {
                    File.WriteAllTextAsync(filePath, $"Error: {error}").Wait(); // For testing purposes only.
                }
                else
                {
                    File.WriteAllText(filePath, $"Error: {error}"); // For testing purposes only.
                }
                returnValues.StdErr.Add($"Error: {error}");
            }
        }
        else
        {
            foreach (var result in results)
            {
                if (isAsync)
                {
                    File.WriteAllTextAsync(filePath, result.ToString()).Wait(); // For testing purposes only.
                }
                else
                {
                    File.WriteAllText(filePath, result.ToString()); // For testing purposes only.
                }
                returnValues.StdOut.Add(result.ToString());
            }
        }

        return returnValues;
    }

    private Task<ExecuteReturnValues> ProcessPowerShellResultsAsync(IEnumerable<PSObject> results)
    {
        return Task.FromResult(ProcessPowerShellResults(results, true));
    }

}

