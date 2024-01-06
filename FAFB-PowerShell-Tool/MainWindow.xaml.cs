using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FAFB_PowerShell_Tool.PowerShell;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Text.Json;
using System.Net.Security;


namespace FAFB_PowerShell_Tool;

public partial class MainWindow
{
    ObservableCollection<Command> saved_commands = new ObservableCollection<Command>();
    List<Button> custom_buttons = new List<Button>();
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded; // Allows for async method to be called in the Loaded event.
    }
    
    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        ComboBox comboBoxCommandList = ComboBoxCommandList;
        ObservableCollection<Command> list = await ActiveDirectoryCommands.GetActiveDirectoryCommands();
        comboBoxCommandList.ItemsSource = list;
        comboBoxCommandList.DisplayMemberPath = "CommandName";


        FillCustomQueries();
    }
    
      /* NOTE: Temporary method for testing purposes only.
    private void MSampleOne(object sender, RoutedEventArgs e)
    {
        _command.CommandName = "Get-ADUser";
        _command.Parameters = new[] {"-filter", "*", "-Properties", "*", "| Select name, department, title"};
    }
    
    // NOTE: Temporary method for testing purposes only.
    private void MSampleTwo(object sender, RoutedEventArgs e)
    {
        _command.CommandName = "Get-Process";
        _command.Parameters = new[] {"| Select name, id, path"};
    }
    
    // NOTE: Temporary method for testing purposes only.
    private void MSampleThree(object sender, RoutedEventArgs e)
    {
        _command.CommandName = "Get-ChildItem";
        _command.Parameters = new[] {"-Path", "$env:USERPROFILE"};
    }
    
    // NOTE: POSSIBLY a temporary method for testing purposes only.
    private void MExecutionButton(object sender, RoutedEventArgs e)
    {
        PowerShellExecutor powerShellExecutor = new();

        try
        {
            List<string> commandOutput = powerShellExecutor.Execute(_command + " | Out-String");
            string fullCommandOutput = "";

            foreach (var str in commandOutput)
            {
                fullCommandOutput += str;
            }

            MessageBoxOutput.ShowMessageBox(fullCommandOutput);
        }
        catch (Exception ex)
        {
            MessageBoxOutput.ShowMessageBox(ex.Message, MessageBoxOutput.OutputType.InternalError);
        }
    }
    */
    /// <summary>
    /// This method is used to populate the second ComboBox with the parameters of the selected command.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBox comboBox = sender as ComboBox ?? throw new InvalidOperationException();

        if (comboBox.SelectedItem is not Command selectedCommand) { return; }
        
        // Set the ItemsSource for the parameters ComboBox.
        ComboBox comboBoxParameters = ComboBoxCommandParameterList;
        await selectedCommand.LoadCommandParametersAsync();  // Lazy loading.
        comboBoxParameters.ItemsSource = selectedCommand.PossibleParameters;
    }

    // TODO: Test to see if this works as it should...
    // TODO: Make the buttons able to save a command to them and have a universal class maybe for executing them
    private void MSaveQueryButton(object sender, RoutedEventArgs e)
    {
        // Try to get the content within the drop downs
        try
        {
            // Get the Command
            Command? commandName = ComboBoxCommandList.SelectedValue as Command;
            // string commandParameters = cmbParameters.Text;

            Button newButton = new()
            {
               
                Content = "Special Command",
                Height = 48
            };

            CustomQueries.CustomQueryButtons.Add(newButton);
            ButtonStackPanel.Children.Add(newButton);

            //Stream stream = File.Open("CustomQueries.dat", FileMode.Create);
            CustomQueries.SerializeMethod();
            
        }
        catch (Exception ex)
        {
            Console.Write(ex);
        }
    }
    /// <summary>
    /// This Method is to fill the left had side query bar
    /// </summary>
    private void FillCustomQueries() {
        try
        {
            CustomQueries.LoadData();

            foreach (Button btn in CustomQueries.CustomQueryButtons)
            {
                ButtonStackPanel.Children.Add(btn);
            }
        }catch (Exception ex){ 
            Trace.WriteLine(ex);
        }
    }

    // TODO: Test to see if this works as it should...
    private void ExecuteScriptEditorButton(object sender, RoutedEventArgs e)
    {
        string scriptEditorText = ScriptEditorTextBox.Text;
        PowerShellExecutor powerShellExecutor = new();

        Trace.WriteLine(scriptEditorText);

        try
        {
            List<string> commandOutput = powerShellExecutor.Execute(scriptEditorText);
            StringBuilder fullCommandOutput = new StringBuilder();

            foreach (var str in commandOutput)
            {
                fullCommandOutput.Append(str);
            }

            MessageBoxOutput.ShowMessageBox(fullCommandOutput.ToString());
        }
        catch (Exception ex)
        {
            MessageBoxOutput.ShowMessageBox(ex.Message, MessageBoxOutput.OutputType.InternalError);
        }
    }
}