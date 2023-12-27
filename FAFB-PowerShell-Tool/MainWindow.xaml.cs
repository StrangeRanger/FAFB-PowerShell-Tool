using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace FAFB_PowerShell_Tool;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private string _command = null!;

    /// <summary>
    /// 
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        // This is for the ComboBox containing the commands.
        try
        {
            // Get the command Combo Box to modify.
            ComboBox comboBoxCommandList = ComboBoxCommandList;
            ObservableCollection<Command> list = Command.ReadFileCommandList();
            comboBoxCommandList.ItemsSource = list;
            comboBoxCommandList.DisplayMemberPath = "CommandName";
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MSampleOne(object sender, RoutedEventArgs e)
    {
        _command = "Get-ADUser -filter * -Properties * | Select name, department, title | Out-String";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MSampleTwo(object sender, RoutedEventArgs e)
    {
        _command = "Get-Process | Out-String";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MSampleThree(object sender, RoutedEventArgs e)
    {
        _command = "Get-ChildItem -Path $env:USERPROFILE | Out-String";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MExecutionButton(object sender, RoutedEventArgs e)
    {
        PowerShellExecutor powerShellExecutor = new PowerShellExecutor();

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MExecuteGenericCommand(object sender, RoutedEventArgs e)
    {
        string hostName = System.Net.Dns.GetHostName();
        MessageBoxOutput.ShowMessageBox("System Host Name: " + hostName);
    }

    // TODO: Review this method, as it has unused variables...
    private void MComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Get the ComboBox that was changed.
        ComboBox? cmb = sender as ComboBox;
        
        // Get the selected command.
        try
        {
            Command? selectedCommand = cmb.SelectedValue as Command;
            Trace.WriteLine("Selcetion changed: " + selectedCommand.CommandName);

            string[] test = Command.GetParametersArray(selectedCommand);
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    // TODO: Figure out the purpose of this method.
    private void MTextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        // TODO: Add method body.
    }

    // TODO: Review this method, as it has unused variables and is not used...
    private void MButtonClickOne(object sender, RoutedEventArgs e)
    {
        String computerName = "";

        String startRemoteSession = "$sessionAD = New-PSSession -ComputerName" + computerName;

        /*
        TextBox tbx = new TextBox();

        tbx.Visibility = Visibility.Visible;
        //tbx.ClearValue
        tbx.
        */
    }

    // TODO: Review this method, as it is not used...
    private void RunRemoteCommand(String command)
    {
        // command is the command you want to run like get-aduser

        String invokeCommand = "Invoke-Command -Session $sessionAD -ScriptBlock{" + command + "}";
        PowerShellExecutor powerShellExecutor = new();

        try
        {
            List<string> commandOutput = powerShellExecutor.Execute(invokeCommand);
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

    // TODO: Review this method, as it has unused variables...
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

    // TODO: Review this method, as it is not used...
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