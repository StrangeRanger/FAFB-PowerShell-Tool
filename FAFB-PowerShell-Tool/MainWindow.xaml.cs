using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace FAFB_PowerShell_Tool;

public partial class MainWindow : Window
{
    private string _command = null!;
    
    public MainWindow()
    {
        InitializeComponent();
        
        ComboBox comboBoxCommandList = ComboBoxCommandList;
        ObservableCollection<Command> list = Command.GetActiveDirectoryCommands();
        comboBoxCommandList.ItemsSource = list;
        comboBoxCommandList.DisplayMemberPath = "CommandName";
    }
    
    // NOTE: Temporary method for testing purposes only.
    private void MSampleOne(object sender, RoutedEventArgs e)
    {
        _command = "Get-ADUser -filter * -Properties * | Select name, department, title | Out-String";
    }
    
    // NOTE: Temporary method for testing purposes only.
    private void MSampleTwo(object sender, RoutedEventArgs e)
    {
        _command = "Get-Process | Out-String";
    }
    
    // NOTE: Temporary method for testing purposes only.
    private void MSampleThree(object sender, RoutedEventArgs e)
    {
        _command = "Get-ChildItem -Path $env:USERPROFILE | Out-String";
    }
    
    private void MExecutionButton(object sender, RoutedEventArgs e)
    {
        PowerShellExecutor powerShellExecutor = new();

        try
        {
            List<string> commandOutput = powerShellExecutor.Execute(_command);
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

    // TODO: Test to see if this works as it should...
    private void MComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBox? comboBox = sender as ComboBox;
        Command? selectedCommand = comboBox?.SelectedValue as Command;
        string[]? commandParameters = Command.GetCommandParameters(selectedCommand?.CommandName);
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