using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using FAFB_PowerShell_Tool.PowerShell;

namespace FAFB_PowerShell_Tool;

public partial class MainWindow
{
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
        ObservableCollection<Command> list = await ActiveDirectoryCommands.GetActiveDirectoryCommands();
        comboBoxCommandList.ItemsSource = list;
        comboBoxCommandList.DisplayMemberPath = "CommandName";
    }
    
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

            ButtonStackPanel.Children.Add(newButton);
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
            StringBuilder fullCommandOutput = new StringBuilder();

            foreach (var str in commandOutput)
            {
                fullCommandOutput.Append(str);
            }

            MessageBoxOutput.Show(fullCommandOutput.ToString());
        }
        catch (Exception ex)
        {
            MessageBoxOutput.Show(ex.Message, MessageBoxOutput.OutputType.InternalError);
        }
    }
}