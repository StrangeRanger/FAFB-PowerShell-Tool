using System.Diagnostics;
using System.Windows;

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
        }

        private void CommandButton1(object sender, RoutedEventArgs e)
        {
            command = "Get-ADUser -filter * -Properties * | out-gridview";
        }

        private void CommandButton2(object sender, RoutedEventArgs e)
        {
            command = "Get-Process | Out-String -Width 4096";
        }

        private void CommandButton3(object sender, RoutedEventArgs e)
        {
            command = "Get-ChildItem -Path $env:USERPROFILE | Out-String -Width 4096";
        }

        private void CommandButton4(object sender, RoutedEventArgs e)
        {
            command = "New-Item -Path $env:USERPROFILE\\myFile.txt -ItemType File";
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

                MessageBox.Show(fullCommandOutput);
            }
            catch (Exception ex)
            {
                MessageBox.Show("INTERNAL ERROR: " + ex.Message);
            }
        }

        private void ExecuteGenericCommand(object sender, RoutedEventArgs e)
        {
            string hostName = System.Net.Dns.GetHostName();
            MessageBox.Show("System Host Name: " + hostName);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}