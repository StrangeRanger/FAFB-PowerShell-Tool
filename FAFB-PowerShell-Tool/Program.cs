namespace FAFB_PowerShell_Tool;

class Program
{
    static void Test(string[] args)
    {
        FAFB_PowerShell_Tool.App app = new FAFB_PowerShell_Tool.App();
        app.InitializeComponent();
        app.Run();

        // Example usage of the PowerShellExecutor class.
        PowerShellExecutor.ExecutePowerShellCommand("Get-Process");

        Console.WriteLine("--------------------");

	// Example usage of the PowerShellExecutor class.
        PowerShellExecutor.ExecutePowerShellCommand("Get-Process | Out-String -Width 4096");

        Console.WriteLine("--------------------");

        // Get list of files in the home directory.
        PowerShellExecutor.ExecutePowerShellCommand("Get-ChildItem -Path $env:USERPROFILE | Out-String -Width 4096");

        Console.WriteLine("--------------------");

        // Create a file in the home directory.
        PowerShellExecutor.ExecutePowerShellCommand("New-Item -Path $env:USERPROFILE\\myFile.txt -ItemType File");
    }
}
