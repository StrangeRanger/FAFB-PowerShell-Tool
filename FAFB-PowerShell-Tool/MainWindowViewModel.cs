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
    // ------------------- Fields ------------------- //

    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly PowerShellExecutor _powerShellExecutor;
    private Command _selectedCommand;
    private String _selectedParam;
    private string _powerShellOutput;
    private ObservableCollection<Button> _buttons;
    private CustomQueries cq = new CustomQueries();

    // ----------------- Properties ----------------- //

    /// <summary>
    /// Collection of Active Directory commands available for execution.
    /// </summary>
    public ObservableCollection<Command> ActiveDirectoryCommandList { get; private set; }

    /// <summary>
    /// Collection of Buttons for the stack panel
    /// </summary>
    public String SelectedParameter
    {
        get => _selectedParam;
        set {
            _selectedParam = value;
            OnPropertyChanged(nameof(SelectedParameter));
        }
    }

    /// <summary>
    /// TODO: Add a description for this property.
    /// </summary>
    public ObservableCollection<Button> ButtonStackPanel
    {
        get {
            if (_buttons == null)
                _buttons = new ObservableCollection<Button>();
            return _buttons;
        }
    }

    /// <summary>
    /// TODO: Add a description for this property.
    /// </summary>
    public ICommand SavedQueries { get; }

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
    /// Command to add a new parameter ComboBox to the UI.
    /// </summary>
    public ICommand Remove_ParameterComboBox { get; }

    /// <summary>
    /// Command to have the output send to a text file when executing
    /// </summary>
    public ICommand OutputToText { get; }

    /// <summary>
    /// Command to output to a csv when executing
    /// </summary>
    public ICommand OutputToCsv { get; }

    /// <summary>
    /// Command to output to a csv when executing
    /// </summary>
    public ICommand _AddButton { get; }

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

    // ----------------- Constructor ----------------- //

    /// <summary>
    /// Class constructor.
    /// </summary>
    /// <todo>Fix warnings about possible null values.</todo>
    public MainWindowViewModel()
    {
        _powerShellExecutor = new PowerShellExecutor();
        ExecuteCommand = new RelayCommand(Execute);
        AddNewParameterComboBox = new RelayCommand(AddParameterComboBox);
        Remove_ParameterComboBox = new RelayCommand(RemoveParameterComboBox);
        SavedQueries = new RelayCommand(PerformSavedQueries);
        _AddButton = new RelayCommand(AddButtonToStackPanel);

        DynamicParameterCollection = new ObservableCollection<ComboBoxParameterViewModel>();

        InitializeCommandsAsync();
        // calls method to deserialize and load buttons
        LoadCustomQueries();
    }

    // ----------------- Methods ----------------- //

    /// <summary>
    /// TODO: Add a description for this method.
    /// </summary>
    private void LoadCustomQueries()
    {
        try
        {
            cq.LoadData();

            // parameter counter
            int i = 0;
            foreach (CustomQueries.query cQuery in cq.Queries)
            {
                Command loadedCommand = new Command(cQuery.commandName);
                // loadedCommand.Parameters.Add(cQuery.commandParams[i]);

                Button newButton = new() { Content = cQuery.commandName, Height = 48, Tag = "{Binding loadedCommand}" };

                ButtonStackPanel.Add(newButton);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
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
    /// TODO: Add a description for this method.
    /// </summary>
    /// <param name="_"></param>
    private void AddButtonToStackPanel(object _)
    {
        Button newButton = new() { Content = "Special Command", Height = 48 };
        ButtonStackPanel.Add(newButton);
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

    /// <summary>
    /// This will be to Execute a query to a text file
    /// TODO: Add a description to the parameter of this method.
    /// </summary>
    /// <param name="_"></param>
    private void _OutputToText(object _)
    { }

    /// <summary>
    /// This will be to Execute a query to csv
    /// TODO: Add a description to the parameter of this method.
    /// </summary>
    /// <param name="_"></param>
    private void _OutputToCsv(object _)
    { }

    /// <summary>
    /// Removes the parameter box after adding them
    /// </summary>
    /// <param name="_"></param>
    private void RemoveParameterComboBox(object _)
    {
        if (DynamicParameterCollection.Count != 0)
        {
            DynamicParameterCollection.RemoveAt(DynamicParameterCollection.Count - 1);
        }
        else
        { }
    }

    /// <summary>
    /// TODO: Add a description for this method and its parameters.
    /// </summary>
    /// <param name="commandParameter"></param>
    private void PerformSavedQueries(object commandParameter)
    {
        // Try to get the content within the drop downs
        try
        {
            // Get the Command
            // GuiCommand? command = ComboBoxCommandList.SelectedValue as GuiCommand;
            string commandString = SelectedCommand.CommandText + SelectedParameter;

            // debug
            Trace.WriteLine(SelectedParameter);

            SelectedCommand.Parameters.Add(SelectedParameter);

            //
            foreach (var p in SelectedCommand.Parameters)
            {
                Trace.WriteLine(p.ToString());
            }
            // Trace.WriteLine(commandString);

            cq.SerializeCommand(SelectedCommand);

            Button newButton = new() { Content = SelectedCommand.CommandText, Height = 48 };
            ButtonStackPanel.Add(newButton);
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
