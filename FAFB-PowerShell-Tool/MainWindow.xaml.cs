using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace FAFB_PowerShell_Tool;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        InitializeAsync();
    }
    
    // TODO: Figure out why application still show loading symbol despite being async, here.
    private async void InitializeAsync()
    {
        ComboBox comboBoxCommandList = ComboBoxCommandList;
        ObservableCollection<Command> list = await ActiveDirectoryCommands.GetActiveDirectoryCommands();
        comboBoxCommandList.ItemsSource = list;
        comboBoxCommandList.DisplayMemberPath = "CommandName";
    }
    
    // NOTE: Temporary method for testing purposes only.
    /*private void MSampleOne(object sender, RoutedEventArgs e)
    {
        _command.CommandName = "Get-ADUser";
        _command.Parameters = new[] {"-filter", "*", "-Properties", "*", "| Select name, department, title"};
    }*/
    
    // NOTE: Temporary method for testing purposes only.
    /*private void MSampleTwo(object sender, RoutedEventArgs e)
    {
        _command.CommandName = "Get-Process";
        _command.Parameters = new[] {"| Select name, id, path"};
    }*/
    
    // NOTE: Temporary method for testing purposes only.
    /*private void MSampleThree(object sender, RoutedEventArgs e)
    {
        _command.CommandName = "Get-ChildItem";
        _command.Parameters = new[] {"-Path", "$env:USERPROFILE"};
    }*/
    
    // NOTE: POSSIBLY a temporary method for testing purposes only.
    /*private void MExecutionButton(object sender, RoutedEventArgs e)
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
    }*/
    
    /// <summary>
    /// This method is used to populate the second ComboBox with the parameters of the selected command.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBox comboBox = sender as ComboBox ?? throw new InvalidOperationException();
        
        if (comboBox.SelectedItem is Command selectedCommand)
        {
            // Set the ItemsSource for your parameters ComboBox.
            ComboBox comboBoxParameters = ComboBoxCommandParameterList;
            comboBoxParameters.ItemsSource = selectedCommand.PossibleParameters;
        }
    }

    // TODO: Test to see if this works as it should...
    private void MSaveQueryButton(object sender, RoutedEventArgs e)
    {
        // Try to get the content within the drop downs
        try
        {
            // Get the Command
            Command? commandName = ComboBoxCommandList.SelectedValue as Command;
            // string commandParameters = cmbParameters.Text;

            Button newButton = new Button();
            newButton.Content = "Special Command";
            newButton.Width = 140;
            newButton.Height = 48;

            LeftSideQueryGrid.Children.Add(newButton);
        }
        catch (Exception ex)
        {
            Console.Write(ex);
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
}