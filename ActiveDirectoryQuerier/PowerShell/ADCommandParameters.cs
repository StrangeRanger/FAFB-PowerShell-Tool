using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace ActiveDirectoryQuerier.PowerShell;

// ReSharper disable once InconsistentNaming
public class ADCommandParameters
{
    private readonly ObservableCollection<string> _availableParameters = new();
    
    public ObservableCollection<string> AvailableParameters
    {
        get {
            if (_availableParameters.Count == 0)
            {
                Debug.WriteLine("Warning: LoadParametersAsync should be called before accessing " +
                                "AvailableParameters, to ensure asynchronous loading of parameters.");
                LoadParameters(null);
            }

            return _availableParameters;
        }
    }
    
    public async Task LoadParametersAsync(Command? powerShellCommand)
    {
        await LoadParametersCore(powerShellCommand, true);
    }

    public void LoadParameters(Command? powerShellCommand)
    {
        LoadParametersCore(powerShellCommand, false).Wait();
    }
    
    private async Task LoadParametersCore(Command? powerShellCommand, bool isAsync)
    {
        // powerShellCommand can be null if the user attempts to select an ActiveDirectory command that doesn't exist.
        // More specifically, if the entered command doesn't exist in the ActiveDirectoryCommandsList in
        // MainWindowViewModel.cs, powerShellCommand will be null, causing an exception to be thrown, crashing the
        // program.
        if (powerShellCommand is null)
        {
            Trace.WriteLine("Error: command is null");
            _availableParameters.Add("No valid command provided");
            return;
        }

        if (_availableParameters.Count == 0)
        {
            using var powerShell = System.Management.Automation.PowerShell.Create();
            string commandString =
                $"Get-Command {powerShellCommand.CommandText} | Select -ExpandProperty Parameters | ForEach-Object {{ $_.Keys }}";

            powerShell.Commands.Clear();
            powerShell.AddScript(commandString);

            if (isAsync)
            {
                PSDataCollection<PSObject> result = await powerShell.InvokeAsync();
                foreach (PSObject cmd in result)
                {
                    _availableParameters.Add($"-{cmd}");
                }
            }
            else
            {
                Collection<PSObject> result = powerShell.Invoke();
                foreach (PSObject cmd in result)
                {
                    _availableParameters.Add($"-{cmd}");
                }
            }
        }
    }
}
