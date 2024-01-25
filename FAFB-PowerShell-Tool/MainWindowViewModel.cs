using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation.Runspaces;
using System.Windows.Controls;
using System.Windows.Input;
using FAFB_PowerShell_Tool.PowerShell;

namespace FAFB_PowerShell_Tool;

/// <summary>
/// Primary view model for the MainWindow.
/// </summary>
public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly PowerShellExecutor _powerShellExecutor;
    private Command _selectedCommand;
    private string _powerShellOutput;

    /// <summary>
    /// Collection of Active Directory commands available for execution.
    /// </summary>
    public ObservableCollection<Command> ActiveDirectoryCommandList { get; private set; }

    /// <summary>
    /// Collection of possible parameters for the currently selected command.
    /// </summary>
    public ObservableCollection<string> PossibleParameterList { get; private set; }

    /// <summary>
    /// Collection of ComboBoxParameterViewModels to dynamically handle multiple parameter selections.
    /// </summary>
    public ObservableCollection<ComboBoxParameterViewModel> DynamicParameterCollection { get; }

    /// <summary>
    /// Command to execute the selected PowerShell command.
    /// </summary>
    public ICommand ExecuteCommand { get; }

    /// <summary>
    /// Command to add a new parameter ComboBox to the UI.
    /// </summary>
    public ICommand AddNewParameterComboBox { get; }

    /// <summary>
    /// Gets or sets the currently selected PowerShell command.
    /// </summary>
    public Command SelectedCommand
    {
        get => _selectedCommand;
        set {
            _selectedCommand = value;
            OnPropertyChanged(nameof(SelectedCommand));
            LoadParametersAsync(value);
        }
    }

    /// <summary>
    /// Gets or sets the output of the executed PowerShell command.
    /// </summary>
    public string PowerShellOutput
    {
        get => _powerShellOutput;
        set {
            _powerShellOutput = value;
            OnPropertyChanged(nameof(PowerShellOutput));
        }
    }

    /// <summary>
    /// Class constructor.
    /// </summary>
    /// <todo>Fix warnings about possible null values.</todo>
    public MainWindowViewModel()
    {
        _powerShellExecutor = new PowerShellExecutor();
        ExecuteCommand = new RelayCommand(Execute);
        AddNewParameterComboBox = new RelayCommand(AddParameterComboBox);

        DynamicParameterCollection = new ObservableCollection<ComboBoxParameterViewModel>();

        InitializeCommandsAsync();
        LoadCustomQueries();
    }

    /// <summary>
    /// Executes the selected PowerShell command asynchronously.
    /// </summary>
    /// <param name="_">Parameter is not used, but required for the ICommand interface.</param>
    private async void Execute(object _)
    {
        await ExecuteSelectedCommand();
    }

    /// <summary>
    /// Initializes the list of Active Directory commands asynchronously.
    /// </summary>
    private async void InitializeCommandsAsync()
    {
        ObservableCollection<Command> list = await ActiveDirectoryCommands.GetActiveDirectoryCommands();
        ActiveDirectoryCommandList = new(list);
        OnPropertyChanged(nameof(ActiveDirectoryCommandList));
    }

    /// <summary>
    /// Loads the parameters for the selected PowerShell command asynchronously.
    /// </summary>
    /// <param name="selectedCommand">The PowerShell command whose parameters are to be loaded.</param>
    private async void LoadParametersAsync(Command selectedCommand)
    {
        CommandParameters commandParameters = new CommandParameters();
        await commandParameters.LoadCommandParametersAsync(selectedCommand);
        PossibleParameterList = new ObservableCollection<string>(commandParameters.PossibleParameters);
        OnPropertyChanged(nameof(PossibleParameterList));

        // Update the possible properties of the ComboBoxParameterViewModels.
        foreach (ComboBoxParameterViewModel comboBoxParameterViewModel in DynamicParameterCollection)
        {
            comboBoxParameterViewModel.PossibleParameterList = PossibleParameterList;
        }
    }

    /// <summary>
    /// Executes the currently selected PowerShell command and updates the PowerShellOutput property with the result.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation of executing the command.</returns>
    private async Task ExecuteSelectedCommand()
    {
        try
        {
            ReturnValues result = await _powerShellExecutor.ExecuteAsync(SelectedCommand);

            if (result.HadErrors)
            {
                PowerShellOutput = string.Join(Environment.NewLine, result.StdErr);
                return;
            }

            PowerShellOutput = string.Join(Environment.NewLine, result.StdOut);
        }
        catch (Exception ex)
        {
            PowerShellOutput = $"Error executing command: {ex.Message}";
        }
    }

    /// <summary>
    /// Adds a new parameter selection ComboBox to the DynamicParameterCollection.
    /// </summary>
    private void AddParameterComboBox(object _)
    {
        DynamicParameterCollection.Add(new ComboBoxParameterViewModel(PossibleParameterList));
    }

    public void SaveQueryCommand(string query)
    {
        // TODO: Write logic to save queries...
    }

    private void LoadCustomQueries()
    {
        // TODO: Write logic to load queries...
    }

    // This
    private RelayCommand savedQueries;
    //
    public ICommand SavedQueries => savedQueries ??= new RelayCommand(PerformSavedQueries);

    //
    private void PerformSavedQueries(object commandParameter)
    {
        // Try to get the content within the drop downs
        try
        {
            // Get the Command
            // GuiCommand? command = ComboBoxCommandList.SelectedValue as GuiCommand;
            string commandString = SelectedCommand.CommandText + SelectedCommand.Parameters.ToString();
            // query

            Trace.WriteLine(commandString);

            CustomQueries cq = new CustomQueries();

            cq.SerializeCommand(SelectedCommand);

            // string commandParameters = cmbParameters.Text;

            Button newButton = new() { Content = "Special Command", Height = 48 };
            // Adds the query to the Queries serializable list

            // Adds the button in the stack panel

            // Saves the Queries to the file
        }
        catch (Exception ex)
        {
            Console.Write(ex);
        }
    }

    /// <summary>
    /// Handles property change notifications.
    /// </summary>
    /// <param name="propertyName">Name of the property that changed.</param>
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
