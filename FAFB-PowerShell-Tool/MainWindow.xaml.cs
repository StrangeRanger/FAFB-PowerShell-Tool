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
using FAFB_PowerShell_Tool.PowerShell.Commands;

namespace FAFB_PowerShell_Tool;

public partial class MainWindow
{
    // Creates an instance to be able to serialize the queries
    CustomQueries CQ = new CustomQueries();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded; // Allows for async method to be called in the Loaded event.
    }

    /// <summary>
    /// This method is used to populate the first ComboBox with the commands that are available.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        ComboBox comboBoxCommandList = ComboBoxCommandList;
        ObservableCollection<GuiCommand> list = await ActiveDirectoryCommands.GetActiveDirectoryCommands();
        comboBoxCommandList.ItemsSource = list;
        comboBoxCommandList.DisplayMemberPath = "CommandName";

        FillCustomQueries();
    }

    /// <summary>
    /// This method is used to populate the second ComboBox with the parameters of the selected command.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBox comboBox = sender as ComboBox ?? throw new InvalidOperationException();

        if (comboBox.SelectedItem is not GuiCommand selectedCommand)
        {
            return;
        }

        // Set the ItemsSource for the parameters ComboBox.
        ComboBox comboBoxParameters = ComboBoxCommandParameterList;
        await selectedCommand.LoadCommandParametersAsync(); // Lazy loading.
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
            GuiCommand? command = ComboBoxCommandList.SelectedValue as GuiCommand;
            // string commandParameters = cmbParameters.Text;

            Button newButton = new() {

                Content = "Special Command",
                Height = 48
            };
            // Adds the query to the Queries serializable list
            CQ.Queries.Add(command.CommandName);
            // Adds the button in the stack panel
            ButtonStackPanel.Children.Add(newButton);
            // Saves the Queries to the file
            CQ.SerializeMethod();
        }
        catch (Exception ex)
        {
            Console.Write(ex);
        }
    }
    /// <summary>
    /// This Method is to fill the left had side query bar
    /// </summary>
    private void FillCustomQueries()
    {
        try
        {
            CQ.LoadData();

            foreach (string cQuery in CQ.Queries)
            {
                Button newButton = new() { Content = "Special Command", Height = 48 };

                ButtonStackPanel.Children.Add(newButton);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    // TODO: Test to see if this works as it should...
    /*private void ExecuteScriptEditorButton(object sender, RoutedEventArgs e)
    {
        string scriptEditorText = ScriptEditorTextBox.Text;
        PowerShellExecutor powerShellExecutor = new();

        Trace.WriteLine(scriptEditorText);

        try
        {
            ExecuteReturnValues commandOutput = powerShellExecutor.Execute(scriptEditorText);
            StringBuilder fullCommandOutput = new StringBuilder();

            foreach (var str in commandOutput.StdOut)
            {
                fullCommandOutput.Append(str);
            }

            MessageBoxOutput.Show(fullCommandOutput.ToString());
        }
        catch (Exception ex)
        {
            MessageBoxOutput.Show(ex.Message, MessageBoxOutput.OutputType.InternalError);
        }
    }*/
}
