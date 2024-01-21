using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation.Runspaces;
using System.Windows.Controls;
using System.Windows.Input;
using FAFB_PowerShell_Tool.PowerShell;

namespace FAFB_PowerShell_Tool;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public ObservableCollection<Command> ActiveDirectoryCommandList { get; private set; }
    public ObservableCollection<string> PossibleParameterList { get; private set; }
    public ICommand ExecuteCommand { get; }

    private readonly PowerShellExecutor _powerShellExecutor;

    private Command _selectedCommand;
    public Command SelectedCommand
    {
        get => _selectedCommand;
        set {
            _selectedCommand = value;
            OnPropertyChanged(nameof(SelectedCommand));
            LoadParametersAsync(value);
        }
    }

    private string _powerShellOutput;
    public string PowerShellOutput
    {
        get => _powerShellOutput;
        set {
            _powerShellOutput = value;
            OnPropertyChanged(nameof(PowerShellOutput));
        }
    }

    public MainWindowViewModel()
    {
        _powerShellExecutor = new PowerShellExecutor();
        ExecuteCommand = new RelayCommand(Execute);
        InitializeCommandsAsync();
        LoadCustomQueries();
    }

    private async void Execute(object _)
    {
        await ExecuteSelectedCommand();
    }

    /// <summary>
    /// Loads the Active Directory commands into the ActiveDirectoryCommandList property.
    /// </summary>
    private async void InitializeCommandsAsync()
    {
        ObservableCollection<Command> list = await ActiveDirectoryCommands.GetActiveDirectoryCommands();
        ActiveDirectoryCommandList = new(list);
        OnPropertyChanged(nameof(ActiveDirectoryCommandList));
    }

    /// <summary>
    /// Loads the parameters for the selected command into the PossibleParameterList property.
    /// </summary>
    /// <param name="selectedCommand"></param>
    private async void LoadParametersAsync(Command selectedCommand)
    {
        if (selectedCommand is not null)
        {
            CommandParameters commandParameters = new CommandParameters();
            await commandParameters.LoadCommandParametersAsync(selectedCommand);
            PossibleParameterList = new ObservableCollection<string>(commandParameters.PossibleParameters);
            OnPropertyChanged(nameof(PossibleParameterList));
        }
    }

    private async Task ExecuteSelectedCommand()
    {
        try
        {
            // Execute the command and get the results
            ReturnValues result = await _powerShellExecutor.ExecuteAsync(SelectedCommand);

            if (result.HadErrors)
            {
                // Handle errors here
                // For example, display an error message in the UI
                PowerShellOutput = string.Join(Environment.NewLine, result.StdErr);
                return;
            }

            // Process the results here
            // For example, you might want to display them in the UI
            // Update a property that is bound to the UI
            PowerShellOutput = string.Join(Environment.NewLine, result.StdOut);
        }
        catch (Exception ex)
        {
            // Handle any exceptions that occurred during execution
            // For example, display an error message in the UI
            PowerShellOutput = $"Error executing command: {ex.Message}";
        }
    }

    public void SaveQueryCommand(string query)
    {
        // TODO: Write logic to save queries...
    }

    private void LoadCustomQueries()
    {
        // TODO: Write logic to load queries...
    }

    // ....BELOW CODE.... //

    /// <summary>
    /// This is the event handler for the INotifyPropertyChanged interface.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// This is the method that is called when a property is changed.
    /// </summary>
    /// <param name="propertyName"></param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
