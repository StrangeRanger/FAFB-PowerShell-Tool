using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace FAFB_PowerShell_Tool;

/// <summary>
///  This class is used to organize the all of the powershell commands
/// </summary>
///
/// <param name="commandName">This is the root commands name ex. "get-aduser" </param>
/// <param name="parameters">This is an array of the parameter options that the command has</param>
/// <param name="parameterCount">This is the counr of the parameter array</param>
public class Command
{
    public string CommandName { get; set; }
    private string[] Parameters { get; set; }
    private int ParameterCount { get; set; }
    private static readonly PowerShellExecutor PowerShellExecutor = PowerShellExecutor.Instance;

    public Command(string commandName, string[] parameters, int parameterCount)
    {
        CommandName = commandName;
        Parameters = parameters;
        ParameterCount = parameterCount;
    }
    public Command(string commandName)
    {
        CommandName = commandName;
    }

    /// <summary>
    /// This is a method for getting a list of commands from a file
    /// </summary>
    /// <returns> "list" this is an ObservableCollection of the Commands </returns>
    public static ObservableCollection<Command> ReadFileCommandList()
    {
        ObservableCollection<Command> list = new ObservableCollection<Command>();

        try
        {
            // get the file path will probably want user input eventually and to use relative paths
            string srcFilePath =
                "C:\\Users\\pickl\\Source\\Repos\\FAFB-PowerShell-Tool\\FAFB-PowerShell-Tool\\commands.txt";
            string[] lines = File.ReadAllLines(srcFilePath);

            // Trim the strings and add them to the return list
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                Command command = new Command(trimmedLine);

                list.Add(command);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return list;
    }

    /// <summary>
    /// Retrieves the commands of a particular module.
    /// </summary>
    /// <returns>A List of Commands in an Observable Collection.</returns>
    public static ObservableCollection<Command> GetPowerShellCommands()
    {
        return new ObservableCollection<Command>();
    }

    /// <summary>
    /// Retrieves the parameters of a Command.
    /// </summary>
    /// <param name="c">The command in question.</param>
    /// <returns>...</returns>
    public static string[] GetParametersArray(Command c)
    {
        string getParameterSetNames =
            "Import-Module ActiveDirectory" + "(Get-Command " + c.CommandName +
            ").ParameterSets | Select-Object -Property @{n='ParameterSetName';e={$_.name}}, @{n='Parameters';e={$_.ToString()}}";

        // List<string> results = PowerShellExecutor.Execute(getParameterSetnames);
        // Trace.WriteLine(results.Count);

        try
        {
            List<string> commandOutput = PowerShellExecutor.Execute(getParameterSetNames);
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

        return new string[1];
    }
}
