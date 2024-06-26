﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace ActiveDirectoryQuerier.ActiveDirectory;

// ReSharper disable once InconsistentNaming
public class ADCommandParameters
{
    private readonly ObservableCollection<string> _availableParameters = new();

    /// <remarks>
    /// IMPORTANT: This property should not be accessed before LoadAvailableParametersAsync or LoadAvailableParameters
    /// has been called. If it is accessed before either method has been called, no parameters will be available.
    /// </remarks>
    public ObservableCollection<string> AvailableParameters
    {
        get {
            if (_availableParameters.Count == 0)
            {
                Trace.WriteLine("Warning: LoadAvailableParametersAsync should be called before accessing " +
                                "AvailableParameters, to ensure asynchronous loading of parameters.");
                LoadAvailableParameters(null);
            }

            return _availableParameters;
        }
    }

    public async Task LoadAvailableParametersAsync(Command? psCommand)
    {
        await LoadAvailableParametersCore(psCommand, true);
    }

    public void LoadAvailableParameters(Command? psCommand)
    {
        LoadAvailableParametersCore(psCommand, false).Wait();
    }

    private async Task LoadAvailableParametersCore(Command? psCommand, bool isAsync)
    {
        // psCommand may be null if the user attempts to select an Active Directory command that doesn't exist. More
        // specifically, if the entered command doesn't exist in the ADCommands property defined in
        // MainWindowViewModel.cs, psCommand will be null, causing an exception to be thrown, crashing the program.
        if (psCommand is null)
        {
            Trace.WriteLine("Error: command is null");
            _availableParameters.Add("No valid command provided");
            return;
        }

        if (_availableParameters.Count == 0)
        {
            ICollection<PSObject> result;
            using var powerShell = System.Management.Automation.PowerShell.Create();
            var commandString = $"Get-Command {psCommand.CommandText} | Select -ExpandProperty Parameters | " +
                                $"ForEach-Object {{ $_.Keys }}";

            powerShell.Commands.Clear();
            powerShell.AddScript(commandString);

            if (isAsync)
            {
                result = await powerShell.InvokeAsync();
            }
            else
            {
                result = powerShell.Invoke();
            }

            foreach (PSObject adCommandParameter in result)
            {
                _availableParameters.Add($"-{adCommandParameter}");
            }
        }
    }
}
