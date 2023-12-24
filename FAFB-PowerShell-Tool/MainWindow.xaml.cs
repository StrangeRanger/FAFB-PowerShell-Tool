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

    public MainWindow()
    {
        InitializeComponent();

        // This is for the ComboBox containing the commands.
        try
        {
            // Get the command Combo Box to modify
            ComboBox cmb = cmbCommandList;
            ObservableCollection<Command> list = Command.ReadFileCommandList();
            cmb.ItemsSource = list;
            cmb.DisplayMemberPath = "CommandName";
        }
        catch (Exception ex)
        {
            Console.Write(ex);
        }
    }

    private void CommandButton1(object sender, RoutedEventArgs e)
    {
        _command = "Get-ADUser -filter * -Properties * | Select name, department, title | Out-String";
    }

    private void CommandButton2(object sender, RoutedEventArgs e)
    {
        _command = "Get-Process | Out-String";
    }

    private void CommandButton3(object sender, RoutedEventArgs e)
    {
        _command = "Get-ChildItem -Path $env:USERPROFILE";
    }

    private void CommandButton4(object sender, RoutedEventArgs e)
    {
        _command = "Get-ChildItem -Path $env:USERPROFILE | Out-String";
    }

    private void ExecutionButton(object sender, RoutedEventArgs e)
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

            MessageBox.Show(fullCommandOutput, "Command Output");
        }
        catch (Exception ex)
        {
            MessageBox.Show("INTERNAL ERROR: " + ex.Message, "ERROR");
        }
    }

    private void ExecuteGenericCommand(object sender, RoutedEventArgs e)
    {
        string hostName = System.Net.Dns.GetHostName();
        MessageBox.Show("System Host Name: " + hostName, "Command Output");
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    { }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Add method body.
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Get the combox that we are working with
        ComboBox? cmb = sender as ComboBox;
        // Get the command that is selected which is just an object so we have to convert to Command
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
        // Debug
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // TODO: Add method body.
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
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

            MessageBox.Show(fullCommandOutput);
        }
        catch (Exception ex)
        {
            MessageBox.Show("INTERNAL ERROR: " + ex.Message);
        }
    }

    /// <summary>
    /// Click Handler for the Save query button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SaveQuery_Click(object sender, RoutedEventArgs e)
    {
        // Try to get the content within the drop downs
        try
        {
            // Get the Command
            Command? commandName = cmbCommandList.SelectedValue as Command;
            // string commandParameters = cmbParameters.Text;

            Button newButton = new Button();
            newButton.Content = "Special Command";
            newButton.Width = 140;
            newButton.Height = 48;

            Left_Side_Query_Bar.Children.Add(newButton);
        }
        catch (Exception ex)
        {
            Console.Write(ex);
        }
    }

    private void ExecuteScriptEditorButton(object sender, RoutedEventArgs e)
    {
        string scriptEditorText = Script_Editor.Text;
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

            MessageBox.Show(fullCommandOutput, "Command Output");
        }
        catch (Exception ex)
        {
            MessageBox.Show("INTERNAL ERROR: " + ex.Message, "ERROR");
        }
    }
}
