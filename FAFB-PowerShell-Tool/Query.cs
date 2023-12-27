using System.IO;
using System.Collections.ObjectModel;

namespace FAFB_PowerShell_Tool;

/// <summary>
/// This will contain a query that is built
/// </summary>
public class Query
{
    public static ObservableCollection<string> CommandList()
    {
        ObservableCollection<string> list = new ObservableCollection<string>();

        try
        {
            string srcFilePath = "commands.txt";
            string[] lines = File.ReadAllLines(srcFilePath);

            // Read file.
            foreach (string line in lines)
            {
                list.Add(line);
            }
            // Closing.
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return list;
    }
}
