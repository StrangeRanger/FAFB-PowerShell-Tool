using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace FAFB_PowerShell_Tool.PowerShell;

/// <summary>
/// Manages execution of PowerShell commands, providing both synchronous and asynchronous execution methods.
/// </summary>
public class PowerShellExecutor
{
    private readonly System.Management.Automation.PowerShell _powerShell;

    /// <summary>
    /// Initializes a new instance of the PowerShellExecutor class and imports the ActiveDirectory module.
    /// </summary>
    public PowerShellExecutor()
    {
        _powerShell = System.Management.Automation.PowerShell.Create();

        _powerShell.AddScript("Import-Module ActiveDirectory;");
        _powerShell.Invoke();
        _powerShell.Commands.Clear();
    }

    /// <summary>
    /// Prepares a PowerShell command for execution.
    /// </summary>
    /// <param name="command">The command to be prepared for execution.</param>
    private void PrepareCommand(Command? command)
    {
        _powerShell.Commands.AddCommand(command.CommandText);
        foreach (var parameter in command.Parameters)
        {
            _powerShell.Commands.AddParameter(parameter.Name, parameter.Value);
        }
    }

    /// <summary>
    /// Executes a PowerShell command synchronously and returns the results.
    /// </summary>
    /// <param name="command">The command to be executed.</param>
    /// <returns>A ReturnValues object containing the results of the command execution.</returns>
    public ReturnValues Execute(Command? command)
    {
        try
        {
            PrepareCommand(command);

            Collection<PSObject> results = _powerShell.Invoke();

            return ProcessPowerShellResults(results);
        }
        catch (Exception ex)
        {
            return ExecutionExceptionHandler(ex);
        }
    }

    /// <summary>
    /// Executes a PowerShell command asynchronously and returns the results.
    /// </summary>
    /// <param name="command">The command to be executed.</param>
    /// <returns>A task containing a ReturnValues object with the results of the command execution.</returns>
    public async Task<ReturnValues> ExecuteAsync(Command? command)
    {
        try
        {
            PrepareCommand(command);

            PSDataCollection<PSObject> results = await _powerShell.InvokeAsync();

            return await ProcessPowerShellResultsAsync(results);
        }
        catch (Exception ex)
        {
            return ExecutionExceptionHandler(ex);
        }
    }

    /// <summary>
    /// Handles exceptions that occur during the execution of a PowerShell command.
    /// </summary>
    /// <param name="ex">The exception that occurred during the execution of the PowerShell command.</param>
    /// <returns>A ReturnValues object containing the exception details.</returns>
    private ReturnValues ExecutionExceptionHandler(Exception ex)
    {
        StringBuilder sb = new();

        sb.AppendLine("An error occurred while executing the PowerShell command:");
        sb.AppendLine(ex.Message);
        sb.AppendLine(ex.StackTrace);
        Debug.WriteLine(sb.ToString());

        return new ReturnValues { StdErr = { sb.ToString() } };
    }

    /// <summary>
    /// Processes and formats the results of a PowerShell command execution synchronously.
    /// </summary>
    /// <param name="results">The results of the PowerShell command execution.</param>
    /// <returns>A ReturnValues object containing the processed results.</returns>
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
    /// Processes and formats the results of a PowerShell command execution asynchronously.
    /// </summary>
    /// <param name="results">The results of the PowerShell command execution.</param>
    /// <returns>A task containing a ReturnValues object with the processed results.</returns>
    private Task<ReturnValues> ProcessPowerShellResultsAsync(IEnumerable<PSObject> results)
    {
        return Task.FromResult(ProcessPowerShellResults(results));
    }
}
