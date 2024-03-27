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

    public PSOutput Execute(Command psCommand, OutputFormat outputFormat = OutputFormat.Text)
    {
        try
        {
            _powerShell.Commands.Clear();
            AssembleFullCommand(psCommand, outputFormat);

            Collection<PSObject> results = _powerShell.Invoke();

            return ProcessExecutionResults(results);
        }
        catch (Exception exception)
        {
            return HandleExecutionException(exception);
        }
    }

    public async Task<PSOutput> ExecuteAsync(Command command, OutputFormat outputFormat = OutputFormat.Text)
    {
        _powerShell.Commands.Clear();
        AssembleFullCommand(command, outputFormat);

        PSDataCollection<PSObject> results = await _powerShell.InvokeAsync();

        return ProcessExecutionResults(results);
    }

    private void AssembleFullCommand(Command psCommand, OutputFormat outputFormat)
    {
        ArgumentNullException.ThrowIfNull(psCommand);

        _powerShell.Commands.AddCommand(psCommand.CommandText);

        foreach (var parameter in psCommand.Parameters)
        {
            _powerShell.Commands.AddParameter(parameter.Name, parameter.Value);
        }

        if (outputFormat == OutputFormat.Csv)
        {
            _powerShell.Commands.AddCommand("ConvertTo-Csv").AddParameter("NoTypeInformation");
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

    private PSOutput HandleExecutionException(Exception exception)
    {
        StringBuilder errorMessage = new();

        errorMessage.AppendLine("An error occurred while executing the PowerShell command:");
        errorMessage.AppendLine(exception.Message);
        errorMessage.AppendLine(exception.StackTrace);
        Trace.WriteLine(errorMessage.ToString());

        return new PSOutput { StdErr = { errorMessage.ToString() } };
    }
}
