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
using static ActiveDirectoryQuerier.PowerShell.CustomQueries;

namespace ActiveDirectoryQuerier;

/// <summary>
/// Primary view model for the MainWindow.
/// </summary>
public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    // [ Fields ]-------------------------------------------------------------------- //
    // [[ Event handler property ]] ------------------------------------------------- //

    public event PropertyChangedEventHandler? PropertyChanged;

    // [[ Backing fields for properties ]] ------------------------------------------ //

    private string _powerShellOutput;
    private string _queryName;
    private string _queryDescription;
    private Command? _selectedComboBoxCommand;
    private ObservableCollection<Button>? _buttons;

    // [[ Other fields ]] ----------------------------------------------------------- //

    private readonly CustomQueries _customQuery;
    private readonly CustomQueries.Query _currentQuery;
    // Probably want to add the ability to toggle editing vs not editing but filled in.
    private CustomQueries.Query? _isEditing;
    private readonly PowerShellExecutor _powerShellExecutor;

    // [ Properties ] --------------------------------------------------------------- //
    // [[ Properties for backing fields ]] ------------------------------------------ //

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
    /// Gets or sets the currently selected PowerShell command.
    /// </summary>
    public Command? SelectedComboBoxCommand
    {
        get => _selectedComboBoxCommand;
        set {
            _selectedComboBoxCommand = value;
            OnPropertyChanged(nameof(SelectedComboBoxCommand));
            LoadCommandParametersAsync(value);
        }
    }

    /// <summary>
    /// This property creates a collection of buttons to be added to the stack panel for custom queries
    /// </summary>
    public ObservableCollection<Button> QueryButtonStackPanel => _buttons ??= new ObservableCollection<Button>();

    // [[ Other properties ]] ------------------------------------------------------- //

    /// <summary>
    /// Collection of Active Directory commands available for execution.
    /// </summary>
    public ObservableCollection<Command> ActiveDirectoryCommandsList { get; private set; }

    /// <summary>
    /// Collection of possible parameters for the currently selected command.
    /// </summary>
    public ObservableCollection<string>? PossibleCommandParametersList { get; private set; }

    /// <summary>
    /// Collection of ComboBoxParameterViewModels to dynamically handle multiple parameter selections.
    /// </summary>
    public ObservableCollection<ComboBoxParameterViewModel> DynamicParametersCollection { get; }

    /// <summary>
    /// Collection of ComboBoxParameterViewModels to dynamically handle multiple parameter value selections.
    /// </summary>
    public ObservableCollection<TextBoxViewModel> DynamicParameterValuesCollection { get; }

    /// <summary>
    /// This is a helper command for running SaveCustomQueries
    /// </summary>
    public ICommand SaveCustomQueriesRelay { get; }

    /// <summary>
    /// This is the command for the edit option on custom queries
    /// TODO: Change property name to be a non-verb.
    /// </summary>
    private ICommand EditCustomQueryRelay { get; }

    /// <summary>
    /// TODO: Add a summary.
    /// </summary>
    private ICommand DeleteCustomQueryRelay { get; }

    /// <summary>
    /// Command to execute the selected PowerShell command.
    /// TODO: Change property name???
    /// </summary>
    public ICommand ExecuteCommandRelay { get; }

    /// <summary>
    /// Command to execute the buttons inside Custom Command buttons
    /// TODO: Change property name???
    /// </summary>
    private ICommand ExecuteCommandButtonRelay { get; }

    /// <summary>
    /// Command to add a new parameter ComboBox to the UI.
    /// </summary>
    public ICommand AddParameterComboBoxRelay { get; }

    /// <summary>
    /// Command to add a new command ComboBox to the UI.
    /// </summary>
    public ICommand AddCommandComboBoxRelay { get; }

    /// <summary>
    /// Command to add a new parameter ComboBox to the UI.
    /// TODO: Rename this property to match Property naming conventions!!!
    /// </summary>
    public ICommand RemoveParameterComboBoxRelay { get; }

    /// <summary>
    /// Command to have the output send to a text file when executing
    /// TODO: Get-only auto-property 'OutputToTextFileAsync' is never assigned!!!
    /// </summary>
    public ICommand OutputToTextFileRelay { get; }

    /// <summary>
    /// Command to output to a csv when executing
    /// </summary>
    public ICommand OutputToCsvFileRelay { get; }

    // [ Constructors ] ------------------------------------------------------------ //

    /// <summary>
    /// Class constructor.
    /// TODO: Fix warnings about possible null values.
    /// </summary>
    public MainWindowViewModel()
    {
        // TODO: Possibly make the fields nullable to enforce they are filled when saved and such.
        _powerShellOutput = string.Empty;
        // TODO: Possibly make the fields nullable to enforce they are filled when saved and such.
        _queryName = string.Empty;
        // TODO: Possibly make the fields nullable to enforce they are filled when saved and such.
        _queryDescription = string.Empty;
        _powerShellExecutor = new PowerShellExecutor();
        _customQuery = new CustomQueries();
        _currentQuery = new CustomQueries.Query();

        ActiveDirectoryCommandsList = new ObservableCollection<Command>();
        DynamicParametersCollection = new ObservableCollection<ComboBoxParameterViewModel>();
        DynamicParameterValuesCollection = new ObservableCollection<TextBoxViewModel>();

        ExecuteCommandRelay = new RelayCommand(ExecuteCommandAsync);
        OutputToCsvFileRelay = new RelayCommand(OutputToCsvFileAsync);
        OutputToTextFileRelay = new RelayCommand(OutputToTextFileAsync);
        AddParameterComboBoxRelay = new RelayCommand(AddParameterComboBox);
        AddCommandComboBoxRelay = new RelayCommand(AddCommandComboBox);
        RemoveParameterComboBoxRelay = new RelayCommand(RemoveParameterComboBox);
        SaveCustomQueriesRelay = new RelayCommand(SaveCustomQueries);
        EditCustomQueryRelay = new RelayCommand(EditCustomQuery);
        DeleteCustomQueryRelay = new RelayCommand(DeleteCustomQuery);
        // TODO: Rename below property and method!!!
        ExecuteCommandButtonRelay = new RelayCommand(ExecuteCustomQueryCommandButton);

        InitializeActiveDirectoryCommandsAsync();
        LoadCustomQueries(); // Calls method to deserialize and load buttons.

        // Debug
        foreach (Button t in QueryButtonStackPanel)
        {
            CustomQueries.Query test = (CustomQueries.Query)t.Tag;
            Trace.WriteLine(test.CommandName);
        }
    }

    // [ Methods ] ----------------------------------------------------------------- //

    /// <summary>
    /// This method will edit the Query and fill out the field with the desired query and you can edit the query
    /// </summary>
    /// <note>
    /// This method is of scope internal because it is tested in the test project, but should remain private.
    /// </note>
    /// <param name="sender">This is the object that is clicked when executing</param>
    private void EditCustomQuery(object sender)
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
            ActiveDirectoryCommandsList.FirstOrDefault(item => item.CommandText == currQuery.CommandName)!;
        SelectedComboBoxCommand = chosenCommand;

        // Load the Possible Parameters Synchronously
        CommandParameters commandParameters = new CommandParameters();
        ((ICommandParameters)commandParameters).LoadCommandParameters(SelectedComboBoxCommand);
        PossibleCommandParametersList = new ObservableCollection<string>(commandParameters.PossibleParameters);
        OnPropertyChanged(nameof(PossibleCommandParametersList));

        // Check to see if the DynamicParametersCollection is empty and clear it if it is
        if (DynamicParametersCollection.Count != 0)
        {
            DynamicParametersCollection.Clear();
            DynamicParameterValuesCollection.Clear();
        }

        // Fill in Parameters and values
        for (int i = 0; i < currQuery.CommandParameters.Length; i++)
        {
            Trace.WriteLine(currQuery.CommandParameters[i]);

            // Adds the Parameters boxes
            object temp = new();
            AddParameterComboBox(temp);

            // Fill in the parameter boxes
            DynamicParametersCollection[i].SelectedParameter = currQuery.CommandParameters[i];
            DynamicParameterValuesCollection[i].SelectedParameterValue = currQuery.CommandParametersValues[i];
        }
    }

    /// <summary>
    /// This method executes the CustomQuery.query.command that is tied to the custom query buttons
    /// </summary>
    /// <param name="_">This is the Button as a generic object that is clicked when executing</param>
    private async void ExecuteCustomQueryCommandButton(object _)
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
    private void DeleteCustomQuery(object _)
    {
        // Delete a button from the custom queries list and from the file
        var currentButton = _ as Button;

        if (currentButton is null)
        {
            return;
        }

        QueryButtonStackPanel.Remove(currentButton);
        _customQuery.Queries.Remove((CustomQueries.Query)currentButton.Tag);
        _customQuery.SerializeMethod();
    }

    /// <summary>
    /// This method will load the custom queries from the file.
    /// TODO: Remove any unused variables.
    /// </summary>
    private void LoadCustomQueries()
    {
        try
        {
            // Load the custom queries from the file (Deserialize)
            _customQuery.LoadData();

            // Loop through the queries and create a button for each one
            foreach (CustomQueries.Query cQuery in _customQuery.Queries)
            {
                // Creates a new button for each query
                Button newButton = CreateCustomButton(cQuery);

                // Lastly add the button to the stack panel
                QueryButtonStackPanel.Add(newButton);
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
    private async void ExecuteCommandAsync(object _)
    {
        await ExecuteSelectedCommandAsync();
    }

    /// <summary>
    /// Initializes the list of Active Directory commands asynchronously.
    /// </summary>
    private async Task InitializeActiveDirectoryCommandsAsync()
    {
        ObservableCollection<Command> list = await ActiveDirectoryCommands.GetActiveDirectoryCommands();
        ActiveDirectoryCommandsList = new(list);
        OnPropertyChanged(nameof(ActiveDirectoryCommandsList));
    }

    /// <summary>
    /// Loads the parameters for the selected PowerShell command asynchronously.
    /// </summary>
    /// <param name="selectedCommand">The PowerShell command whose parameters are to be loaded.</param>
    private async Task LoadCommandParametersAsync(Command? selectedCommand)
    {
        CommandParameters commandParameters = new();
        await commandParameters.LoadCommandParametersAsync(selectedCommand);
        PossibleCommandParametersList = new ObservableCollection<string>(commandParameters.PossibleParameters);
        OnPropertyChanged(nameof(PossibleCommandParametersList));

        // Update the possible properties of the ComboBoxParameterViewModels.
        foreach (ComboBoxParameterViewModel comboBoxParameterViewModel in DynamicParametersCollection)
        {
            comboBoxParameterViewModel.PossibleParameterList = PossibleCommandParametersList;
        }
    }

    /// <summary>
    /// Executes the currently selected PowerShell command and updates the PowerShellOutput property with the result.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation of executing the command.</returns>
    private async Task ExecuteSelectedCommandAsync()
    {
        if (SelectedComboBoxCommand is null)
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
            for (int i = 0; i < DynamicParametersCollection.Count; i++)
            {
                string parameterName = DynamicParametersCollection[i].SelectedParameter;
                string parameterValue = DynamicParameterValuesCollection[i].SelectedParameterValue;
                SelectedComboBoxCommand.Parameters.Add(new CommandParameter(parameterName, parameterValue));
            }

            ReturnValues result = await _powerShellExecutor.ExecuteAsync(SelectedComboBoxCommand);

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
    /// Executes the inputted Command PowerShell command and updates the PowerShellOutput property with the result.
    /// TODO: Rename this method...
    /// TODO: Does this method do the same thing an another method???
    /// </summary>
    /// <param name="toBeExecuted">This is the command to be executed</param>
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
    /// Adds a new parameter and value selection ComboBox to the DynamicParametersCollection.
    /// </summary>
    /// <param name="_">This is the object that the command is bound to</param>
    private void AddParameterComboBox(object _)
    {
        // Check if some variable is null and throw an exception if it is
        if (PossibleCommandParametersList is null)
        {
            Trace.WriteLine("PossibleCommandParametersList has not been populated yet.");
            MessageBox.Show("To add a parameter, you must first select a command.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
            return;
        }

        DynamicParametersCollection.Add(new ComboBoxParameterViewModel(PossibleCommandParametersList));
        DynamicParameterValuesCollection.Add(new TextBoxViewModel());
    }

    /// <summary>
    /// TODO: Add a summary.
    /// </summary>
    /// <param name="_">This is the object that the command is bound to.</param>
    private void AddCommandComboBox(object _)
    {
        // throw new NotImplementedException();
        Trace.WriteLine("Not implemented yet.");
        MessageBox.Show("Not implemented yet.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// This method executes the currently selected command and saves the output to a text file.
    /// </summary>
    /// <param name="_">Represents the object that the command is bound to.</param>
    private async void OutputToTextFileAsync(object _)
    {
        await ExecuteSelectedCommandAsync();

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
    /// This method is in the working
    /// Status: prompts user for file path and saves correctly though the string could be edited to be better
    /// </summary>
    /// <param name="_">Represents the object that the command is bound to</param>
    private async void OutputToCsvFileAsync(object _)
    {
        await ExecuteSelectedCommandAsync();

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
        SaveFileDialog saveFileDialog = new() { FileName = "Document", Filter = "All files(*.*) | *.*" };

        // Display
        bool? result = saveFileDialog.ShowDialog();

        // Get file and write text
        if (result == true)
        {
            // Open document
            string filePath = saveFileDialog.FileName;
            await File.WriteAllTextAsync(filePath, csv.ToString());
        }
    }

    /// <summary>
    /// This method is for getting the currently selected command at anytime
    /// TODO: !Still in the works!
    /// TODO: Remove any unused variables.
    /// TODO: Does this method do the same thing an another method???
    /// </summary>
    private void UpdateSelectedCommand()
    {
        if (SelectedComboBoxCommand is null)
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
            for (int i = 0; i < DynamicParametersCollection.Count; i++)
            {
                SelectedComboBoxCommand.Parameters.Add(
                    new CommandParameter(DynamicParametersCollection[i].SelectedParameter,
                                         DynamicParameterValuesCollection[i].SelectedParameterValue));
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex);
        }
    }

    /// <summary>
    /// Updates the _currentQuery object with the current query information.
    /// TODO: It needs to be tested! Still in the works!
    /// TODO: Fix any and all warnings about possible null values.
    /// </summary>
    private void GetCurrentQuery()
    {
        // Update the selected command
        UpdateSelectedCommand();

        try
        {
            string[] commandParameters = new string[SelectedComboBoxCommand.Parameters.Count];
            string[] commandParameterValues = new string[SelectedComboBoxCommand.Parameters.Count];

            _currentQuery.QueryDescription = QueryDescription;
            _currentQuery.QueryName = QueryName;

            _currentQuery.CommandName = SelectedComboBoxCommand.CommandText;

            int i = 0;
            foreach (CommandParameter commandParameter in SelectedComboBoxCommand.Parameters)
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
        if (DynamicParametersCollection.Count != 0)
        {
            DynamicParametersCollection.RemoveAt(DynamicParametersCollection.Count - 1);
            DynamicParameterValuesCollection.RemoveAt(DynamicParameterValuesCollection.Count - 1);
        }
    }

    /// <summary>
    /// This method will serialize the command and add it to the list of buttons.
    /// TODO: Fix any and all warnings about possible null values.
    /// TODO: Remove any unused variables.
    /// </summary>
    /// <param name="commandParameter">This is not used but necessary for the relay command</param>
    private void SaveCustomQueries(object commandParameter)
    {
        if (SelectedComboBoxCommand is null)
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
                foreach (var comboBoxData in DynamicParametersCollection)
                {
                    // string selectedItem = comboBoxData.SelectedParameter;
                    // Need to look at this to see if it is working with the object type and then serialize it
                    // Trace.WriteLine(DynamicParameterValuesCollection[i].SelectedParameterValue);

                    SelectedComboBoxCommand.Parameters.Add(
                        new CommandParameter(comboBoxData.SelectedParameter,
                                             DynamicParameterValuesCollection[i].SelectedParameterValue));
                    i++;
                }

                _customQuery.SerializeCommand(SelectedComboBoxCommand, QueryName, QueryDescription);

                Button newButton = CreateCustomButton();

                QueryButtonStackPanel.Add(newButton);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }
    }

    /// <summary>
    /// This method is for creating buttons, right now it creates it off of the current/selectedcommand parameters but
    /// could be changed to also do it from the query list.
    /// TODO: Change method name???
    /// </summary>
    /// <returns>This method returns a button that has been customized for the custom query list</returns>
    private Button CreateCustomButton(CustomQueries.Query query = null)
    {
        GetCurrentQuery();

        if (SelectedComboBoxCommand is null)
        {
            Trace.WriteLine("No command selected.");
            MessageBox.Show("To save a query, you must first select a command.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
            return null; // TODO: Figure out what to return here!!!
        }

        Button newButton = new Button();

        if (query != null)
        {
            newButton.Height = 48;
            newButton.Content = query.QueryName ?? query.CommandName;
            newButton.Tag = query;
        }
        else
        {
            newButton.Height = 48;
            newButton.Content = QueryName.Length != 0 ? QueryName : SelectedComboBoxCommand.CommandText;
            newButton.Tag = _currentQuery;
        }

        // Button newButton =
        // new() { Height = 48, Content = QueryName.Length != 0 ? QueryName : SelectedComboBoxCommand.CommandText, Tag =
        // _currentQuery };

        // Want to add right click context menu to each button
        ContextMenu contextMenu = new();

        MenuItem menuItem1 =
            new() { Header = "Execute", Command = ExecuteCommandButtonRelay, CommandParameter = newButton };

        MenuItem menuItem2 = new() { Header = "Edit", Command = EditCustomQueryRelay, CommandParameter = newButton };

        MenuItem menuItem3 =
            new() { Header = "Delete", Command = DeleteCustomQueryRelay, CommandParameter = newButton };

        // Add menu item to the context menu
        contextMenu.Items.Add(menuItem1);
        contextMenu.Items.Add(menuItem2);
        contextMenu.Items.Add(menuItem3);

        // Add the context menu to the button
        newButton.ContextMenu = contextMenu;
        return newButton;
    }

    // [[ Event Handlers ]] --------------------------------------------------------- //

    /// <summary>
    /// Handles property change notifications.
    /// </summary>
    /// <param name="propertyName">Name of the property that changed.</param>
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
