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

/// TODO: Place class into ViewModel folder.
public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    // [ Fields ] ------------------------------------------------------------------- //
    // [[ Event handler property ]] ------------------------------------------------- //

    public event PropertyChangedEventHandler? PropertyChanged;

    // [[ Backing fields for properties ]] ------------------------------------------ //

    private bool _queryEditingEnabled;
    private string _queryName;
    private string _queryDescription;
    private AppConsole _consoleOutputInQueryBuilder;
    private AppConsole _consoleOutputInActiveDirectoryInfo;
    private Command? _selectedCommandFromComboBoxInQueryBuilder;
    private ObservableCollection<Button>? _buttons; // TODO: Rename to be more descriptive.

    // [[ Other fields ]] ----------------------------------------------------------- //

    private readonly Query _currentQuery;
    private readonly CustomQueries _customQuery;
    private readonly PSExecutor _psExecutor;
    private Query? _isEditing;

    // [ Properties ] --------------------------------------------------------------- //
    // [[ Properties for backing fields ]] ------------------------------------------ //
    
    public bool QueryEditingEnabled
    {
        get => _queryEditingEnabled;
        set {
            _queryEditingEnabled = value;
            OnPropertyChanged(nameof(QueryEditingEnabled));
        }
    }
    
    public AppConsole ConsoleOutputInQueryBuilder
    {
        get => _consoleOutputInQueryBuilder;
        set {
            _consoleOutputInQueryBuilder = value;
            OnPropertyChanged(nameof(ConsoleOutputInQueryBuilder));
        }
    }

    public AppConsole ConsoleOutputInActiveDirectoryInfo
    {
        get => _consoleOutputInActiveDirectoryInfo;
        set {
            _consoleOutputInActiveDirectoryInfo = value;
            OnPropertyChanged(nameof(ConsoleOutputInActiveDirectoryInfo));
        }
    }
    
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
    
    public Command? SelectedCommandFromComboBoxInQueryBuilder
    {
        get => _selectedCommandFromComboBoxInQueryBuilder;
        set {
            _selectedCommandFromComboBoxInQueryBuilder = value;
            OnPropertyChanged(nameof(SelectedCommandFromComboBoxInQueryBuilder));
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
    // ReSharper disable once InconsistentNaming
    public ObservableCollection<ComboBoxParameterViewModel> DynamicallyAvailableADCommandParametersComboBox { get; }
    // ReSharper disable once InconsistentNaming
    public ObservableCollection<TextBoxViewModel> DynamicallyAvailableADCommandParameterValueTextBox { get; }
    
    // [[ GUI element relays ]] ----------------------------------------------------- //
    // [[[ Dynamically created elements ]]] ----------------------------------------- //
    
    private ICommand EditQueryFromQueryStackPanelRelay { get; }
    private ICommand DeleteQueryFromQueryStackPanelRelay { get; }
    private ICommand ExecuteQueryFromQueryStackPanelRelay { get; }
    
    // [[[ Existing GUI elements ]]] ------------------------------------------------ //
    
    public ICommand SaveQueryRelay { get; }
    public ICommand ClearQueryBuilderRelay { get; }
    public ICommand ExecuteQueryFromQueryBuilderRelay { get; }
    public ICommand ExecuteQueryFromActiveDirectoryInfoRelay { get; } // TODO: Pieter use this for execution button
    public ICommand AddCommandComboBoxRelay { get; }
    public ICommand AddCommandParameterComboBoxRelay { get; }
    public ICommand RemoveCommandParameterComboBoxRelay { get; }
    public ICommand OutputToTextFileRelay { get; }
    public ICommand OutputToCsvFileRelay { get; }
    public ICommand ExportConsoleOutputRelay { get; }
    public ICommand ClearConsoleOutputInQueryBuilderRelay { get; }
    public ICommand ClearConsoleOutputInActiveDirectoryInfoRelay { get; } // TODO: Pieter use this for clear console button


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
        _consoleOutputInQueryBuilder = new AppConsole();
        _consoleOutputInActiveDirectoryInfo = new AppConsole();
        _psExecutor = new PSExecutor();
        _customQuery = new CustomQueries();
        _currentQuery = new Query();

        ADCommands = new ObservableCollection<Command>();
        DynamicallyAvailableADCommandParametersComboBox = new ObservableCollection<ComboBoxParameterViewModel>();
        DynamicallyAvailableADCommandParameterValueTextBox = new ObservableCollection<TextBoxViewModel>();
        
        OutputToCsvFileRelay = new RelayCommand(OutputToCsvFileAsync);
        OutputToTextFileRelay = new RelayCommand(OutputToTextFileAsync);
        ExportConsoleOutputRelay = new RelayCommand(ExportConsoleOutput);
        // TODO: Figure out how resolve the warning about the async method not being awaited.
        ExecuteQueryFromQueryBuilderRelay = new RelayCommand(_ => ExecuteQuery(_consoleOutputInQueryBuilder));
        // TODO: Figure out how resolve the warning about the async method not being awaited.
        ExecuteQueryFromActiveDirectoryInfoRelay = new RelayCommand(_ => ExecuteQuery(_consoleOutputInActiveDirectoryInfo));
        AddCommandParameterComboBoxRelay = new RelayCommand(AddParameterComboBox);
        AddCommandComboBoxRelay = new RelayCommand(AddCommandComboBox);
        RemoveCommandParameterComboBoxRelay = new RelayCommand(RemoveCommandParameterComboBox);
        SaveQueryRelay = new RelayCommand(SaveQuery);
        EditQueryFromQueryStackPanelRelay = new RelayCommand(EditQueryFromQueryStackPanel);
        DeleteQueryFromQueryStackPanelRelay = new RelayCommand(DeleteQueryFromQueryStackPanel);
        ExecuteQueryFromQueryStackPanelRelay = new RelayCommand(ExecuteQueryFromQueryStackPanel);
        ClearConsoleOutputInQueryBuilderRelay = new RelayCommand(_ => ClearConsoleOutput(_consoleOutputInQueryBuilder));
        ClearConsoleOutputInActiveDirectoryInfoRelay = new RelayCommand(_ => ClearConsoleOutput(_consoleOutputInActiveDirectoryInfo));
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
    
    private void ClearConsoleOutput(AppConsole appConsole)
    {
        if (appConsole.ConsoleOutput.Length == 0)
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
            appConsole.Clear();
        }
    }
    
    private void EditQueryFromQueryStackPanel(object queryButton)
    {
        Query currentQuery = (Query)((Button)queryButton).Tag;

        _isEditing = currentQuery;
        QueryEditingEnabled = true;
        QueryName = currentQuery.QueryName;
        QueryDescription = currentQuery.QueryDescription;

        // Fill in the commandName
        Command chosenCommand = 
            ADCommands.FirstOrDefault(item => item.CommandText == currentQuery.PSCommandName)!;
        SelectedCommandFromComboBoxInQueryBuilder = chosenCommand;

        // Load the Possible Parameters Synchronously
        ADCommandParameters adCommandParameters = new();
        adCommandParameters.LoadAvailableParameters(SelectedCommandFromComboBoxInQueryBuilder);
        AvailableADCommandParameters = new ObservableCollection<string>(adCommandParameters.AvailableParameters);
        OnPropertyChanged(nameof(AvailableADCommandParameters));

        // Check to see if the DynamicallyAvailableADCommandParametersComboBox is empty and clear it if it is
        if (DynamicallyAvailableADCommandParametersComboBox.Count != 0)
        {
            DynamicallyAvailableADCommandParametersComboBox.Clear();
            DynamicallyAvailableADCommandParameterValueTextBox.Clear();
        }

        // Fill in Parameters and values
        for (int i = 0; i < currentQuery.PSCommandParameters.Length; i++)
        {
            Trace.WriteLine(currentQuery.PSCommandParameters[i]);

            // Adds the Parameters boxes
            object temp = new();
            AddParameterComboBox(temp);

            // Fill in the parameter boxes
            DynamicallyAvailableADCommandParametersComboBox[i].SelectedParameter = currentQuery.PSCommandParameters[i];
            DynamicallyAvailableADCommandParameterValueTextBox[i].SelectedParameterValue = currentQuery.PSCommandParameterValues[i];
        }
    }
    
    private async void ExecuteQueryFromQueryStackPanel(object queryButton)
    {
        if (queryButton is not Button currentButton)
        {
            Trace.WriteLine("No button selected.");
            MessageBox.Show("To execute a query, you must first select a query.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
            return;
        }

        Query buttonQuery = (Query)currentButton.Tag;
        await ExecuteQueryCoreAsync(ConsoleOutputInQueryBuilder, buttonQuery.Command);
    }
    
    private void DeleteQueryFromQueryStackPanel(object queryButton)
    {
        if (queryButton is not Button currentButton)
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
    
    // TODO: Possibly change Task to void?
    private async Task ExecuteQuery(AppConsole appConsole, Command? command = null)
    {
        await ExecuteQueryCoreAsync(appConsole, command);
    }
    
    // TODO: Possibly change Task to void?
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
        foreach (ComboBoxParameterViewModel comboBoxParameterViewModel in DynamicallyAvailableADCommandParametersComboBox)
        {
            comboBoxParameterViewModel.AvailableParameters = AvailableADCommandParameters;
        }
    }
    
    private async Task ExecuteQueryCoreAsync(AppConsole appConsole, Command? command = null)
    {
        if (SelectedCommandFromComboBoxInQueryBuilder is null && command is null)
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
                result = await _psExecutor.ExecuteAsync(SelectedCommandFromComboBoxInQueryBuilder);
            }

            if (result.HadErrors)
            {
                appConsole.Append(result.StdErr);
                return;
            }

            appConsole.Append(result.StdOut);
        }
        catch (Exception ex)
        {
            appConsole.Append($"Error executing command: {ex.Message}" + ex.Message);
        }
    }

    /// <summary>
    /// Adds a new parameter and value selection ComboBox to the DynamicallyAvailableADCommandParametersComboBox.
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

        DynamicallyAvailableADCommandParametersComboBox.Add(new ComboBoxParameterViewModel(AvailableADCommandParameters));
        DynamicallyAvailableADCommandParameterValueTextBox.Add(new TextBoxViewModel());
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
        await ExecuteQueryCoreAsync(ConsoleOutputInQueryBuilder);

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
            await File.WriteAllTextAsync(filePath, ConsoleOutputInQueryBuilder.ConsoleOutput);
        }
    }

    /// <summary>
    /// This method is in the working
    /// Status: prompts user for file path and saves correctly though the string could be edited to be better
    /// </summary>
    /// <param name="_">Represents the object that the command is bound to</param>
    private async void OutputToCsvFileAsync(object _)
    {
        await ExecuteQueryCoreAsync(ConsoleOutputInQueryBuilder);

        var csv = new StringBuilder();
        string[] output = ConsoleOutputInQueryBuilder.ConsoleOutput.Split(' ', '\n');

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
        if (ConsoleOutputInQueryBuilder.ConsoleOutput.Length == 0)
        {
            MessageBox.Show("The console is empty.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SaveFileDialog saveFileDialog = new() { DefaultExt = ".txt", Filter = "Text documents (.txt)|*.txt" };

        bool? result = saveFileDialog.ShowDialog();

        if (result == true)
        {
            string filename = saveFileDialog.FileName;
            ConsoleOutputInQueryBuilder.ExportToText(filename);
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
        if (SelectedCommandFromComboBoxInQueryBuilder is null)
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
            SelectedCommandFromComboBoxInQueryBuilder.Parameters.Clear();
            for (int i = 0; i < DynamicallyAvailableADCommandParametersComboBox.Count; i++)
            {
                SelectedCommandFromComboBoxInQueryBuilder.Parameters.Add(
                    new CommandParameter(DynamicallyAvailableADCommandParametersComboBox[i].SelectedParameter,
                                         DynamicallyAvailableADCommandParameterValueTextBox[i].SelectedParameterValue));
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

            if (SelectedCommandFromComboBoxInQueryBuilder is not null && SelectedCommandFromComboBoxInQueryBuilder.Parameters is not null)
            {
                commandParameters = new string[SelectedCommandFromComboBoxInQueryBuilder.Parameters.Count];
                commandParameterValues = new string[SelectedCommandFromComboBoxInQueryBuilder.Parameters.Count];

                _currentQuery.QueryDescription = QueryDescription;
                _currentQuery.QueryName = QueryName;
                _currentQuery.PSCommandName = SelectedCommandFromComboBoxInQueryBuilder.CommandText;
                
                for (int i = 0; i < SelectedCommandFromComboBoxInQueryBuilder.Parameters.Count; i++)
                {
                    CommandParameter commandParameter = SelectedCommandFromComboBoxInQueryBuilder.Parameters[i];

                    commandParameters[i] = commandParameter.Name;
                    commandParameterValues[i] = commandParameter.Value.ToString()!;
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
    private void RemoveCommandParameterComboBox(object _)
    {
        if (DynamicallyAvailableADCommandParametersComboBox.Count != 0)
        {
            DynamicallyAvailableADCommandParametersComboBox.RemoveAt(DynamicallyAvailableADCommandParametersComboBox.Count - 1);
            DynamicallyAvailableADCommandParameterValueTextBox.RemoveAt(DynamicallyAvailableADCommandParameterValueTextBox.Count - 1);
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
    private void SaveQuery(object commandParameter)
    {
        if (SelectedCommandFromComboBoxInQueryBuilder is null)
        {
            Trace.WriteLine("No command selected.");
            MessageBox.Show("To save a query, you must first select a command.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
            return;
        }

        if (_isEditing is not null && QueryEditingEnabled)
        {
            // CustomQueries.query editingQuery = _customQuery.Queries.Find(item => item == isEditing);
            GetCurrentQuery();
            Trace.WriteLine(_customQuery.Queries.IndexOf(_isEditing));
            _customQuery.Queries[_customQuery.Queries.IndexOf(_isEditing)] = _currentQuery;
            _customQuery.SaveQueriesToJson();
            _isEditing = null;
            QueryEditingEnabled = false;
        }
        else
        {
            // Try to get the content within the drop downs
            try
            {
                UpdateSelectedCommand();

                _customQuery.SerializeCommand(SelectedCommandFromComboBoxInQueryBuilder, QueryName, QueryDescription);

                Button newButton = CreateCustomButton();

                QueryButtonStackPanel.Add(newButton);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }
    }

    private void ClearQueryBuilder(object _)
    {
        if (SelectedCommandFromComboBoxInQueryBuilder is null && DynamicallyAvailableADCommandParametersComboBox.Count == 0)
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
            SelectedCommandFromComboBoxInQueryBuilder = null;
            DynamicallyAvailableADCommandParametersComboBox.Clear();
            DynamicallyAvailableADCommandParameterValueTextBox.Clear();
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
            if (SelectedCommandFromComboBoxInQueryBuilder is null)
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
            newButton.Content = QueryName.Length != 0 ? QueryName : SelectedCommandFromComboBoxInQueryBuilder.CommandText;
            newButton.Tag = _currentQuery;
        }

        // Want to add right click context menu to each button
        ContextMenu contextMenu = new();

        MenuItem menuItem1 =
            new() { Header = "Execute", Command = ExecuteQueryFromQueryStackPanelRelay, CommandParameter = newButton };

        MenuItem outputToCsv = new() { Header = "Output to CSV", Command = OutputToCsvFileRelay };
        MenuItem outputToText = new() { Header = "Output to Text", Command = OutputToTextFileRelay };
        MenuItem outputToConsole = new() { Header = "Execute to Console", Command = ExecuteQueryFromQueryBuilderRelay };

        menuItem1.Items.Add(outputToCsv);
        menuItem1.Items.Add(outputToText);
        menuItem1.Items.Add(outputToConsole);

        MenuItem menuItem2 = new() { Header = "Edit", Command = EditQueryFromQueryStackPanelRelay, CommandParameter = newButton };

        MenuItem menuItem3 =
            new() { Header = "Delete", Command = DeleteQueryFromQueryStackPanelRelay, CommandParameter = newButton };

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
