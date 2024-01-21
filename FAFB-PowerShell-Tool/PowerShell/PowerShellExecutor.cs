using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

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

    public ReturnValues Execute(Command commandString)
    {
        return ExecuteInternal(CommandToString(commandString));
    }

    public ReturnValues Execute(string commandString)
    {
        return ExecuteInternal(commandString);
    }

    public async Task<ReturnValues> ExecuteAsync(Command commandString)
    {
        return await ExecuteInternalAsync(CommandToString(commandString));
    }

    public async Task<ReturnValues> ExecuteAsync(string commandString)
    {
        return await ExecuteInternalAsync(commandString);
    }

    private ReturnValues ExecuteInternal(string command)
    {
        ValidateCommandString(command);
        _powerShell.AddScript(command);
        Collection<PSObject> results = _powerShell.Invoke();
        return ProcessPowerShellResults(results);
    }

    private async Task<ReturnValues> ExecuteInternalAsync(string command)
    {
        ValidateCommandString(command);
        _powerShell.AddScript(command);
        PSDataCollection<PSObject> results = await _powerShell.InvokeAsync();
        return await ProcessPowerShellResultsAsync(results);
    }

    /// <summary>
    /// Checks if the command string is null, contains whitespace, or is blank.
    /// </summary>
    /// <param name="commandString">The command and parameters in a single string.</param>
    /// <exception cref="ArgumentException">Thrown when 'commandName' is null, contains whitespace, or is
    /// blank</exception>
    private void ValidateCommandString(string commandString)
    {
        if (string.IsNullOrWhiteSpace(commandString))
        {
            throw new ArgumentException("Command string cannot be null, contain whitespace, or be blank.");
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

    public string CommandToString(Command command)
    {
        StringBuilder commandString = new StringBuilder();

        // Add the command name
        commandString.Append(command.CommandText);

        // Iterate over the parameters and add them to the string
        foreach (var param in command.Parameters)
        {
            commandString.Append($" -{param.Name} {param.Value}");
        }

        return commandString.ToString();
    }
}

