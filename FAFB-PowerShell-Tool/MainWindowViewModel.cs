using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation.Runspaces;
using System.Windows.Controls;
using System.Windows.Input;
using FAFB_PowerShell_Tool.PowerShell;
using System.Management.Automation;
using System.IO;
using System.Globalization;
using System.Text;
using Microsoft.Win32;
using Microsoft.WSMan.Management;
using Newtonsoft.Json.Linq;

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
    private string _powerShellOutput;
    private ObservableCollection<Button> _buttons;
    private CustomQueries.query _currentQuery;
    private CustomQueries _customQuery = new();
    private string _queryName;
    private string _queryDescription;
    private string _parameterValue;
    // probably want to add the ability to toggle editing vs not editing but filled in
    private CustomQueries.query isEditing = null;

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
        get {
            return _queryName;
        }
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
    /// </summary>
    public string ParameterValue
    {
        get {
            return _parameterValue;
        }
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
        get {
            return _queryDescription;
        }
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
    public ObservableCollection<Button> ButtonStackPanel
    {
        get {
            if (_buttons == null)
            {
                _buttons = new ObservableCollection<Button>();
            }

            return _buttons;
        }
    }

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
    /// Command to add a new parameter ComboBox to the UI.
    /// TODO: Rename this property to match Property naming conventions!!!
    /// </summary>
    public ICommand Remove_ParameterComboBox { get; }

    /// <summary>
    /// Command to have the output send to a text file when executing
    /// TODO: Get-only auto-property 'OutputToText' is never assigned!!!
    /// </summary>
    public ICommand _OutputToText { get; }

    public ICommand DeleteCustomQuery { get; }

    /// <summary>
    /// Command to output to a csv when executing
    /// </summary>
    public ICommand OutputToCsv { get; }

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
    /// TODO: Fix warnings about possible null values.
    /// </summary>
    public MainWindowViewModel()
    {
        _powerShellExecutor = new PowerShellExecutor();
        ExecuteCommand = new RelayCommand(Execute);
        OutputToCsv = new RelayCommand(_OutputToCsvAsync);
        _OutputToText = new RelayCommand(OutputPowershellOutputToText);
        AddNewParameterComboBox = new RelayCommand(AddParameterComboBox);
        Remove_ParameterComboBox = new RelayCommand(RemoveParameterComboBox);
        SavedQueries = new RelayCommand(PerformSavedQueries);
        EditCustomQuery = new RelayCommand(PerformEditCustomQuery);
        DeleteCustomQuery = new RelayCommand(PerformDeleteCustomQuery);
        // Change the naming I know
        ExecuteCommandButton = new RelayCommand(ExecuteButtonCommand);

        _currentQuery = new CustomQueries.query();

        DynamicParameterCollection = new ObservableCollection<ComboBoxParameterViewModel>();
        DynamicParameterValuesCollection = new ObservableCollection<TextBoxViewModel>();

        InitializeCommandsAsync();
        LoadCustomQueries(); // Calls method to deserialize and load buttons.

        // Debug
        foreach (Button t in ButtonStackPanel)
        {
            CustomQueries.query test = (CustomQueries.query)t.Tag;
            Trace.WriteLine(test.commandName);
        }
    }

    // ----------------- Methods ----------------- //

    /// <summary>
    /// This method will edit the Query and fill out the field with the desired query and you can edit the query
    /// TODO: Add a description to the parameter of this method.
    /// </summary>
    /// <param name="sender"></param>
    private void PerformEditCustomQuery(object sender)
    {
        // Get the button that we are editing
        Button currButton = (Button)sender;
        CustomQueries.query currQuery = (CustomQueries.query)currButton.Tag;

        isEditing = currQuery;

        // Need to fill in the queryName
        QueryName = currQuery.queryName;
        // Fill in the queryDescription
        QueryDescription = currQuery.queryDescription;
        // Fill in the commandName
        Command chosenCommand =
            ActiveDirectoryCommandList.FirstOrDefault(item => item.CommandText == currQuery.commandName);
        SelectedCommand = chosenCommand;
        // Load the Possible Parameters Syncronously
        CommandParameters commandParameters = new CommandParameters();
        commandParameters.LoadCommandParameters(SelectedCommand);
        PossibleParameterList = new ObservableCollection<string>(commandParameters.PossibleParameters);
        OnPropertyChanged(nameof(PossibleParameterList));
        // Fill in Parameters and values

        for (int i = 0; i < currQuery.commandParameters.Count(); i++)
        {
            // Adds the Parameters boxes
            object temp = new object();
            AddParameterComboBox(temp);
            // Fill in the parameter boxes
            DynamicParameterCollection[i].SelectedParameter =
                PossibleParameterList.FirstOrDefault(currQuery.commandParameters[i]);
            DynamicParameterValuesCollection[i].SelectedParameterValue = currQuery.commandParametersValues[i];
        }
    }

    /// <summary>
    ///  TODO: Change the naming
    ///  This method executes the CustomQuery.query.command that is tied to the custom query buttons
    /// </summary>
    /// <param name="_">This is the Button as a generic object that is clicked when executing</param>
    public void ExecuteButtonCommand(object _)
    {
        var currButton = _ as Button;
        CustomQueries.query ButtonQuery = (CustomQueries.query)currButton.Tag;
        ExecuteGenericCommand(ButtonQuery.command);
    }

    /// <summary>
    /// This method deletes the button from the list and saves the changes to the file
    /// </summary>
    /// <param name="_">This is the Button as a generic object that is clicked when executing</param>
    public void PerformDeleteCustomQuery(object _)
    {
        // Delete a button from the custom queries list and from the file
        var currButton = _ as Button;

        ButtonStackPanel.Remove(currButton);
        _customQuery.Queries.Remove((CustomQueries.query)currButton.Tag);
        _customQuery.SerializeMethod();
    }

    /// <summary>
    /// This method will load the custom queries from the file.
    /// </summary>
    private void LoadCustomQueries()
    {
        try
        {
            _customQuery.LoadData();

            // parameter counter
            int i = 0;
            foreach (CustomQueries.query cQuery in _customQuery.Queries)
            {
                // Creates a new button for each query
                Button newButton = new() { Height = 48 };

                // Names the query the command if null otherwise names it correctly
                if (cQuery.queryName != null)
                {
                    newButton.Content = cQuery.queryName;
                }
                else
                {
                    newButton.Content = cQuery.commandName;
                }

                // Binds the query to the button
                newButton.Tag = cQuery;

                // Want to add right click context menu to each button
                ContextMenu contextMenu = new ContextMenu();

                MenuItem menuItem1 = new MenuItem { Header = "Execute" };
                menuItem1.Command = ExecuteCommandButton;
                menuItem1.CommandParameter = newButton;

                MenuItem menuItem2 = new MenuItem { Header = "Edit" };
                menuItem2.Command = EditCustomQuery;
                menuItem2.CommandParameter =
                    newButton; // This set the parent of the menuitem to the button so it is accessible

                MenuItem menuItem3 = new MenuItem { Header = "Delete" };
                menuItem3.Command = DeleteCustomQuery;
                menuItem3.CommandParameter = newButton;

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
            SelectedCommand.Parameters.Clear();

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
    private async Task ExecuteGenericCommand(Command toBeExecuted)
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
    /// <param name="_">Neccisary Parameter that passes the object that is clicked</param>
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
    /// This will be to Execute a query to a text file
    /// TODO: Add a description to the parameter of this method.
    /// TODO: Rename this method to match method naming conventions!!!
    /// </summary>
    /// <param name="_"></param>
    private async void OutputPowershellOutputToText(object _)
    {
        await ExecuteSelectedCommand();

        // Filepath
        //  Write the text to a file & prompt user for the location

        SaveFileDialog saveFileDialog = new SaveFileDialog();

        // Set properties of the OpenFileDialog
        saveFileDialog.FileName = "Document"; // Default file name
        saveFileDialog.Filter = "All files(*.*) | *.*";

        // Display
        Nullable<bool> result = saveFileDialog.ShowDialog();

        // Get file and write text
        if (result == true)
        {
            // Open document
            string filePath = saveFileDialog.FileName;
            File.WriteAllText(filePath, PowerShellOutput);
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
            var newLine = string.Format("{0},{1}", first, second);
            csv.AppendLine(newLine);
        }

        // Write the text to a file & prompt user for the location

        SaveFileDialog saveFileDialog = new SaveFileDialog();

        // Set properties of the OpenFileDialog
        saveFileDialog.FileName = "Document"; // Default file name
        saveFileDialog.Filter = "All files(*.*) | *.*";

        // Display
        Nullable<bool> result = saveFileDialog.ShowDialog();

        // Get file and write text
        if (result == true)
        {
            // Open document
            string filePath = saveFileDialog.FileName;
            File.WriteAllText(filePath, csv.ToString());
        }

        // debug
        // Trace.WriteLine(PowerShellOutput);
    }

    /// <summary>
    /// This method is for getting the currently selected command at anytime !Still in the works!
    /// </summary>
    public void UpdateSelectedCommand()
    {
        // Try to get the content within the drop downs
        try
        {
            int i = 0;
            foreach (var comboBoxData in DynamicParameterCollection)
            {
                string selectedItem = comboBoxData.SelectedParameter;
                // Need to look at this to see if it is working with the object type and then serialize it
                SelectedCommand.Parameters.Add(
                    new CommandParameter(comboBoxData.SelectedParameter,
                                         DynamicParameterValuesCollection[i].SelectedParameterValue));
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex);
        }
    }

    /// <summary>
    /// Gets the CustomQuery.query object for the current query when called TODO: It needs to be tested !Still in the
    /// works !
    /// </summary>
    public void GetCurrentQuery()
    {
        // Update the selected command
        UpdateSelectedCommand();

        try
        {
            string[] commandParameters = new string[SelectedCommand.Parameters.Count];
            string[] commandParameterValues = new string[SelectedCommand.Parameters.Count];

            _currentQuery.queryDescription = QueryDescription;
            _currentQuery.queryName = QueryName;

            _currentQuery.commandName = SelectedCommand.CommandText;

            int i = 0;
            foreach (CommandParameter CP in SelectedCommand.Parameters)
            {
                commandParameters[i] = CP.Name;
                commandParameterValues[i] = CP.Value.ToString();
                i++;
            }
            _currentQuery.commandParameters = commandParameters;
            _currentQuery.commandParametersValues = commandParameterValues;
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
    /// </summary>
    /// <param name="commandParameter">This is not used but neccisary for the relaycommand</param>
    private void PerformSavedQueries(object commandParameter)
    {
        if (isEditing != null)
        {
            // CustomQueries.query editingQuery = _customQuery.Queries.Find(item => item == isEditing);
            GetCurrentQuery();
            _customQuery.Queries[_customQuery.Queries.IndexOf(isEditing)] = _currentQuery;
            _customQuery.SerializeMethod();
            isEditing = null;
        }
        else
        {
            // Try to get the content within the drop downs
            try
            {
                int i = 0;
                foreach (var comboBoxData in DynamicParameterCollection)
                {
                    string selectedItem = comboBoxData.SelectedParameter;
                    // Need to look at this to see if it is working with the object type and then serialize it
                    // Trace.WriteLine(DynamicParameterValuesCollection[i].SelectedParameterValue);

                    SelectedCommand.Parameters.Add(
                        new CommandParameter(comboBoxData.SelectedParameter,
                                             DynamicParameterValuesCollection[i].SelectedParameterValue));
                    i++;
                }

                _customQuery.SerializeCommand(SelectedCommand, QueryName, QueryDescription);

                Button newButton = createCustomButton();

                ButtonStackPanel.Add(newButton);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
        }
    }

    public Button createCustomButton()
    {
        Button newButton = new() { Height = 48 };

        if (QueryName.Length != 0)
        {
            newButton.Content = QueryName;
        }
        else
        {
            newButton.Content = SelectedCommand.CommandText;
        }
        // Want to add right click context menu to each button
        ContextMenu contextMenu = new ContextMenu();

        MenuItem menuItem1 = new MenuItem { Header = "Execute" };
        menuItem1.Command = ExecuteCommandButton;
        menuItem1.CommandParameter = newButton;

        MenuItem menuItem2 = new MenuItem { Header = "Edit" };
        menuItem2.Command = EditCustomQuery;
        menuItem2.CommandParameter = newButton; // This set the parent of the menuitem to the button so it is accessible

        MenuItem menuItem3 = new MenuItem { Header = "Delete" };
        menuItem3.Command = DeleteCustomQuery;
        menuItem3.CommandParameter = newButton;

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
