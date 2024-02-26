using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ActiveDirectoryQuerier.PowerShell;
using Microsoft.Win32;

namespace ActiveDirectoryQuerier;

/// <summary>
/// Primary view model for the MainWindow.
/// </summary>
public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    // ------------------- Fields ------------------- //

    public event PropertyChangedEventHandler? PropertyChanged;
    private string _powerShellOutput;
    private string _queryName;
    private string _queryDescription;
    private string _parameterValue;
    private readonly PowerShellExecutor _powerShellExecutor;
    private Command? _selectedCommand;
    private CustomQueries _customQuery;
    private CustomQueries.Query _currentQuery;
    // Probably want to add the ability to toggle editing vs not editing but filled in.
    private CustomQueries.Query? _isEditing;
    private ObservableCollection<Button>? _buttons;

    // ----------------- Properties ----------------- //

    /// <summary>
    /// Collection of Active Directory commands available for execution.
    /// </summary>
    public ObservableCollection<Command> ActiveDirectoryCommandList { get; private set; }

    /// <summary>
    /// The name of the query.
    /// </summary>
    public string QueryName
    {
        get => _queryName;
        set {
            if (_queryName != value)
            {
                _queryName = value;
                OnPropertyChanged(nameof(QueryName));
            }
        }
    }

    /// <summary>
    /// This is the Parameter Value for the param
    /// TODO: Delete property 'ParameterValue' if it is not used.
    /// </summary>
    public string ParameterValue
    {
        get => _parameterValue;
        set {
            if (_parameterValue != value)
            {
                _parameterValue = value;
                OnPropertyChanged(nameof(ParameterValue));
            }
        }
    }

    /// <summary>
    /// The description of the query.
    /// </summary>
    public string QueryDescription
    {
        get => _queryDescription;
        set {
            if (_queryDescription != value)
            {
                _queryDescription = value;
                OnPropertyChanged(nameof(QueryDescription));
            }
        }
    }

    /// <summary>
    /// This property creates a collection of buttons to be added to the stack panel for custom queries
    /// </summary>
    public ObservableCollection<Button> ButtonStackPanel => _buttons ??= new ObservableCollection<Button>();

    /// <summary>
    /// This is a helper command for running PerformSavedQueries
    /// </summary>
    public ICommand SavedQueries { get; }

    /// <summary>
    /// This is the command for the edit option on custom queries
    /// </summary>
    public ICommand EditCustomQuery { get; }

    /// <summary>
    /// Collection of possible parameters for the currently selected command.
    /// </summary>
    public ObservableCollection<string>? PossibleParameterList { get; private set; }

    /// <summary>
    /// Collection of ComboBoxParameterViewModels to dynamically handle multiple parameter selections.
    /// </summary>
    public ObservableCollection<ComboBoxParameterViewModel> DynamicParameterCollection { get; }

    /// <summary>
    /// Collection of ComboBoxParameterViewModels to dynamically handle multiple parameter value selections.
    /// </summary>
    public ObservableCollection<TextBoxViewModel> DynamicParameterValuesCollection { get; }

    /// <summary>
    /// Command to execute the selected PowerShell command.
    /// </summary>
    public ICommand ExecuteCommand { get; }

    /// <summary>
    /// Command to execute the buttons inside Custom Command buttons
    /// </summary>
    public ICommand ExecuteCommandButton { get; }

    /// <summary>
    /// Command to add a new parameter ComboBox to the UI.
    /// </summary>
    public ICommand AddNewParameterComboBox { get; }

    /// <summary>
    /// Command to add a new command ComboBox to the UI.
    /// </summary>
    public ICommand AddNewCommandComboBox { get; }

    /// <summary>
    /// Command to add a new parameter ComboBox to the UI.
    /// TODO: Rename this property to match Property naming conventions!!!
    /// </summary>
    public ICommand Remove_ParameterComboBox { get; }

    /// <summary>
    /// Command to have the output send to a text file when executing
    /// TODO: Get-only auto-property 'OutputToText' is never assigned!!!
    /// </summary>
    public ICommand _OutputToText { get; }

    /// <summary>
    /// TODO: Add a summary.
    /// </summary>
    public ICommand DeleteCustomQuery { get; }

    /// <summary>
    /// Command to output to a csv when executing
    /// </summary>
    public ICommand OutputToCsv { get; }

    /// <summary>
    /// Gets or sets the currently selected PowerShell command.
    /// </summary>
    public Command? SelectedCommand
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
    /// TODO: Fix warnings about possible null values.
    /// </summary>
    public MainWindowViewModel()
    {
        // TODO: Possibly make the fields nullable such to enforce they are filled when saved and such.
        _powerShellOutput = string.Empty;
        // TODO: Possibly make the fields nullable such to enforce they are filled when saved and such.
        _queryName = string.Empty;
        // TODO: Possibly make the fields nullable such to enforce they are filled when saved and such.
        _queryDescription = string.Empty;
        // TODO: Possibly make the fields nullable such to enforce they are filled when saved and such.
        _parameterValue = string.Empty;
        _powerShellExecutor = new PowerShellExecutor();
        _customQuery = new CustomQueries();
        _currentQuery = new CustomQueries.Query();

        ActiveDirectoryCommandList = new ObservableCollection<Command>();
        ExecuteCommand = new RelayCommand(Execute);
        OutputToCsv = new RelayCommand(_OutputToCsvAsync);
        _OutputToText = new RelayCommand(OutputPowershellOutputToText);
        AddNewParameterComboBox = new RelayCommand(AddParameterComboBox);
        AddNewCommandComboBox = new RelayCommand(AddCommandComboBox);
        Remove_ParameterComboBox = new RelayCommand(RemoveParameterComboBox);
        SavedQueries = new RelayCommand(PerformSavedQueries);
        EditCustomQuery = new RelayCommand(PerformEditCustomQuery);
        DeleteCustomQuery = new RelayCommand(PerformDeleteCustomQuery);
        // Change the naming I know
        ExecuteCommandButton = new RelayCommand(ExecuteButtonCommand);

        DynamicParameterCollection = new ObservableCollection<ComboBoxParameterViewModel>();
        DynamicParameterValuesCollection = new ObservableCollection<TextBoxViewModel>();

        InitializeCommandsAsync();
        LoadCustomQueries(); // Calls method to deserialize and load buttons.

        // Debug
        foreach (Button t in ButtonStackPanel)
        {
            CustomQueries.Query test = (CustomQueries.Query)t.Tag;
            Trace.WriteLine(test.CommandName);
        }
    }

    // ----------------- Methods ----------------- //

    /// <summary>
    /// This method will edit the Query and fill out the field with the desired query and you can edit the query
    /// TODO: Fix any and all warnings about possible null values.
    /// </summary>
    /// <note>
    /// This method is of scope internal because it is tested in the test project, but should remain private.
    /// </note>
    /// <param name="sender">This is the object that is clicked when executing</param>
    private void PerformEditCustomQuery(object sender)
    {
        // Get the button that we are editing
        Button currButton = (Button)sender;
        CustomQueries.Query currQuery = (CustomQueries.Query)currButton.Tag;

        _isEditing = currQuery;

        // Need to fill in the queryName
        QueryName = currQuery.QueryName;
        // Fill in the queryDescription
        QueryDescription = currQuery.QueryDescription;
        // Fill in the commandName
        Command chosenCommand =
            ActiveDirectoryCommandList.FirstOrDefault(item => item.CommandText == currQuery.CommandName);
        SelectedCommand = chosenCommand;
        // Load the Possible Parameters Synchronously
        CommandParameters commandParameters = new CommandParameters();
        ((ICommandParameters)commandParameters).LoadCommandParameters(SelectedCommand);
        PossibleParameterList = new ObservableCollection<string>(commandParameters.PossibleParameters);
        OnPropertyChanged(nameof(PossibleParameterList));
        // Fill in Parameters and values

        for (int i = 0; i < currQuery.CommandParameters.Length; i++)
        {
            // Adds the Parameters boxes
            object temp = new();
            AddParameterComboBox(temp);
            // Fill in the parameter boxes
            DynamicParameterCollection[i].SelectedParameter =
                PossibleParameterList.FirstOrDefault(currQuery.CommandParameters[i]);
            DynamicParameterValuesCollection[i].SelectedParameterValue = currQuery.CommandParametersValues[i];
        }
    }

    /// <summary>
    /// This method executes the CustomQuery.query.command that is tied to the custom query buttons
    /// TODO: Fix any and all warnings about possible null values.
    /// </summary>
    /// <param name="_">This is the Button as a generic object that is clicked when executing</param>
    private async void ExecuteButtonCommand(object _)
    {
        var currentButton = _ as Button;

        if (currentButton is null)
        {
            return;
        }

        CustomQueries.Query buttonQuery = (CustomQueries.Query)currentButton.Tag;
        await ExecuteGenericCommand(buttonQuery.Command);
    }

    /// <summary>
    /// This method deletes the button from the list and saves the changes to the file
    /// </summary>
    /// <param name="_">This is the Button as a generic object that is clicked when executing</param>
    private void PerformDeleteCustomQuery(object _)
    {
        // Delete a button from the custom queries list and from the file
        var currentButton = _ as Button;

        if (currentButton is null)
        {
            return;
        }

        ButtonStackPanel.Remove(currentButton);
        _customQuery.Queries.Remove((CustomQueries.Query)currentButton.Tag);
        _customQuery.SerializeMethod();
    }

    /// <summary>
    /// This method will load the custom queries from the file.
    /// TODO: Fix any and all warnings about possible null values.
    /// TODO: Remove any unused variables.
    /// </summary>
    private void LoadCustomQueries()
    {
        try
        {
            _customQuery.LoadData();

            // parameter counter
            // int i = 0;
            foreach (CustomQueries.Query cQuery in _customQuery.Queries)
            {
                // Creates a new button for each query
                Button newButton = new() { Height = 48,
                                           // Names the query the command if null otherwise names it correctly
                                           Content = cQuery.QueryName ?? cQuery.CommandName,
                                           // Binds the query to the button
                                           Tag = cQuery };

                // Want to add right click context menu to each button
                ContextMenu contextMenu = new();

                MenuItem menuItem1 =
                    new() { Header = "Execute", Command = ExecuteCommandButton, CommandParameter = newButton };

                MenuItem menuItem2 = new() { Header = "Edit",
                                             Command = EditCustomQuery,
                                             // This set the parent of the menuitem to the button so it is accessible
                                             CommandParameter = newButton };

                MenuItem menuItem3 =
                    new() { Header = "Delete", Command = DeleteCustomQuery, CommandParameter = newButton };

                // Add menu item to the context menu
                contextMenu.Items.Add(menuItem1);
                contextMenu.Items.Add(menuItem2);
                contextMenu.Items.Add(menuItem3);

                // Add the context menu to the button
                newButton.ContextMenu = contextMenu;

                // Lastly add the button to the stack panel
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
    private async void LoadParametersAsync(Command? selectedCommand)
    {
        CommandParameters commandParameters = new();
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
        if (SelectedCommand is null)
        {
            Trace.WriteLine("No command selected.");
            MessageBox.Show("To execute a command, you must first select a command.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
            return;
        }

        try
        {
            // Add selected parameters and their values to the command.
            for (int i = 0; i < DynamicParameterCollection.Count; i++)
            {
                string parameterName = DynamicParameterCollection[i].SelectedParameter;
                string parameterValue = DynamicParameterValuesCollection[i].SelectedParameterValue;
                SelectedCommand.Parameters.Add(new CommandParameter(parameterName, parameterValue));
            }

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
    /// Executes the currently selected PowerShell command and updates the PowerShellOutput property with the result.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation of executing the command.</returns>
    private async Task ExecuteGenericCommand(Command? toBeExecuted)
    {
        try
        {
            // Execute the command
            ReturnValues result = await _powerShellExecutor.ExecuteAsync(toBeExecuted);
            // Error handling
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
    /// Adds a new parameter and value selection ComboBox to the DynamicParameterCollection.
    /// </summary>
    /// <param name="_">This is the object that the command is bound to</param>
    private void AddParameterComboBox(object _)
    {
        // Check if some variable is null and throw an exception if it is
        if (PossibleParameterList is null)
        {
            Trace.WriteLine("PossibleParameterList has not been populated yet.");
            return;
        }

        DynamicParameterCollection.Add(new ComboBoxParameterViewModel(PossibleParameterList));
        DynamicParameterValuesCollection.Add(new TextBoxViewModel());
    }

    /// <summary>
    /// TODO: Add a summary.
    /// </summary>
    /// <param name="_">This is the object that the command is bound to.</param>
    /// <exception cref="NotImplementedException"></exception>
    private void AddCommandComboBox(object _)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// This will be to Execute a query to a text file
    /// </summary>
    /// <param name="_">Represents the object that the command is bound to.</param>
    private async void OutputPowershellOutputToText(object _)
    {
        await ExecuteSelectedCommand();

        // Filepath
        // Write the text to a file & prompt user for the location

        SaveFileDialog saveFileDialog = new() {                       // Set properties of the OpenFileDialog
                                               FileName = "Document", // Default file name
                                               Filter = "All files(*.*) | *.*"
        };

        // Display
        bool? result = saveFileDialog.ShowDialog();

        // Get file and write text
        if (result == true)
        {
            // Open document
            string filePath = saveFileDialog.FileName;
            await File.WriteAllTextAsync(filePath, PowerShellOutput);
        }
    }

    /// <summary>
    /// TODO: Rename this method to match method naming conventions!!!
    /// This method is in the working
    /// Status: prompts user for file path and saves correctly though the string could be edited to be better
    /// </summary>
    /// <param name="_">Represents the object that the command is bound to</param>
    private async void _OutputToCsvAsync(object _)
    {
        await ExecuteSelectedCommand();

        var csv = new StringBuilder();
        string[] output = PowerShellOutput.Split(' ', '\n');

        for (int i = 0; i < output.Length - 2; i++)
        {
            var first = output[i];
            var second = output[i + 1];
            // format the strings and add them to a string
            var newLine = $"{first},{second}";
            csv.AppendLine(newLine);
        }

        // Write the text to a file & prompt user for the location

        SaveFileDialog saveFileDialog = new() {                       // Set properties of the OpenFileDialog
                                               FileName = "Document", // Default file name
                                               Filter = "All files(*.*) | *.*"
        };

        // Display
        bool? result = saveFileDialog.ShowDialog();

        // Get file and write text
        if (result == true)
        {
            // Open document
            string filePath = saveFileDialog.FileName;
            await File.WriteAllTextAsync(filePath, csv.ToString());
        }

        // debug
        // Trace.WriteLine(PowerShellOutput);
    }

    /// <summary>
    /// This method is for getting the currently selected command at anytime !Still in the works!
    /// TODO: Remove any unused variables.
    /// </summary>
    private void UpdateSelectedCommand()
    {
        if (SelectedCommand is null)
        {
            Trace.WriteLine("No command selected.");
            MessageBox.Show("To save a query, you must first select a command.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
            return;
        }

        // Try to get the content within the drop downs
        try
        {
            for (int i = 0; i < DynamicParameterCollection.Count; i++)
            {
                SelectedCommand.Parameters.Add(
                    new CommandParameter(DynamicParameterCollection[i].SelectedParameter,
                                         DynamicParameterValuesCollection[i].SelectedParameterValue));
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex);
        }
    }

    /// <summary>
    /// Gets the CustomQuery.query object for the current query when called
    /// TODO: It needs to be tested! Still in the works!
    /// TODO: Fix any and all warnings about possible null values.
    /// </summary>
    private void GetCurrentQuery()
    {
        // Update the selected command
        UpdateSelectedCommand();

        try
        {
            string[] commandParameters = new string[SelectedCommand.Parameters.Count];
            string[] commandParameterValues = new string[SelectedCommand.Parameters.Count];

            _currentQuery.QueryDescription = QueryDescription;
            _currentQuery.QueryName = QueryName;

            _currentQuery.CommandName = SelectedCommand.CommandText;

            int i = 0;
            foreach (CommandParameter commandParameter in SelectedCommand.Parameters)
            {
                commandParameters[i] = commandParameter.Name;
                commandParameterValues[i] = commandParameter.Value.ToString();
                i++;
            }
            _currentQuery.CommandParameters = commandParameters;
            _currentQuery.CommandParametersValues = commandParameterValues;
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    /// <summary>
    /// Removes the parameter box after adding them
    /// </summary>
    /// <param name="_">This is the CommandParameter object that this command is tied to</param>
    private void RemoveParameterComboBox(object _)
    {
        if (DynamicParameterCollection.Count != 0)
        {
            DynamicParameterCollection.RemoveAt(DynamicParameterCollection.Count - 1);
            DynamicParameterValuesCollection.RemoveAt(DynamicParameterValuesCollection.Count - 1);
        }
    }

    /// <summary>
    /// This method will serialize the command and add it to the list of buttons.
    /// TODO: Fix any and all warnings about possible null values.
    /// TODO: Remove any unused variables.
    /// </summary>
    /// <param name="commandParameter">This is not used but necessary for the relay command</param>
    private void PerformSavedQueries(object commandParameter)
    {
        if (SelectedCommand is null)
        {
            Trace.WriteLine("No command selected.");
            MessageBox.Show("To save a query, you must first select a command.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
            return;
        }

        if (_isEditing is not null)
        {
            // CustomQueries.query editingQuery = _customQuery.Queries.Find(item => item == isEditing);
            GetCurrentQuery();
            _customQuery.Queries[_customQuery.Queries.IndexOf(_isEditing)] = _currentQuery;
            _customQuery.SerializeMethod();
            _isEditing = null;
        }
        else
        {
            // Try to get the content within the drop downs
            try
            {
                int i = 0;
                foreach (var comboBoxData in DynamicParameterCollection)
                {
                    // string selectedItem = comboBoxData.SelectedParameter;
                    // Need to look at this to see if it is working with the object type and then serialize it
                    // Trace.WriteLine(DynamicParameterValuesCollection[i].SelectedParameterValue);

                    SelectedCommand.Parameters.Add(
                        new CommandParameter(comboBoxData.SelectedParameter,
                                             DynamicParameterValuesCollection[i].SelectedParameterValue));
                    i++;
                }

                _customQuery.SerializeCommand(SelectedCommand, QueryName, QueryDescription);

                Button newButton = CreateCustomButton();

                ButtonStackPanel.Add(newButton);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }
    }

    /// <summary>
    /// TODO: Add a summary.
    /// TODO: Add a description of the return value.
    /// </summary>
    /// <returns></returns>
    private Button CreateCustomButton()
    {
        if (SelectedCommand is null)
        {
            Trace.WriteLine("No command selected.");
            MessageBox.Show("To save a query, you must first select a command.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
            return null; // TODO: Figure out what to return here!!!
        }

        Button newButton =
            new() { Height = 48, Content = QueryName.Length != 0 ? QueryName : SelectedCommand.CommandText };

        // Want to add right click context menu to each button
        ContextMenu contextMenu = new();

        MenuItem menuItem1 = new() { Header = "Execute", Command = ExecuteCommandButton, CommandParameter = newButton };

        MenuItem menuItem2 = new() {
            Header = "Edit",
            Command = EditCustomQuery,
            CommandParameter = newButton // This set the parent of the menuitem to the button so it is accessible
        };

        MenuItem menuItem3 = new() { Header = "Delete", Command = DeleteCustomQuery, CommandParameter = newButton };

        // Add menu item to the context menu
        contextMenu.Items.Add(menuItem1);
        contextMenu.Items.Add(menuItem2);
        contextMenu.Items.Add(menuItem3);

        // Add the context menu to the button
        newButton.ContextMenu = contextMenu;
        return newButton;
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
