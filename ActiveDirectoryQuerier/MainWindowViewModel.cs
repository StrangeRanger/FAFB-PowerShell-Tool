using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ActiveDirectoryQuerier.ActiveDirectory;
using ActiveDirectoryQuerier.MessageBoxService;
using ActiveDirectoryQuerier.PowerShell;
using ActiveDirectoryQuerier.Queries;
using ActiveDirectoryQuerier.ViewModels;
using Microsoft.Win32;

namespace ActiveDirectoryQuerier;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    // [ Fields ] ------------------------------------------------------------------- //
    // [[ Event handler property ]] ------------------------------------------------- //

    public event PropertyChangedEventHandler? PropertyChanged;

    // [[ Backing fields for properties ]] ------------------------------------------ //

    private bool _isQueryEditingEnabled;
    private string _queryName;
    private string _queryDescription;
    private string? _selectedQueryInActiveDirectoryInfo;
    private Command? _selectedCommandInQueryBuilder;
    private ConsoleViewModel _consoleOutputInQueryBuilder;
    private ConsoleViewModel _consoleOutputInActiveDirectoryInfo;
    private ObservableCollection<Button>? _queryButtonStackPanel;

    // [[ Other fields ]] ----------------------------------------------------------- //

    private Query? _queryBeingEdited;
    private readonly Query _currentQuery;
    private readonly QueryManager _queryManager;
    private readonly ActiveDirectoryInfo _activeDirectoryInfo;

    // [ Properties ] --------------------------------------------------------------- //
    // [[ Properties for backing fields ]] ------------------------------------------ //

    public ObservableCollection<Button> QueryButtonStackPanel => _queryButtonStackPanel ??=
        new ObservableCollection<Button>();

    public ConsoleViewModel ConsoleOutputInQueryBuilder
    {
        get => _consoleOutputInQueryBuilder;
        set {
            if (_consoleOutputInQueryBuilder != value)
            {
                _consoleOutputInQueryBuilder = value;
                OnPropertyChanged(nameof(ConsoleOutputInQueryBuilder));
            }
        }
    }

    public ConsoleViewModel ConsoleOutputInActiveDirectoryInfo
    {
        get => _consoleOutputInActiveDirectoryInfo;
        set {
            if (_consoleOutputInActiveDirectoryInfo != value)
            {
                _consoleOutputInActiveDirectoryInfo = value;
                OnPropertyChanged(nameof(ConsoleOutputInActiveDirectoryInfo));
            }
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

    /// <summary>
    /// The currently selected powershell command from the ComboBox in the Query Builder tab.
    /// </summary>
    public Command? SelectedCommandInQueryBuilder
    {
        get => _selectedCommandInQueryBuilder;
        set {
            if (_selectedCommandInQueryBuilder != value)
            {
                _selectedCommandInQueryBuilder = value;
                OnPropertyChanged(nameof(SelectedCommandInQueryBuilder));
                // No need to load parameters if the command is null.
                if (value is not null)
                {
                    Task.Run(() => LoadCommandParametersAsync(value));
                }
            }
        }
    }

    /// <summary>
    /// The currently selected query from the ComboBox in the Active Directory Info tab.
    /// </summary>
    public string? SelectedQueryInActiveDirectoryInfo
    {
        get => _selectedQueryInActiveDirectoryInfo;
        set {
            if (_selectedQueryInActiveDirectoryInfo != value)
            {
                _selectedQueryInActiveDirectoryInfo = value;
                OnPropertyChanged(nameof(SelectedQueryInActiveDirectoryInfo));
            }
        }
    }

    public bool IsQueryEditingEnabled
    {
        get => _isQueryEditingEnabled;
        set {
            if (_isQueryEditingEnabled != value)
            {
                _isQueryEditingEnabled = value;
                OnPropertyChanged(nameof(IsQueryEditingEnabled));
                OnIsQueryEditingEnabledChanged();
            }
        }
    }

    /// <summary>
    /// The available queries that can be selected from the ComboBox in the ActiveDirectoryInfo tab.
    /// </summary>
    public ActiveDirectoryInfo AvailableQueriesInActiveDirectoryInfo { get; } = new();

    // [[ Other properties ]] ------------------------------------------------------- //

    // ReSharper disable once InconsistentNaming
    public ObservableCollection<Command> ADCommands { get; private set; }
    // ReSharper disable once InconsistentNaming
    public ObservableCollection<string>? AvailableADCommandParameters { get; private set; }
    // ReSharper disable once InconsistentNaming
    public ObservableCollection<ComboBoxParameterViewModel> DynamicallyAvailableADCommandParameterComboBoxes { get; }
    // ReSharper disable once InconsistentNaming
    public ObservableCollection<TextBoxViewModel> DynamicallyAvailableADCommandParameterValueTextBoxes { get; }

    // [[ GUI element relays ]] ----------------------------------------------------- //
    // [[[ Dynamically created elements ]]] ----------------------------------------- //

    private ICommand EditQueryFromQueryStackPanelRelay { get; }
    private ICommand DeleteQueryFromQueryStackPanelRelay { get; }
    private ICommand ExecuteQueryFromQueryStackPanelRelay { get; }

    // [[[ Existing GUI elements ]]] ------------------------------------------------ //

    public ICommand SaveCurrentQueryRelay { get; }
    public ICommand ClearQueryBuilderRelay { get; }
    public ICommand ExecuteQueryInQueryBuilderRelay { get; }
    public ICommand ExecuteSelectedQueryInADInfoRelay { get; }
    public ICommand AddCommandComboBoxInQueryBuilderRelay { get; }
    public ICommand AddParameterComboBoxInQueryBuilderRelay { get; }
    public ICommand RemoveParameterComboBoxInQueryBuilderRelay { get; }
    public ICommand OutputExecutionResultsToTextFileRelay { get; }
    public ICommand OutputExecutionResultsToCsvFileRelay { get; }
    public ICommand ExportConsoleOutputToFileRelay { get; }
    public ICommand ClearConsoleOutputInQueryBuilderRelay { get; }
    public ICommand ImportQueryFileRelay { get; }
    public ICommand CreateNewQueryFileRelay { get; }
    public ICommand CheckBoxCheckedRelay { get; private set; }
    public IMessageBoxService MessageBoxService { get; init; }

    //  [ Constructor ] ------------------------------------------------------------- //

    public MainWindowViewModel()
    {
        _queryName = string.Empty;
        _queryDescription = string.Empty;
        _currentQuery = new Query();
        _queryManager = new QueryManager();
        _activeDirectoryInfo = new ActiveDirectoryInfo();
        _consoleOutputInQueryBuilder = new ConsoleViewModel();
        _consoleOutputInActiveDirectoryInfo = new ConsoleViewModel();

        ADCommands = new ObservableCollection<Command>();
        DynamicallyAvailableADCommandParameterComboBoxes = new ObservableCollection<ComboBoxParameterViewModel>();
        DynamicallyAvailableADCommandParameterValueTextBoxes = new ObservableCollection<TextBoxViewModel>();
        MessageBoxService = new MessageBoxService.MessageBoxService();

        OutputExecutionResultsToCsvFileRelay = new RelayCommand(OutputExecutionResultsToCsvFileAsync);
        OutputExecutionResultsToTextFileRelay = new RelayCommand(OutputExecutionResultsToTextFileAsync);
        ExecuteQueryFromQueryStackPanelRelay = new RelayCommand(ExecuteQueryFromQueryStackPanelAsync);
        ExecuteQueryInQueryBuilderRelay = new RelayCommand(
            _ => Task.Run(() => ExecuteQueryAsync(_consoleOutputInQueryBuilder)));
        ExecuteSelectedQueryInADInfoRelay = new RelayCommand(ExecuteSelectedQueryInADInfo);
        ExportConsoleOutputToFileRelay = new RelayCommand(ExportConsoleOutputToFile);
        ImportQueryFileRelay = new RelayCommand(ImportQueryFile);
        CreateNewQueryFileRelay = new RelayCommand(CreateNewQueryFile);
        AddParameterComboBoxInQueryBuilderRelay = new RelayCommand(AddParameterComboBoxInQueryBuilder);
        AddCommandComboBoxInQueryBuilderRelay = new RelayCommand(AddCommandComboBoxInQueryBuilder);
        RemoveParameterComboBoxInQueryBuilderRelay = new RelayCommand(RemoveParameterComboBoxInQueryBuilder);
        SaveCurrentQueryRelay = new RelayCommand(SaveCurrentQuery);
        EditQueryFromQueryStackPanelRelay = new RelayCommand(EditQueryFromQueryStackPanel);
        DeleteQueryFromQueryStackPanelRelay = new RelayCommand(DeleteQueryFromQueryStackPanel);
        ClearConsoleOutputInQueryBuilderRelay = new RelayCommand(
            _ => ClearConsoleOutput(_consoleOutputInQueryBuilder));
        ClearQueryBuilderRelay = new RelayCommand(ClearQueryBuilder);
        CheckBoxCheckedRelay = new RelayCommand(CheckBoxChecked);

        Task.Run(InitializeActiveDirectoryCommandsAsync);
        LoadSavedQueriesFromFile(); // Calls method to deserialize and load buttons.
    }

    // [ Methods ] ----------------------------------------------------------------- //

    internal void ClearConsoleOutput(ConsoleViewModel consoleOutput)
    {
        if (consoleOutput.GetConsoleOutput.Length == 0)
        {
            MessageBoxService.Show("The console is already clear.", "Information", MessageBoxButton.OK,
                                   MessageBoxImage.Information);
            return;
        }

        MessageBoxResult result =
            MessageBoxService.Show("Are you sure you want to clear the console output?", "Warning",
                                   MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

        if (result == MessageBoxResult.Yes)
        {
            consoleOutput.Clear();
        }
    }

    private async void ExecuteSelectedQueryInADInfo(object? _)
    {
        if (SelectedQueryInActiveDirectoryInfo is null)
        {
            Trace.WriteLine("No command selected.");
            MessageBoxService.Show("You must first select an option to execute.", "Warning", MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
            return;
        }

        if (_activeDirectoryInfo.AvailableOptions.TryGetValue(SelectedQueryInActiveDirectoryInfo, out var method))
        {
            PSOutput result = await method.Invoke();
            ConsoleOutputInActiveDirectoryInfo.Append(result.HadErrors ? result.StdErr : result.StdOut);
        }
        // This is an internal error to ensure that if the selected option is not found, the program will not continue.
        else
        {
            var errorMessage = "Internal Error: The selected option was not found in the dictionary: " +
                               $"{SelectedQueryInActiveDirectoryInfo}";
            MessageBoxService.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw new KeyNotFoundException("The selected option was not found in the dictionary.");
        }
    }

    internal void EditQueryFromQueryStackPanel(object? queryButton)
    {
        if (queryButton is not Button button)
        {
            Trace.WriteLine("No button selected.");
            MessageBoxService.Show($"An internal error occurred while trying to edit the query: {queryButton}", "Error",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var currentQuery = (Query)button.Tag;
        Command chosenCommand = ADCommands.FirstOrDefault(item => item.CommandText == currentQuery.PSCommandName)!;
        ADCommandParameters adCommandParameters = new();

        _queryBeingEdited = currentQuery;
        IsQueryEditingEnabled = true;
        QueryName = currentQuery.QueryName;
        QueryDescription = currentQuery.QueryDescription;
        // Fill in the commandName
        SelectedCommandInQueryBuilder = chosenCommand;

        // Load the Possible Parameters Synchronously
        adCommandParameters.LoadAvailableParameters(SelectedCommandInQueryBuilder);
        AvailableADCommandParameters = new ObservableCollection<string>(adCommandParameters.AvailableParameters);
        OnPropertyChanged(nameof(AvailableADCommandParameters));

        // Check to see if the DynamicallyAvailableADCommandParameterComboBoxes is empty and clear it if it is
        if (DynamicallyAvailableADCommandParameterComboBoxes.Count != 0)
        {
            DynamicallyAvailableADCommandParameterComboBoxes.Clear();
            DynamicallyAvailableADCommandParameterValueTextBoxes.Clear();
        }

        // Fill in Parameters and values
        for (var i = 0; i < currentQuery.PSCommandParameters.Length; i++)
        {
            Trace.WriteLine(currentQuery.PSCommandParameters[i]);

            // Adds the Parameters boxes
            AddParameterComboBoxInQueryBuilder(null!); // Null is never used, so we use null forgiveness operator.

            // Fill in the parameter boxes
            DynamicallyAvailableADCommandParameterComboBoxes[i].SelectedParameter = currentQuery.PSCommandParameters[i];
            DynamicallyAvailableADCommandParameterValueTextBoxes[i].SelectedParameterValue =
                currentQuery.PSCommandParameterValues[i];
        }
    }

    internal void DeleteQueryFromQueryStackPanel(object? queryButton)
    {
        if (queryButton is not Button currentButton)
        {
            Trace.WriteLine("No button selected.");
            MessageBoxService.Show("To delete a query, you must first select a query.", "Warning", MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
            return;
        }

        MessageBoxResult result =
            MessageBoxService.Show("Are you sure you want to delete the query?", "Warning", MessageBoxButton.YesNo,
                                   MessageBoxImage.Warning, MessageBoxResult.No);

        if (result == MessageBoxResult.Yes)
        {
            Query deletedQuery = (Query)currentButton.Tag;
            int deletedQueryIndex = _queryManager.Queries.IndexOf(deletedQuery);

            QueryButtonStackPanel.Remove(currentButton);
            _queryManager.Queries.Remove((Query)currentButton.Tag);
            _queryManager.SaveQueryToFile();

            // Check if the deleted query is currently being edited
            if (IsQueryEditingEnabled && _queryBeingEdited == (Query)currentButton.Tag)
            {
                IsQueryEditingEnabled = false;
                _queryBeingEdited = null;
            }
            // If a different query was deleted, update the index of the editing query
            else if (IsQueryEditingEnabled && _queryBeingEdited != null)
            {
                int editingQueryIndex = _queryManager.Queries.IndexOf(_queryBeingEdited);
                if (editingQueryIndex > deletedQueryIndex)
                {
                    _queryBeingEdited = _queryManager.Queries[editingQueryIndex - 1];
                }
            }
        }
    }

    // TODO: Continue refactoring starting here...
    private void CreateNewQueryFile(object? _)
    {
        // Saves/creates a new save file for the queries
        SaveFileDialog saveFileDialog = new() { Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt" };

        if (saveFileDialog.ShowDialog() == true)
        {
            QueryButtonStackPanel.Clear();
            _queryManager.QueryFileSaveLocation = saveFileDialog.FileName;
        }
    }

    private void LoadSavedQueriesFromFile()
    {
        try
        {
            // Load the custom queries from the file (Deserialize)
            _queryManager.LoadQueriesFromFile();

            // Loop through the queries and create a button for each one.
            foreach (var newButton in _queryManager.Queries.Select(CreateQueryButtonInStackPanel))
            {
                // Lastly add the button to the stack panel
                QueryButtonStackPanel.Add(newButton);
            }
        }
        catch (Exception exception)
        {
            Trace.WriteLine(exception);
        }
    }

    private void ImportQueryFile(object? _)
    {
        OpenFileDialog dialog =
            new() { FileName = "CustomQueries.dat", Filter = "Json files (*.json)|*.json|Text Files (*.txt)|*.txt" };

        // Display
        bool? result = dialog.ShowDialog();

        // Get file and write text
        if (result == true)
        {
            // Open document
            _queryManager.QueryFileSaveLocation = dialog.FileName;

            QueryButtonStackPanel.Clear();
            LoadSavedQueriesFromFile();
        }
    }

    private async Task InitializeActiveDirectoryCommandsAsync()
    {
        try
        {
            ObservableCollection<Command> list = await ADCommandsFetcher.GetADCommandsAsync();
            ADCommands = new ObservableCollection<Command>(list);
            OnPropertyChanged(nameof(ADCommands));
        }
        catch (Exception exception)
        {
            Trace.WriteLine(exception);
        }
    }

    private async Task LoadCommandParametersAsync(Command? selectedCommand)
    {
        try
        {
            ADCommandParameters adCommandParameters = new();
            await adCommandParameters.LoadAvailableParametersAsync(selectedCommand);
            AvailableADCommandParameters = new ObservableCollection<string>(adCommandParameters.AvailableParameters);
            OnPropertyChanged(nameof(AvailableADCommandParameters));

            // Update the possible properties of the ComboBoxParameterViewModels.
            foreach (ComboBoxParameterViewModel comboBoxParameterViewModel in
                         DynamicallyAvailableADCommandParameterComboBoxes)
            {
                comboBoxParameterViewModel.AvailableParameters = AvailableADCommandParameters;
            }
        }
        catch (Exception exception)
        {
            Trace.WriteLine(exception);
        }
    }

    internal void AddParameterComboBoxInQueryBuilder(object? _)
    {
        // Check if some variable is null and throw an exception if it is
        if (AvailableADCommandParameters is null)
        {
            Trace.WriteLine("AvailableADCommandParameters has not been populated yet.");
            MessageBoxService.Show("To add a parameter, you must first select a command.", "Warning",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DynamicallyAvailableADCommandParameterComboBoxes.Add(
            new ComboBoxParameterViewModel(AvailableADCommandParameters));
        DynamicallyAvailableADCommandParameterValueTextBoxes.Add(new TextBoxViewModel());
    }

    private void AddCommandComboBoxInQueryBuilder(object? _)
    {
        Trace.WriteLine("Not implemented yet.");
        MessageBoxService.Show("Not implemented yet.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void UpdateCommandWithSelectedParameters()
    {
        if (SelectedCommandInQueryBuilder is null)
        {
            Trace.WriteLine("No command selected.");
            MessageBoxService.Show("To save a query, you must first select a command.", "Warning", MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
            return;
        }

        SelectedCommandInQueryBuilder.Parameters.Clear();
        // Loop through the ComboBoxes and add the selected parameters to the command.
        for (var i = 0; i < DynamicallyAvailableADCommandParameterComboBoxes.Count; i++)
        {
            SelectedCommandInQueryBuilder.Parameters.Add(
                new CommandParameter(DynamicallyAvailableADCommandParameterComboBoxes[i].SelectedParameter,
                                     DynamicallyAvailableADCommandParameterValueTextBoxes[i].SelectedParameterValue));
        }
    }

    /// <summary>
    /// Updates the _currentQuery object with the current query information.
    /// </summary>
    private void GetCurrentQuery()
    {
        UpdateCommandWithSelectedParameters();

        if (SelectedCommandInQueryBuilder?.Parameters == null)
        {
            return;
        }

        var commandParameters = new string[SelectedCommandInQueryBuilder.Parameters.Count];
        var commandParameterValues = new string[SelectedCommandInQueryBuilder.Parameters.Count];

        _currentQuery.QueryDescription = QueryDescription;
        _currentQuery.QueryName = QueryName;
        _currentQuery.PSCommandName = SelectedCommandInQueryBuilder.CommandText;

        for (int i = 0; i < SelectedCommandInQueryBuilder.Parameters.Count; i++)
        {
            CommandParameter commandParameter = SelectedCommandInQueryBuilder.Parameters[i];
            commandParameters[i] = commandParameter.Name;
            commandParameterValues[i] = commandParameter.Value.ToString()!;
        }

        _currentQuery.PSCommandParameters = commandParameters;
        _currentQuery.PSCommandParameterValues = commandParameterValues;
    }

    internal void RemoveParameterComboBoxInQueryBuilder(object? _)
    {
        if (DynamicallyAvailableADCommandParameterComboBoxes.Count != 0)
        {
            DynamicallyAvailableADCommandParameterComboBoxes.RemoveAt(
                DynamicallyAvailableADCommandParameterComboBoxes.Count - 1);
            DynamicallyAvailableADCommandParameterValueTextBoxes.RemoveAt(
                DynamicallyAvailableADCommandParameterValueTextBoxes.Count - 1);
        }
        else
        {
            MessageBoxService.Show("There are no parameters to remove.", "Warning", MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
        }
    }

    internal void SaveCurrentQuery(object? commandParameter)
    {
        if (SelectedCommandInQueryBuilder is null)
        {
            Trace.WriteLine("No command selected.");
            MessageBoxService.Show("To save a query, you must first select a command.", "Warning", MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
            return;
        }

        // If the user is editing a query, update the query and save it.
        if (_queryBeingEdited is not null && IsQueryEditingEnabled)
        {
            GetCurrentQuery();
            _queryManager.Queries[_queryManager.Queries.IndexOf(_queryBeingEdited)] = _currentQuery;
            _queryManager.SaveQueryToFile();
            _queryBeingEdited = null;
            IsQueryEditingEnabled = false;
        }
        else
        {
            // Try to get the content within the drop downs
            try
            {
                UpdateCommandWithSelectedParameters();

                _queryManager.ConvertCommandToQueryAndSave(SelectedCommandInQueryBuilder, QueryName, QueryDescription);

                Button newButton = CreateQueryButtonInStackPanel();

                if (newButton.Content != null)
                {
                    QueryButtonStackPanel.Add(newButton);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }
    }

    internal void ClearQueryBuilder(object? _)
    {
        if (SelectedCommandInQueryBuilder is null && DynamicallyAvailableADCommandParameterComboBoxes.Count == 0)
        {
            MessageBoxService.Show("The query builder is already clear.", "Information", MessageBoxButton.OK,
                                   MessageBoxImage.Information);
            return;
        }

        // Display a gui box confirming if the user wants to confirm the clear
        MessageBoxResult result =
            MessageBoxService.Show("Are you sure you want to clear the query builder?", "Warning",
                                   MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

        // If the user selects yes, clear the consoleOutput
        if (result == MessageBoxResult.Yes)
        {
            QueryName = string.Empty;
            QueryDescription = string.Empty;
            SelectedCommandInQueryBuilder = null;
            DynamicallyAvailableADCommandParameterComboBoxes.Clear();
            DynamicallyAvailableADCommandParameterValueTextBoxes.Clear();
        }
    }

    /// <remarks>
    /// The use of the <c>command</c> parameter indicates that the method can be called from the Query Stack Panel.
    /// On the other hand, the absence of the <c>command</c> parameter and use of the
    /// <c>SelectedCommandInQueryBuilder</c> property indicates that the method can be called from the Query Builder.
    /// </remarks>
    private async Task ExecuteQueryAsync(ConsoleViewModel consoleOutput, Command? command = null)
    {
        PSExecutor psExecutor = new();

        if (SelectedCommandInQueryBuilder is null && command is null)
        {
            Trace.WriteLine("No command selected.");
            MessageBoxService.Show("To execute a command, you must first select a command.", "Warning",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            PSOutput result;
            OutputFormat outputFormat = CalculateOutputFormat();

            if (command is not null)
            {
                result = await psExecutor.ExecuteAsync(command, outputFormat);
            }
            else
            {
                UpdateCommandWithSelectedParameters();
                // Null forgiveness operator is used because if command is not null, this line will never be reached.
                // If it is null, that must mean that SelectedCommandInQueryBuilder is not null, else the return
                // statement above would have been executed.
                result = await psExecutor.ExecuteAsync(SelectedCommandInQueryBuilder!, outputFormat);
            }

            if (result.HadErrors)
            {
                consoleOutput.Append(result.StdErr);
                return;
            }

            consoleOutput.Append(result.StdOut);
        }
        catch (Exception exception)
        {
            consoleOutput.Append($"Error executing command: {exception.Message}");
        }
    }

    private async void ExecuteQueryFromQueryStackPanelAsync(object? queryButton)
    {
        if (queryButton is not Button currentButton)
        {
            Trace.WriteLine("No button selected.");
            MessageBoxService.Show("To execute a query, you must first select a query.", "Warning", MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
            return;
        }

        var buttonQuery = (Query)currentButton.Tag;
        await ExecuteQueryAsync(ConsoleOutputInQueryBuilder, buttonQuery.Command);
    }

    private void ExportConsoleOutputToFile(object? _)
    {
        if (ConsoleOutputInQueryBuilder.GetConsoleOutput.Length == 0)
        {
            MessageBoxService.Show("The console is empty.", "Information", MessageBoxButton.OK,
                                   MessageBoxImage.Information);
            return;
        }

        SaveFileDialog saveFileDialog = new() { DefaultExt = ".txt", Filter = "Text documents (.txt)|*.txt" };

        bool? result = saveFileDialog.ShowDialog();

        if (result == true)
        {
            string filename = saveFileDialog.FileName;
            ConsoleOutputInQueryBuilder.ExportToTextFile(filename);
        }
    }

    private async void OutputExecutionResultsToTextFileAsync(object? queryButton)
    {
        await OutputExecutionResultsToFileAsync(queryButton, ".txt", "Text documents (.txt)|*.txt");
    }

    private async void OutputExecutionResultsToCsvFileAsync(object? queryButton)
    {
        await OutputExecutionResultsToFileAsync(queryButton, ".csv", "CSV files (*.csv)|*.csv");
    }

    private async Task OutputExecutionResultsToFileAsync(object? queryButton, string fileExtension, string filter)
    {
        if (queryButton is not null)
        {
            var buttonQuery = (Query)((Button)queryButton).Tag;
            await ExecuteQueryAsync(ConsoleOutputInQueryBuilder, buttonQuery.Command);
        }
        else
        {
            await ExecuteQueryAsync(ConsoleOutputInQueryBuilder);
        }

        SaveFileDialog saveFileDialog = new() { DefaultExt = fileExtension, Filter = filter };

        bool? result = saveFileDialog.ShowDialog();

        if (result == true)
        {
            string filePath = saveFileDialog.FileName;
            await File.WriteAllTextAsync(filePath, ConsoleOutputInQueryBuilder.GetConsoleOutput);
        }
    }

    private Button CreateQueryButtonInStackPanel(Query? query = null)
    {
        Button newButton = new();

        if (query is not null)
        {
            newButton.Height = 48;
            newButton.Content = string.IsNullOrEmpty(query.QueryName) ? query.PSCommandName : query.QueryName;
            newButton.Tag = query;
        }
        else
        {
            if (SelectedCommandInQueryBuilder is null)
            {
                Trace.WriteLine("No command selected.");
                MessageBoxService.Show("To save a query, you must first select a command.", "Warning",
                                       MessageBoxButton.OK, MessageBoxImage.Warning);
                return new Button();
            }

            GetCurrentQuery();
            newButton.Height = 48;
            newButton.Content = QueryName.Length != 0 ? QueryName : SelectedCommandInQueryBuilder.CommandText;
            newButton.Tag = _currentQuery;
        }

        // Want to add right click context menu to each button
        ContextMenu contextMenu = new();

        MenuItem menuItem1 =
            new() { Header = "Execute", Command = ExecuteQueryFromQueryStackPanelRelay, CommandParameter = newButton };

        MenuItem outputToCsv = new() { Header = "Output to CSV", Command = OutputExecutionResultsToCsvFileRelay,
                                       CommandParameter = newButton };
        MenuItem outputToText = new() { Header = "Output to Text", Command = OutputExecutionResultsToTextFileRelay,
                                        CommandParameter = newButton };
        MenuItem outputToConsole =
            new() { Header = "Execute to Console", Command = ExecuteQueryFromQueryStackPanelRelay,
                    CommandParameter = newButton };

        menuItem1.Items.Add(outputToCsv);
        menuItem1.Items.Add(outputToText);
        menuItem1.Items.Add(outputToConsole);

        MenuItem menuItem2 =
            new() { Header = "Edit", Command = EditQueryFromQueryStackPanelRelay, CommandParameter = newButton };

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

    private void OnIsQueryEditingEnabledChanged()
    {
        if (!IsQueryEditingEnabled)
        {
            _queryBeingEdited = null;
        }
    }

    private void CheckBoxChecked(object? _)
    {
        if (IsQueryEditingEnabled)
        {
            MessageBoxService.Show(
                "To edit a query, right click on a query button and select 'Edit' from the context menu.", "Warning",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            IsQueryEditingEnabled = false;
        }
    }

    private OutputFormat CalculateOutputFormat()
    {
        StackTrace stackTrace = new();
        StackFrame[] stackFrames = stackTrace.GetFrames();

        try
        {
            if (stackFrames.Length > 2)
            {
                // Null forgiveness operator is used because the method this statement would not be reached if the
                // length of the stackFrames array is less than 3.
                MethodBase callingMethod = stackFrames[2].GetMethod()!;
                if (callingMethod.Name == "OutputExecutionResultsToCsvFileAsync")
                {
                    return OutputFormat.Csv;
                }
            }

            return OutputFormat.Text;
        }
        catch (Exception)
        {
            return OutputFormat.Text;
        }
    }

    // [[ Event Handlers ]] --------------------------------------------------------- //

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
