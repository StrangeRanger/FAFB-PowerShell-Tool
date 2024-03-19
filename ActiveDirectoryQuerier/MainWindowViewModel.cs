using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ActiveDirectoryQuerier.ActiveDirectory;
using ActiveDirectoryQuerier.PowerShell;
using ActiveDirectoryQuerier.Queries;
using ActiveDirectoryQuerier.ViewModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Win32;

namespace ActiveDirectoryQuerier;

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
    private string? _selectedCommandFromComboBoxInActiveDirectoryInfo;
    private ObservableCollection<Button>? _buttons; // TODO: Rename to be more descriptive.

    // [[ Other fields ]] ----------------------------------------------------------- //

    private readonly Query _currentQuery;
    private readonly QueryStackPanel _queryStackPanel;
    private readonly PSExecutor _psExecutor;
    private Query? _isEditing;
    private readonly ActiveDirectoryInfo _activeDirectoryInfo = new();

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

    // TODO: Pieter: Use this for the console output in the Active Directory Info tab (link in GUI)
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
                // TODO: Figure out how to resolve the warning about the async method not being awaited...
                LoadCommandParametersAsync(value);
            }
        }
    }

    public string? SelectedCommandFromComboBoxInActiveDirectoryInfo
    {
        get => _selectedCommandFromComboBoxInActiveDirectoryInfo;
        set {
            if (_selectedCommandFromComboBoxInActiveDirectoryInfo != value)
            {
                _selectedCommandFromComboBoxInActiveDirectoryInfo = value;
                OnPropertyChanged(nameof(SelectedCommandFromComboBoxInActiveDirectoryInfo));
            }
        }
    }

    public ActiveDirectoryInfo AvailableOptionsFromComboBoxInActiveDirectoryInfo { get; } = new();

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
    public ICommand ExecuteQueryAsyncFromActiveDirectoryInfoRelay { get; }
    public ICommand AddCommandComboBoxRelay { get; }
    public ICommand AddCommandParameterComboBoxRelay { get; }
    public ICommand RemoveCommandParameterComboBoxRelay { get; }
    public ICommand OutputToTextFileRelay { get; }
    public ICommand OutputToCsvFileRelay { get; }
    public ICommand ExportConsoleOutputRelay { get; }
    public ICommand ClearConsoleOutputInQueryBuilderRelay { get; }
    public ICommand ImportQueryFileRelay { get; }
    public ICommand CreateNewQueryFileRelay { get; }
    public ICommand ClearConsoleOutputInActiveDirectoryInfoRelay { get; } 
    
    //  [ Constructor ] ------------------------------------------------------------- //

    public MainWindowViewModel()
    {
        _queryName = string.Empty;
        _queryDescription = string.Empty;
        _consoleOutputInQueryBuilder = new AppConsole();
        _consoleOutputInActiveDirectoryInfo = new AppConsole();
        _psExecutor = new PSExecutor();
        _queryStackPanel = new QueryStackPanel();
        _currentQuery = new Query();

        ADCommands = new ObservableCollection<Command>();
        DynamicallyAvailableADCommandParametersComboBox = new ObservableCollection<ComboBoxParameterViewModel>();
        DynamicallyAvailableADCommandParameterValueTextBox = new ObservableCollection<TextBoxViewModel>();

        OutputToCsvFileRelay = new RelayCommand(OutputExecutionResultsToCsvFileAsync);
        OutputToTextFileRelay = new RelayCommand(OutputExecutionResultsToTextFileAsync);
        ExportConsoleOutputRelay = new RelayCommand(ExportConsoleOutputToFile);
        ExecuteQueryFromQueryBuilderRelay = new RelayCommand(
            _ => ExecuteQueryAsync(_consoleOutputInQueryBuilder));
        ExecuteQueryAsyncFromActiveDirectoryInfoRelay =
            new RelayCommand(ExecuteQueryAsyncFromComboBoxInActiveDirectoryInfo);
        ImportQueryFileRelay = new RelayCommand(ImportQueryFile);
        CreateNewQueryFileRelay = new RelayCommand(CreateNewQueryFile);
        AddCommandParameterComboBoxRelay = new RelayCommand(AddParameterComboBoxInQueryBuilder);
        AddCommandComboBoxRelay = new RelayCommand(AddCommandComboBoxInQueryBuilder);
        RemoveCommandParameterComboBoxRelay = new RelayCommand(RemoveCommandParameterComboBoxInQueryBuilder);
        SaveQueryRelay = new RelayCommand(SaveQuery);
        EditQueryFromQueryStackPanelRelay = new RelayCommand(EditQueryFromQueryStackPanel);
        DeleteQueryFromQueryStackPanelRelay = new RelayCommand(DeleteQueryFromQueryStackPanel);
        ExecuteQueryFromQueryStackPanelRelay = new RelayCommand(ExecuteQueryFromQueryStackPanel);
        ClearConsoleOutputInQueryBuilderRelay = new RelayCommand(
            _ => ClearConsoleOutput(_consoleOutputInQueryBuilder));
        ClearConsoleOutputInActiveDirectoryInfoRelay = new RelayCommand(
            _ => ClearConsoleOutput(_consoleOutputInActiveDirectoryInfo));
        ClearQueryBuilderRelay = new RelayCommand(ClearQueryBuilder);
        
        InitializeActiveDirectoryCommandsAsync();
        LoadSavedQueriesFromFile(); // Calls method to deserialize and load buttons.
    }

    // [ Methods ] ----------------------------------------------------------------- //

    private async void ExecuteQueryAsyncFromComboBoxInActiveDirectoryInfo(object _)
    {
        if (SelectedCommandFromComboBoxInActiveDirectoryInfo is null)
        {
            Trace.WriteLine("No command selected.");
            MessageBox.Show("You must first select an option to execute.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
            return;
        }

        string selectedOption = SelectedCommandFromComboBoxInActiveDirectoryInfo;
        if (_activeDirectoryInfo.AvailableOptions.TryGetValue(selectedOption, out var method))
        {
            PSOutput result = await method.Invoke();

            if (result.HadErrors)
            {
                ConsoleOutputInActiveDirectoryInfo.Append(result.StdErr);
            }
            else
            {
                ConsoleOutputInActiveDirectoryInfo.Append(result.StdOut);
            }
        }
        // This is more of an internal error catch, as even through this command shouldn't fail, it's better safe than
        // sorry.
        else
        {
            string errorMessage =
                "Internal Error: The selected option was not found in the dictionary: " + $"{selectedOption}";
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw new KeyNotFoundException("The selected option was not found in the dictionary.");
        }
    }

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
        var currentQuery = (Query)((Button)queryButton).Tag;

        _isEditing = currentQuery;
        QueryEditingEnabled = true;
        QueryName = currentQuery.QueryName;
        QueryDescription = currentQuery.QueryDescription;

        // Fill in the commandName
        Command chosenCommand = ADCommands.FirstOrDefault(item => item.CommandText == currentQuery.PSCommandName)!;
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
        for (var i = 0; i < currentQuery.PSCommandParameters.Length; i++)
        {
            Trace.WriteLine(currentQuery.PSCommandParameters[i]);

            // Adds the Parameters boxes
            AddParameterComboBoxInQueryBuilder(null!); // Null is never used, so we use null forgiveness operator.

            // Fill in the parameter boxes
            DynamicallyAvailableADCommandParametersComboBox[i].SelectedParameter = currentQuery.PSCommandParameters[i];
            DynamicallyAvailableADCommandParameterValueTextBox[i].SelectedParameterValue =
                currentQuery.PSCommandParameterValues[i];
        }
    }

    private async void ExecuteQueryFromQueryStackPanel(object queryButton)
    {
        var currentButton = queryButton as Button;
        
        if (currentButton is null)
        {
            Trace.WriteLine("No button selected.");
            MessageBox.Show("To execute a query, you must first select a query.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
            return;
        }
        

        var buttonQuery = (Query)currentButton.Tag;
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

        MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the query?",
                                                  "Warning",
                                                  MessageBoxButton.YesNo,
                                                  MessageBoxImage.Warning,
                                                  MessageBoxResult.No);

        if (result == MessageBoxResult.Yes)
        {
            QueryButtonStackPanel.Remove(currentButton);
            _queryStackPanel.Queries.Remove((Query)currentButton.Tag);
            _queryStackPanel.SaveQueriesToJson();
        }
    }

    private void CreateNewQueryFile(object _)
    {
        // Saves/creates a new save file for the queries
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
        if (saveFileDialog.ShowDialog() == true)
        {
            QueryButtonStackPanel.Clear();
            //File.WriteAllText(saveFileDialog.FileName, string.Empty);
            _queryStackPanel.CustomQueryFileLocation = saveFileDialog.FileName;
        }
    }

    private void LoadSavedQueriesFromFile()
    {
        try
        {
            // Load the custom queries from the file (Deserialize)
            _queryStackPanel.LoadData();

            // Loop through the queries and create a button for each one.
            foreach (var newButton in _queryStackPanel.Queries.Select(CreateQueryButtonInStackPanel))
            {
                // Lastly add the button to the stack panel
                QueryButtonStackPanel.Add(newButton);
            }
        }
        // TODO: Possibly provide more comprehensive error handling.
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    private void ImportQueryFile(object _)
    {
        OpenFileDialog dialog =
            new() { FileName = "CustomQueries.dat", Filter = "Json files (*.json)|*.json|Text Files (*.txt)|*.txt" };

        // Display
        bool? result = dialog.ShowDialog();

        // Get file and write text
        if (result == true)
        {
            // Open document
            _queryStackPanel.CustomQueryFileLocation = dialog.FileName;

            QueryButtonStackPanel.Clear();
            LoadSavedQueriesFromFile();
        }
    }

// It's okay to suppress this warning because this method is called within the constructor. There is more than enough
// time for the method to complete before the user interacts with the GUI.
#pragma warning disable S3168
    private async void ExecuteQueryAsync(AppConsole appConsole, Command? command = null)
    {
        await ExecuteQueryCoreAsync(appConsole, command);
    }

    private async void InitializeActiveDirectoryCommandsAsync()
    {
        ObservableCollection<Command> list = await ADCommandsFetcher.GetADCommands();
        ADCommands = new ObservableCollection<Command>(list);
        OnPropertyChanged(nameof(ADCommands));
    }
#pragma warning restore S3168

    private async Task LoadCommandParametersAsync(Command? selectedCommand)
    {
        ADCommandParameters adCommandParameters = new();
        await adCommandParameters.LoadAvailableParametersAsync(selectedCommand);
        AvailableADCommandParameters = new ObservableCollection<string>(adCommandParameters.AvailableParameters);
        OnPropertyChanged(nameof(AvailableADCommandParameters));

        // Update the possible properties of the ComboBoxParameterViewModels.
        foreach (ComboBoxParameterViewModel comboBoxParameterViewModel in
                     DynamicallyAvailableADCommandParametersComboBox)
        {
            comboBoxParameterViewModel.AvailableParameters = AvailableADCommandParameters;
        }
    }

    // TODO: Hunter: Re-review this method and make any necessary changes.
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

    private void AddParameterComboBoxInQueryBuilder(object _)
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

        DynamicallyAvailableADCommandParametersComboBox.Add(
            new ComboBoxParameterViewModel(AvailableADCommandParameters));
        DynamicallyAvailableADCommandParameterValueTextBox.Add(new TextBoxViewModel());
    }

    private void AddCommandComboBoxInQueryBuilder(object _)
    {
        Trace.WriteLine("Not implemented yet.");
        MessageBox.Show("Not implemented yet.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private async void OutputExecutionResultsToTextFileAsync(object _)
    {

        if (_ is not null)
        {
            var currentButton = _ as Button;
            Query buttonQuery;

            buttonQuery = (Query)currentButton!.Tag;
            await ExecuteQueryCoreAsync(ConsoleOutputInQueryBuilder, buttonQuery.Command);

        }
        else
        {
            await ExecuteQueryCoreAsync(ConsoleOutputInQueryBuilder);
        }

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
    private async void OutputExecutionResultsToCsvFileAsync(object _)
    {

        if (_ is not null) 
        {
            var currentButton = _ as Button;
            Query buttonQuery;

            buttonQuery = (Query)currentButton!.Tag;
            await ExecuteQueryCoreAsync(ConsoleOutputInQueryBuilder, buttonQuery.Command);

        }
        else
        {
            await ExecuteQueryCoreAsync(ConsoleOutputInQueryBuilder);
        }

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

    private void ExportConsoleOutputToFile(object _)
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

            if (SelectedCommandFromComboBoxInQueryBuilder?.Parameters != null)
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
        // TODO: Possibly provide more comprehensive error handling.
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void RemoveCommandParameterComboBoxInQueryBuilder(object _)
    {
        if (DynamicallyAvailableADCommandParametersComboBox.Count != 0)
        {
            DynamicallyAvailableADCommandParametersComboBox.RemoveAt(
                DynamicallyAvailableADCommandParametersComboBox.Count - 1);
            DynamicallyAvailableADCommandParameterValueTextBox.RemoveAt(
                DynamicallyAvailableADCommandParameterValueTextBox.Count - 1);
        }
        else
        {
            MessageBox.Show("There are no parameters to remove.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
        }
    }

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
            Trace.WriteLine(_queryStackPanel.Queries.IndexOf(_isEditing));
            _queryStackPanel.Queries[_queryStackPanel.Queries.IndexOf(_isEditing)] = _currentQuery;
            _queryStackPanel.SaveQueriesToJson();
            _isEditing = null;
            QueryEditingEnabled = false;
        }
        else
        {
            // Try to get the content within the drop downs
            try
            {
                UpdateSelectedCommand();

                _queryStackPanel.SerializeCommand(SelectedCommandFromComboBoxInQueryBuilder, QueryName, QueryDescription);

                Button newButton = CreateQueryButtonInStackPanel();

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
        if (SelectedCommandFromComboBoxInQueryBuilder is null &&
            DynamicallyAvailableADCommandParametersComboBox.Count == 0)
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
            QueryName = "";
            QueryDescription = "";
            SelectedCommandFromComboBoxInQueryBuilder = null;
            DynamicallyAvailableADCommandParametersComboBox.Clear();
            DynamicallyAvailableADCommandParameterValueTextBox.Clear();
        }
    }

    private Button CreateQueryButtonInStackPanel(Query? query = null)
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
            newButton.Content =
                QueryName.Length != 0 ? QueryName : SelectedCommandFromComboBoxInQueryBuilder.CommandText;
            newButton.Tag = _currentQuery;
        }

        // Want to add right click context menu to each button
        ContextMenu contextMenu = new();

        MenuItem menuItem1 =
            new() { Header = "Execute", Command = ExecuteQueryFromQueryStackPanelRelay, CommandParameter = newButton };

        MenuItem outputToCsv = new() { Header = "Output to CSV", Command = OutputToCsvFileRelay, CommandParameter = newButton };
        MenuItem outputToText = new() { Header = "Output to Text", Command = OutputToTextFileRelay, CommandParameter = newButton };
        MenuItem outputToConsole = new() { Header = "Execute to Console", Command = ExecuteQueryFromQueryStackPanelRelay, CommandParameter = newButton };

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

    // [[ Event Handlers ]] --------------------------------------------------------- //

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
