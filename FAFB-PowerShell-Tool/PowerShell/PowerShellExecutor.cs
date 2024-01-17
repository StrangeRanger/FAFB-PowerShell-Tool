using System.IO;
using System.Management.Automation;
using FAFB_PowerShell_Tool.PowerShell.Commands;

namespace FAFB_PowerShell_Tool.PowerShell;

/// <summary>
/// This class is used for executing PowerShell commands.
/// </summary>
public class PowerShellExecutor
{
    private readonly System.Management.Automation.PowerShell _powerShell;

    public PowerShellExecutor()
    {
        _powerShell = System.Management.Automation.PowerShell.Create();
        // ActiveDirectory module must be imported before executing any commands from the module.
        _powerShell.AddScript("Import-Module ActiveDirectory;");
        _powerShell.Invoke();
        _powerShell.Commands.Clear();
    }

    /// <summary>
    /// Executes a PowerShell command synchronously.
    /// </summary>
    /// <typeparam name="T">Of type 'ICommand'.</typeparam>
    /// <param name="commandString">The command and parameters in a single string.</param>
    /// <returns>The processed and formatted results of the executed PowerShell command.</returns>
    public ReturnValues Execute<T>(T commandString) where T : ICommand
    {
        ValidateCommandString(commandString);
        _powerShell.AddScript(commandString.CommandString);
        var results = _powerShell.Invoke();
        return ProcessPowerShellResults(results);
    }

    /// <summary>
    /// Executes a PowerShell command asynchronously.
    /// </summary>
    /// <typeparam name="T">Of type 'ICommand'.</typeparam>
    /// <param name="commandString">The command and parameters in a single string.</param>
    /// <returns>The processed and formatted results of the executed PowerShell command.</returns>
    public async Task<ReturnValues> ExecuteAsync<T>(T commandString) where T : ICommand
    {
        ValidateCommandString(commandString);
        _powerShell.AddScript(commandString.CommandString);
        var results = await _powerShell.InvokeAsync();
        return await ProcessPowerShellResultsAsync(results);
    }

    /// <summary>
    /// Checks if the command string is null, contains whitespace, or is blank.
    /// </summary>
    /// <typeparam name="T">Of type 'ICommand'.</typeparam>
    /// <param name="commandString">The command and parameters in a single string.</param>
    /// <exception cref="ArgumentException">Thrown when 'commandName' is null, contains whitespace, or is blank</exception>
    private void ValidateCommandString<T>(T commandString) where T : ICommand
    {
        if (string.IsNullOrWhiteSpace(commandString.CommandString))
        {
            MessageBoxOutput.Show("Command text cannot be null or whitespace.", MessageBoxOutput.OutputType.Error);
            throw new ArgumentException("Command text cannot be null or whitespace.", nameof(commandString));
        }
    }

    /// <summary>
    /// Processes the results from the PowerShell command synchronously.
    /// </summary>
    /// <param name="results">The results of the PowerShell command.</param>
    /// <returns>The processed and formatted results from the executed PowerShell command</returns>
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

    /// <summary>
    /// Processes the results from the PowerShell command asynchronously.
    /// </summary>
    /// <param name="results">The results of the PowerShell command.</param>
    /// <returns>The processed and formatted results from the executed PowerShell command</returns>
    private Task<ReturnValues> ProcessPowerShellResultsAsync(IEnumerable<PSObject> results)
    {
        return Task.FromResult(ProcessPowerShellResults(results));
    }
}

