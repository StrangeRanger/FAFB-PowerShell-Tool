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
            command = "Get-Process";
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
            List<string> commandOutput = PowerShellExecutor.Execute(command);
            string fullCommandOutput = "";


            foreach (var str in commandOutput)
            {
                fullCommandOutput += str;
            }

            MessageBox.Show(fullCommandOutput);
        }
    }
}