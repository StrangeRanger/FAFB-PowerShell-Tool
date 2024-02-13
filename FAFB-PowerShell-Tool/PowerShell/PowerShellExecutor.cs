using System.Collections.ObjectModel;
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
    /// Executes a PowerShell command synchronously and returns the results.
    /// </summary>
    /// <param name="commandString">The PowerShell command to be executed, encapsulated in a Command object.</param>
    /// <returns>A ReturnValues object containing the execution results.</returns>
    public ReturnValues Execute(Command commandString)
    {
        return ExecuteInternal(CommandToString(commandString));
    }

    /// <summary>
    /// Executes a PowerShell command synchronously and returns the results.
    /// </summary>
    /// <param name="commandString">The PowerShell command string to be executed.</param>
    /// <returns>A ReturnValues object containing the execution results.</returns>
    public ReturnValues Execute(string commandString)
    {
        return ExecuteInternal(commandString);
    }

    /// <summary>
    /// Executes a PowerShell command asynchronously and returns the results.
    /// </summary>
    /// <param name="commandString">The PowerShell command to be executed, encapsulated in a Command object.</param>
    /// <returns>A Task representing the asynchronous operation, containing the execution results.</returns>
    public async Task<ReturnValues> ExecuteAsync(Command commandString)
    {
        return await ExecuteInternalAsync(CommandToString(commandString));
    }

    /// <summary>
    /// Executes a PowerShell command asynchronously and returns the results.
    /// </summary>
    /// <param name="commandString">The PowerShell command string to be executed.</param>
    /// <returns>A Task representing the asynchronous operation, containing the execution results.</returns>
    public async Task<ReturnValues> ExecuteAsync(string commandString)
    {
        return await ExecuteInternalAsync(commandString);
    }

    /// <summary>
    /// Executes the provided PowerShell command string synchronously.
    /// </summary>
    /// <param name="command">The PowerShell command in string format.</param>
    /// <returns>The execution results encapsulated in a ReturnValues object.</returns>
    private ReturnValues ExecuteInternal(string command)
    {
        ValidateCommandString(command);
        _powerShell.AddScript(command);
        Collection<PSObject> results = _powerShell.Invoke();
        return ProcessPowerShellResults(results);
    }

    /// <summary>
    /// Executes the provided PowerShell command string asynchronously.
    /// </summary>
    /// <param name="command">The PowerShell command in string format.</param>
    /// <returns>A task representing the asynchronous operation, containing the execution results.</returns>
    private async Task<ReturnValues> ExecuteInternalAsync(string command)
    {
        ValidateCommandString(command);
        _powerShell.AddScript(command);
        PSDataCollection<PSObject> results = await _powerShell.InvokeAsync();
        return await ProcessPowerShellResultsAsync(results);
    }

    /// <summary>
    /// Validates the provided command string.
    /// </summary>
    /// <param name="commandString">The command string to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the command string is null, empty, or consists only of
    /// white-space.</exception>
    private void ValidateCommandString(string commandString)
    {
        if (string.IsNullOrWhiteSpace(commandString))
        {
            throw new ArgumentException("Command string cannot be null, contain whitespace, or be blank.");
        }
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

    /// <summary>
    /// Converts a PowerShell Command object to its string representation.
    /// </summary>
    /// <param name="command">The PowerShell Command object to convert.</param>
    /// <returns>A string representation of the PowerShell Command.</returns>
    public string CommandToString(Command command)
    {
        StringBuilder commandString = new StringBuilder();

        commandString.Append(command.CommandText);

        foreach (var param in command.Parameters)
        {
            commandString.Append($" -{param.Name} {param.Value}");
        }

        return commandString.ToString();
    }
}
