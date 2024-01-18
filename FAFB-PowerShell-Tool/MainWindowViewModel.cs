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
    public ObservableCollection<Command> Commands { get; private set; }
    public ObservableCollection<string> Parameters { get; private set; } // Adjust the type if needed
    public ICommand ExecuteCommand { get; }
    
    private readonly PowerShellExecutor _powerShellExecutor;

    private Command _selectedCommand;
    public Command SelectedCommand
    {
        get => _selectedCommand;
        set
        {
            _selectedCommand = value;
            OnPropertyChanged(nameof(SelectedCommand));
            LoadParametersAsync(value);
        }
    }
    
    private string _powerShellOutput;
    public string PowerShellOutput
    {
        get => _powerShellOutput;
        set
        {
            _powerShellOutput = value;
            OnPropertyChanged(nameof(PowerShellOutput));
        }
    }

    public MainWindowViewModel()
    {
        _powerShellExecutor = new PowerShellExecutor();
        InitializeCommandsAsync();
        LoadCustomQueries();
    }

    /// <summary>
    /// Loads the Active Directory commands into the Commands property.
    /// </summary>
    private async void InitializeCommandsAsync()
    {
        ObservableCollection<Command> list = await ActiveDirectoryCommands.GetActiveDirectoryCommands();
        Commands = new(list);
        OnPropertyChanged(nameof(Commands));
    }

    /// <summary>
    /// Loads the parameters for the selected command into the Parameters property.
    /// </summary>
    /// <param name="selectedCommand"></param>
    private async void LoadParametersAsync(Command selectedCommand)
    {
        if (selectedCommand is not null)
        {
            CommandParameters commandParameters = new CommandParameters();
            await commandParameters.LoadCommandParametersAsync(selectedCommand);
            Parameters = new ObservableCollection<string>(commandParameters.PossibleParameters); // Adjust if needed
            OnPropertyChanged(nameof(Parameters));
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