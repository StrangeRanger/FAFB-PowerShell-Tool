using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace ActiveDirectoryQuerier.PowerShell;

// ReSharper disable once InconsistentNaming
public class PSExecutor
{
    private readonly System.Management.Automation.PowerShell _powerShell;

    public PSExecutor()
    {
        _powerShell = System.Management.Automation.PowerShell.Create();

        // Import the ActiveDirectory module to ensure that the ActiveDirectory commands are available.
        _powerShell.AddScript("Import-Module ActiveDirectory;");
        _powerShell.Invoke();
        _powerShell.Commands.Clear();
    }

    private void AssembleFullCommand(Command psCommand)
    {
        ArgumentNullException.ThrowIfNull(psCommand);

        _powerShell.Commands.AddCommand(psCommand.CommandText);

        foreach (var parameter in psCommand.Parameters)
        {
            _powerShell.Commands.AddParameter(parameter.Name, parameter.Value);
        }
    }

    public PSOutput Execute(Command psCommand)
    {
        try
        {
            _powerShell.Commands.Clear();
            AssembleFullCommand(psCommand);

            Collection<PSObject> results = _powerShell.Invoke();

            return ProcessExecutionResults(results);
        }
        catch (Exception exception)
        {
            return HandleExecutionException(exception);
        }
    }

    public async Task<PSOutput> ExecuteAsync(Command command)
    {
        try
        {
            _powerShell.Commands.Clear();
            AssembleFullCommand(command);

            PSDataCollection<PSObject> results = await _powerShell.InvokeAsync();

            return await ProcessExecutionResultsAsync(results);
        }
        catch (Exception exception)
        {
            return HandleExecutionException(exception);
        }
    }

    private PSOutput ProcessExecutionResults(IEnumerable<PSObject> results)
    {
        PSOutput psOutput = new();

        if (_powerShell.HadErrors)
        {
            foreach (ErrorRecord error in _powerShell.Streams.Error)
            {
                psOutput.StdErr.Add($"Error: {error}");
            }
        }
        else
        {
            foreach (PSObject result in results)
            {
                psOutput.StdOut.Add(result.ToString());
            }
        }

        return psOutput;
    }

    private Task<PSOutput> ProcessExecutionResultsAsync(IEnumerable<PSObject> results)
    {
        return Task.FromResult(ProcessExecutionResults(results));
    }

    private PSOutput HandleExecutionException(Exception exception)
    {
        StringBuilder errorMessage = new();

        errorMessage.AppendLine("An error occurred while executing the PowerShell command:");
        errorMessage.AppendLine(exception.Message);
        errorMessage.AppendLine(exception.StackTrace);
        Debug.WriteLine(errorMessage.ToString());

        return new PSOutput { StdErr = { errorMessage.ToString() } };
    }
}
