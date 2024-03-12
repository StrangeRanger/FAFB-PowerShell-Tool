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
using ActiveDirectoryQuerier.Queries;
using Microsoft.Win32;

namespace ActiveDirectoryQuerier;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    // [ Fields ] ------------------------------------------------------------------- //
    // [[ Event handler property ]] ------------------------------------------------- //

    public event PropertyChangedEventHandler? PropertyChanged;

    // [[ Backing fields for properties ]] ------------------------------------------ //

    private bool _editingEnabled;
    private string _queryName;
    private string _queryDescription;
    private AppConsole _queryBuilderConsoleOutput;
    private AppConsole _activeDirectoryInfoConsoleOutput;
    private Command? _selectedComboBoxCommandInQueryBuilder;
    private ObservableCollection<Button>? _buttons;

    // [[ Other fields ]] ----------------------------------------------------------- //

    private readonly Query _currentQuery;
    private readonly CustomQueries _customQuery;
    private readonly PSExecutor _psExecutor;
    private Query? _isEditing;

    // [ Properties ] --------------------------------------------------------------- //
    // [[ Properties for backing fields ]] ------------------------------------------ //

    /// <summary>
    /// This is tied to the Editing checkbox and will allow the user to edit the custom queries or not to
    /// </summary>
    public bool EditingEnabled
    {
        get => _editingEnabled;
        set {
            _editingEnabled = value;
            OnPropertyChanged(nameof(EditingEnabled));
        }
    }
    
    public AppConsole QueryBuilderConsoleOutput
    {
        get => _queryBuilderConsoleOutput;
        set {
            _queryBuilderConsoleOutput = value;
            OnPropertyChanged(nameof(QueryBuilderConsoleOutput));
        }
    }

    public AppConsole ActiveDirectoryInfoConsoleOutput
    {
        get => _activeDirectoryInfoConsoleOutput;
        set {
            _activeDirectoryInfoConsoleOutput = value;
            OnPropertyChanged(nameof(ActiveDirectoryInfoConsoleOutput));
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
    
    public Command? SelectedComboBoxCommandInQueryBuilder
    {
        get => _selectedComboBoxCommandInQueryBuilder;
        set {
            _selectedComboBoxCommandInQueryBuilder = value;
            OnPropertyChanged(nameof(SelectedComboBoxCommandInQueryBuilder));
            // No need to load parameters if the command is null.
            if (value is not null)
            {
                // TODO: Figure out how to resolve the warning about the async method not being awaited!!!
                LoadCommandParametersAsync(value);
            }
        }
    }

    /// <summary>
    /// This property creates a collection of buttons to be added to the stack panel for custom queries
    /// </summary>
    public ObservableCollection<Button> QueryButtonStackPanel => _buttons ??= new ObservableCollection<Button>();

    // [[ Other properties ]] ------------------------------------------------------- //
    
    // ReSharper disable once InconsistentNaming
    public ObservableCollection<Command> ADCommands { get; private set; }
    // ReSharper disable once InconsistentNaming
    public ObservableCollection<string>? AvailableADCommandParameters { get; private set; }

    /// <summary>
    /// Collection of ComboBoxParameterViewModels to dynamically handle multiple parameter selections.
    /// </summary>
    /// TODO: Rename this property?
    public ObservableCollection<ComboBoxParameterViewModel> DynamicParameterComboBoxes { get; }

    /// <summary>
    /// Collection of ComboBoxParameterViewModels to dynamically handle multiple parameter value selections.
    /// </summary>
    /// TODO: Rename this property?
    public ObservableCollection<TextBoxViewModel> DynamicParameterValueTextBoxes { get; }

    /// <summary>
    /// This is a helper command for running SaveCustomQueries
    /// </summary>
    public ICommand SaveCustomQueriesRelay { get; }

    /// <summary>
    /// This is the command tied to Clear Query calls the ClearQueryBuilder method
    /// </summary>
    public ICommand ClearQueryBuilderRelay { get; }

    /// <summary>
    /// This is the command for the edit option on custom queries
    /// </summary>
    private ICommand EditCustomQueryRelay { get; }

    /// <summary>
    /// This is tied to the Button menu calls the DeleteCustomQuery method to delete a CustomQuery button
    /// </summary>
    private ICommand DeleteCustomQueryRelay { get; }
    
    public ICommand ExecuteSelectedCommandRelay { get; }
    private ICommand ExecuteCustomQueryCommandButtonRelay { get; }
    public ICommand AddParameterComboBoxRelay { get; }
    public ICommand AddCommandComboBoxRelay { get; }
    public ICommand RemoveParameterComboBoxRelay { get; }
    public ICommand OutputToTextFileRelay { get; }
    public ICommand OutputToCsvFileRelay { get; }
    public ICommand ExportConsoleOutputRelay { get; }
    public ICommand ClearConsoleOutputRelay { get; }

    /* TODO: for Pieter
     *
     * Create a property that contains the ComboBox dropdown options/text.
     *
     * Create a property that will contain the selected item.
     *
     * Create a property to act as the relay to the execution button.
     */

    // [ Constructors ] ------------------------------------------------------------- //
    
    public MainWindowViewModel()
    {
        _queryName = string.Empty;
        _queryDescription = string.Empty;
        _queryBuilderConsoleOutput = new AppConsole();
        _activeDirectoryInfoConsoleOutput = new AppConsole();
        _psExecutor = new PSExecutor();
        _customQuery = new CustomQueries();
        _currentQuery = new Query();

        ADCommands = new ObservableCollection<Command>();
        DynamicParameterComboBoxes = new ObservableCollection<ComboBoxParameterViewModel>();
        DynamicParameterValueTextBoxes = new ObservableCollection<TextBoxViewModel>();

        ExecuteSelectedCommandRelay = new RelayCommand(ExecuteSelectedCommandAsync);
        OutputToCsvFileRelay = new RelayCommand(OutputToCsvFileAsync);
        OutputToTextFileRelay = new RelayCommand(OutputToTextFileAsync);
        ExportConsoleOutputRelay = new RelayCommand(ExportConsoleOutput);
        AddParameterComboBoxRelay = new RelayCommand(AddParameterComboBox);
        AddCommandComboBoxRelay = new RelayCommand(AddCommandComboBox);
        RemoveParameterComboBoxRelay = new RelayCommand(RemoveParameterComboBox);
        SaveCustomQueriesRelay = new RelayCommand(SaveCustomQueries);
        EditCustomQueryRelay = new RelayCommand(EditCustomQuery);
        DeleteCustomQueryRelay = new RelayCommand(DeleteCustomQuery);
        ExecuteCustomQueryCommandButtonRelay = new RelayCommand(ExecuteCustomQueryCommandButton);
        ClearConsoleOutputRelay = new RelayCommand(ClearConsoleOutput);
        ClearQueryBuilderRelay = new RelayCommand(ClearQueryBuilder);
        
        /* TODO: For Pieter
         * Connect the relay property to for the execute button to you method that performs the execution.
         */
        

        // TODO: Figure out how resolve the warning about the async method not being awaited.
        InitializeActiveDirectoryCommandsAsync();
        LoadCustomQueries(); // Calls method to deserialize and load buttons.
    }

    // [ Methods ] ----------------------------------------------------------------- //

    /* TODO: Info for Pieter to get started
     * Create a class (outside of this one) that will contain three methods, all of each will perform one specific
     * action, such as getting the users on the domain, getting computers on the domain, and getting the IP of each
     * system on the domain.
     *      - The methods in this class should use the powershell executor to execute the specific command.
     *      - Make method async, and utilize the async execute methods in the powershell executor.
     *
     *      public async Task<the return type> MethodName() { } // don't forget to use await when dealing with async
     *                                                             methods.
     *
     * In this class, create a method or two, that will be used to execute the specific selected action.
     */
    
    /// <summary>
    /// This method will clear the console output and prompt the user if they are sure they want to clear the console.
    /// </summary>
    /// <param name="_">This is the object that the command is tied to.</param>
    private void ClearConsoleOutput(object _)
    {
        if (QueryBuilderConsoleOutput.ConsoleOutput.Length == 0)
        {
            MessageBox.Show("The console is already clear.",
                            "Information",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
            return;
        }

        MessageBoxResult result = MessageBox.Show("Are you sure you want to clear the console output?",
                                                  "Warning",
                                                  MessageBoxButton.YesNo,
                                                  MessageBoxImage.Warning,
                                                  MessageBoxResult.No);

        if (result == MessageBoxResult.Yes)
        {
            QueryBuilderConsoleOutput.Clear();
        }
    }

    /// <summary>
    /// This method will edit the Query and fill out the field with the desired query and you can edit the query
    /// </summary>
    /// <param name="sender">This is the object that is clicked when executing</param>
    private void EditCustomQuery(object sender)
    {
        // Get the button that we are editing
        Button currentButton = (Button)sender;
        Query currentQuery = (Query)currentButton.Tag;

        _isEditing = currentQuery;
        EditingEnabled = true;

        // Need to fill in the queryName
        QueryName = currentQuery.QueryName;

        // Fill in the queryDescription
        QueryDescription = currentQuery.QueryDescription;

        // Fill in the commandName
        Command chosenCommand =
            ADCommands.FirstOrDefault(item => item.CommandText == currentQuery.PSCommandName)!;
        SelectedComboBoxCommandInQueryBuilder = chosenCommand;

        // Load the Possible Parameters Synchronously
        ADCommandParameters adCommandParameters = new();
        adCommandParameters.LoadAvailableParameters(SelectedComboBoxCommandInQueryBuilder);
        AvailableADCommandParameters = new ObservableCollection<string>(adCommandParameters.AvailableParameters);
        OnPropertyChanged(nameof(AvailableADCommandParameters));

        // Check to see if the DynamicParameterComboBoxes is empty and clear it if it is
        if (DynamicParameterComboBoxes.Count != 0)
        {
            DynamicParameterComboBoxes.Clear();
            DynamicParameterValueTextBoxes.Clear();
        }

        // Fill in Parameters and values
        for (int i = 0; i < currentQuery.PSCommandParameters.Length; i++)
        {
            Trace.WriteLine(currentQuery.PSCommandParameters[i]);

            // Adds the Parameters boxes
            object temp = new();
            AddParameterComboBox(temp);

            // Fill in the parameter boxes
            DynamicParameterComboBoxes[i].SelectedParameter = currentQuery.PSCommandParameters[i];
            DynamicParameterValueTextBoxes[i].SelectedParameterValue = currentQuery.PSCommandParameterValues[i];
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
            Trace.WriteLine("No button selected.");
            MessageBox.Show("To execute a query, you must first select a query.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
            return;
        }

        Query buttonQuery = (Query)currentButton.Tag;
        await ExecuteSelectedCommandCoreAsync(buttonQuery.Command);
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
            Trace.WriteLine("No button selected.");
            MessageBox.Show("To delete a query, you must first select a query.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
            return;
        }

        QueryButtonStackPanel.Remove(currentButton);
        _customQuery.Queries.Remove((Query)currentButton.Tag);
        _customQuery.SaveQueriesToJson();
    }

    /// <summary>
    /// This method will load the custom queries from the file.
    /// </summary>
    private void LoadCustomQueries()
    {
        try
        {
            // Load the custom queries from the file (Deserialize)
            _customQuery.LoadData();

            // Loop through the queries and create a button for each one
            foreach (Query cQuery in _customQuery.Queries)
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
    /// <param name="_">This is the object that the command is bound to.</param>
    private async void ExecuteSelectedCommandAsync(object _)
    {
        await ExecuteSelectedCommandCoreAsync();
    }

    /// <summary>
    /// Initializes the list of Active Directory commands asynchronously.
    /// </summary>
    private async Task InitializeActiveDirectoryCommandsAsync()
    {
        ObservableCollection<Command> list = await ADCommandsFetcher.GetADCommands();
        ADCommands = new ObservableCollection<Command>(list);
        OnPropertyChanged(nameof(ADCommands));
    }

    /// <summary>
    /// Loads the parameters for the selected PowerShell command asynchronously.
    /// </summary>
    /// <param name="selectedCommand">The PowerShell command whose parameters are to be loaded.</param>
    private async Task LoadCommandParametersAsync(Command? selectedCommand)
    {
        ADCommandParameters adCommandParameters = new();
        await adCommandParameters.LoadAvailableParametersAsync(selectedCommand);
        AvailableADCommandParameters = new ObservableCollection<string>(adCommandParameters.AvailableParameters);
        OnPropertyChanged(nameof(AvailableADCommandParameters));

        // Update the possible properties of the ComboBoxParameterViewModels.
        foreach (ComboBoxParameterViewModel comboBoxParameterViewModel in DynamicParameterComboBoxes)
        {
            comboBoxParameterViewModel.AvailableParameters = AvailableADCommandParameters;
        }
    }

    /// <summary>
    /// Executes the currently selected PowerShell command and updates the QueryBuilderConsoleOutput property with the result.
    /// </summary>
    /// <param name="command">The PowerShell command to execute.</param>
    /// <returns>A Task representing the asynchronous operation of executing the command.</returns>
    private async Task ExecuteSelectedCommandCoreAsync(Command? command = null)
    {
        if (SelectedComboBoxCommandInQueryBuilder is null && command is null)
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
            PSOutput result;
            if (command is not null)
            {
                result = await _psExecutor.ExecuteAsync(command);
            }
            else
            {
                // Add selected parameters and their values to the command.
                UpdateSelectedCommand();
                result = await _psExecutor.ExecuteAsync(SelectedComboBoxCommandInQueryBuilder);
            }

            if (result.HadErrors)
            {
                QueryBuilderConsoleOutput.Append(result.StdErr);
                return;
            }

            QueryBuilderConsoleOutput.Append(result.StdOut);
        }
        catch (Exception ex)
        {
            QueryBuilderConsoleOutput.Append($"Error executing command: {ex.Message}" + ex.Message);
        }
    }

    /// <summary>
    /// Adds a new parameter and value selection ComboBox to the DynamicParameterComboBoxes.
    /// </summary>
    /// <param name="_">This is the object that the command is bound to</param>
    private void AddParameterComboBox(object _)
    {
        // Check if some variable is null and throw an exception if it is
        if (AvailableADCommandParameters is null)
        {
            Trace.WriteLine("AvailableADCommandParameters has not been populated yet.");
            MessageBox.Show("To add a parameter, you must first select a command.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
            return;
        }

        DynamicParameterComboBoxes.Add(new ComboBoxParameterViewModel(AvailableADCommandParameters));
        DynamicParameterValueTextBoxes.Add(new TextBoxViewModel());
    }

    /// <summary>
    /// Adds a new command selection ComboBox to the UI.
    /// </summary>
    /// <param name="_">This is the object that the command is bound to.</param>
    private void AddCommandComboBox(object _)
    {
        Trace.WriteLine("Not implemented yet.");
        MessageBox.Show("Not implemented yet.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// This method executes the currently selected command and saves the output to a text file.
    /// </summary>
    /// <param name="_">Represents the object that the command is bound to.</param>
    private async void OutputToTextFileAsync(object _)
    {
        await ExecuteSelectedCommandCoreAsync();

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
            await File.WriteAllTextAsync(filePath, QueryBuilderConsoleOutput.ConsoleOutput);
        }
    }

    /// <summary>
    /// This method is in the working
    /// Status: prompts user for file path and saves correctly though the string could be edited to be better
    /// </summary>
    /// <param name="_">Represents the object that the command is bound to</param>
    private async void OutputToCsvFileAsync(object _)
    {
        await ExecuteSelectedCommandCoreAsync();

        var csv = new StringBuilder();
        string[] output = QueryBuilderConsoleOutput.ConsoleOutput.Split(' ', '\n');

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
    /// This method is for getting the currently selected command at anytime.
    /// </summary>
    /// <param name="_">This is the object that the command is bound to.</param>
    private void ExportConsoleOutput(object _)
    {
        if (QueryBuilderConsoleOutput.ConsoleOutput.Length == 0)
        {
            MessageBox.Show("The console is empty.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SaveFileDialog saveFileDialog = new() { DefaultExt = ".txt", Filter = "Text documents (.txt)|*.txt" };

        bool? result = saveFileDialog.ShowDialog();

        if (result == true)
        {
            string filename = saveFileDialog.FileName;
            QueryBuilderConsoleOutput.ExportToText(filename);
        }
    }

    /// <summary>
    /// This method is for getting the currently selected command at anytime
    /// </summary>
    /// <note>
    /// TODO: !Still in the works!
    /// TODO: Does this method do the same thing an another method?
    /// </note>
    private void UpdateSelectedCommand()
    {
        if (SelectedComboBoxCommandInQueryBuilder is null)
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
            SelectedComboBoxCommandInQueryBuilder.Parameters.Clear();
            for (int i = 0; i < DynamicParameterComboBoxes.Count; i++)
            {
                SelectedComboBoxCommandInQueryBuilder.Parameters.Add(
                    new CommandParameter(DynamicParameterComboBoxes[i].SelectedParameter,
                                         DynamicParameterValueTextBoxes[i].SelectedParameterValue));
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex);
        }
    }

    /// <summary>
    /// Updates the _currentQuery object with the current query information.
    /// </summary>
    /// <note>
    /// TODO: It needs to be tested! Still in the works!
    /// TODO: Fix any and all warnings about possible null values.
    /// </note>
    private void GetCurrentQuery()
    {
        UpdateSelectedCommand();

        try
        {
            string[] commandParameters;
            string[] commandParameterValues;

            if (SelectedComboBoxCommandInQueryBuilder is not null && SelectedComboBoxCommandInQueryBuilder.Parameters is not null)
            {
                commandParameters = new string[SelectedComboBoxCommandInQueryBuilder.Parameters.Count];
                commandParameterValues = new string[SelectedComboBoxCommandInQueryBuilder.Parameters.Count];

                _currentQuery.QueryDescription = QueryDescription;
                _currentQuery.QueryName = QueryName;
                _currentQuery.PSCommandName = SelectedComboBoxCommandInQueryBuilder.CommandText;
                
                for (int i = 0; i < SelectedComboBoxCommandInQueryBuilder.Parameters.Count; i++)
                {
                    CommandParameter commandParameter = SelectedComboBoxCommandInQueryBuilder.Parameters[i];

                    commandParameters[i] = commandParameter.Name;
                    commandParameterValues[i] = commandParameter.Value.ToString()!;
                    i++;  // TODO: Remove this line!
                }
                _currentQuery.PSCommandParameters = commandParameters;
                _currentQuery.PSCommandParameterValues = commandParameterValues;
            }
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
        if (DynamicParameterComboBoxes.Count != 0)
        {
            DynamicParameterComboBoxes.RemoveAt(DynamicParameterComboBoxes.Count - 1);
            DynamicParameterValueTextBoxes.RemoveAt(DynamicParameterValueTextBoxes.Count - 1);
        }
        else
        {
            MessageBox.Show("There are no parameters to remove.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
        }
    }

    /// <summary>
    /// This method will serialize the command and add it to the list of buttons.
    /// </summary>
    /// <param name="commandParameter">This is not used but necessary for the relay command</param>
    private void SaveCustomQueries(object commandParameter)
    {
        if (SelectedComboBoxCommandInQueryBuilder is null)
        {
            Trace.WriteLine("No command selected.");
            MessageBox.Show("To save a query, you must first select a command.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
            return;
        }

        if (_isEditing is not null && EditingEnabled)
        {
            // CustomQueries.query editingQuery = _customQuery.Queries.Find(item => item == isEditing);
            GetCurrentQuery();
            Trace.WriteLine(_customQuery.Queries.IndexOf(_isEditing));
            _customQuery.Queries[_customQuery.Queries.IndexOf(_isEditing)] = _currentQuery;
            _customQuery.SaveQueriesToJson();
            _isEditing = null;
            EditingEnabled = false;
        }
        else
        {
            // Try to get the content within the drop downs
            try
            {
                UpdateSelectedCommand();

                _customQuery.SerializeCommand(SelectedComboBoxCommandInQueryBuilder, QueryName, QueryDescription);

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
    /// This method will clear the query builder and reset the fields to their default values
    /// </summary>
    /// <param name="_">Object that the command is tied to</param>
    private void ClearQueryBuilder(object _)
    {
        if (SelectedComboBoxCommandInQueryBuilder is null && DynamicParameterComboBoxes.Count == 0)
        {
            MessageBox.Show("The query builder is already clear.",
                            "Information",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
            return;
        }

        // Display a gui box confirming if the user wants to confirm the clear
        MessageBoxResult result = MessageBox.Show("Are you sure you want to clear the query builder?",
                                                  "Warning",
                                                  MessageBoxButton.YesNo,
                                                  MessageBoxImage.Warning,
                                                  MessageBoxResult.No);

        // If the user selects yes, clear the console
        if (result == MessageBoxResult.Yes)
        {
            SelectedComboBoxCommandInQueryBuilder = null;
            DynamicParameterComboBoxes.Clear();
            DynamicParameterValueTextBoxes.Clear();
        }
    }

    /// <summary>
    /// This method is for creating buttons, right now it creates it off of the current/selectedcommand parameters but
    /// could be changed to also do it from the query list.
    /// </summary>
    /// <returns>This method returns a button that has been customized for the custom query list</returns>
    /// TODO: Change method name?
    private Button CreateCustomButton(Query? query = null)
    {
        Button newButton = new();

        if (query != null)
        {
            newButton.Height = 48;
            newButton.Content = (string.IsNullOrEmpty(query.QueryName) ? query.PSCommandName : query.QueryName);
            newButton.Tag = query;
        }
        else
        {
            // Check for null
            if (SelectedComboBoxCommandInQueryBuilder is null)
            {
                Trace.WriteLine("No command selected.");
                MessageBox.Show("To save a query, you must first select a command.",
                                "Warning",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return null; // TODO: Figure out what to return here!!!
            }

            GetCurrentQuery();
            newButton.Height = 48;
            newButton.Content = QueryName.Length != 0 ? QueryName : SelectedComboBoxCommandInQueryBuilder.CommandText;
            newButton.Tag = _currentQuery;
        }

        // Want to add right click context menu to each button
        ContextMenu contextMenu = new();

        MenuItem menuItem1 =
            new() { Header = "Execute", Command = ExecuteCustomQueryCommandButtonRelay, CommandParameter = newButton };

        MenuItem outputToCsv = new() { Header = "Output to CSV", Command = OutputToCsvFileRelay };
        MenuItem outputToText = new() { Header = "Output to Text", Command = OutputToTextFileRelay };
        MenuItem outputToConsole = new() { Header = "Execute to Console", Command = ExecuteSelectedCommandRelay };

        menuItem1.Items.Add(outputToCsv);
        menuItem1.Items.Add(outputToText);
        menuItem1.Items.Add(outputToConsole);

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
    
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
