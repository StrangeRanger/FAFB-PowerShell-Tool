using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace FAFB_PowerShell_Tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string command = null!;

        public MainWindow()
        {
            InitializeComponent();


            // This is for the combox containing the commands
            try
            {
                //Get the command Combo Box to modify
                ComboBox Cmb = this.cmbCommandList;
                ObservableCollection<Command> list = Command.ReadFileCommandList();
                Cmb.ItemsSource = list;
                Cmb.DisplayMemberPath = "commandName";
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            
        }

        private void CommandButton1(object sender, RoutedEventArgs e)
        {
            //Seems like you need to import the neccesary module that you are using possibly
            command = "Import-Module ActiveDirectory" +
                "Get-ADUser -filter * -Properties * | Select name, department, title | Out-String -Width 4096";
        }

        private void CommandButton2(object sender, RoutedEventArgs e)
        {
            command = "Get-Process";
        }

        private void CommandButton3(object sender, RoutedEventArgs e)
        {
            command = "Get-Process | Out-String -Width 4096";
        }

        private void CommandButton4(object sender, RoutedEventArgs e)
        {
            command = "Get-ChildItem -Path $env:USERPROFILE | Out-String -Width 4096";
        }

        private void ExecutionButton(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> commandOutput = PowerShellExecutor.Execute(command);
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

        private void Window_Loaded(object sender, RoutedEventArgs e) {
        
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Add method body.
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Get the combox that we are working with
            ComboBox? cmb = sender as ComboBox;
            //Get the command that is selected which is just an object so we have to convert to Command
            try
            {
                Command? selectedCommand = cmb.SelectedValue as Command;
                Trace.WriteLine("Selcetion changed: " + selectedCommand.commandName);

                string[] test = Command.GetParametersArray(selectedCommand);
            } catch (Exception ex) 
            { 
                Trace.WriteLine(ex); 
            }
            //Debug


        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
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

            try
            {
                List<string> commandOutput = PowerShellExecutor.Execute(invokeCommand);
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
    }
}
