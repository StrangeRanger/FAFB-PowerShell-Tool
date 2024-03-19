using System.Diagnostics;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace ActiveDirectoryQuerier.Queries;

/// <summary>
/// This class is used to save a json file named "CustomQueries.json" inside of
/// \ActiveDirectoryQuerier\ActiveDirectoryQuerier\bin\Debug\net6.0-windows This
/// </summary>
public class QueryStackPanel
{
    public List<Query> Queries { get; private set; } = new();
    public string CustomQueryFileLocation { get; set; } = string.Empty;

    /// <summary>
    /// This a variable for feeding options to the Json serializer
    /// </summary>
    private static readonly JsonSerializerOptions _options = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.Preserve,
    };

    /// <summary>
    /// This takes the Queries List and serializes it to a file
    /// </summary>
    public void SaveQueriesToJson()
    {
        try
        {
            string serializedJsonQueries = JsonSerializer.Serialize(Queries, _options);
            File.WriteAllText(CustomQueryFileLocation == "" ? "CustomQueries.json" : CustomQueryFileLocation,
                              serializedJsonQueries);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    /// <summary>
    /// This method is for serializing a command type, so it converts the type to a query and then serializes it
    /// </summary>
    /// <param name="psCommand">Command class that you want to serialize</param>
    /// <param name="queryDescription">Description field of the Query</param>
    /// <param name="queryName">Query Name</param>
    public void SerializeCommand(Command? psCommand, string queryName, string queryDescription)
    {
        string[] commandParameters = new string[psCommand!.Parameters.Count];
        string[] commandParameterValues = new string[psCommand.Parameters.Count];
        Query newQuery = new(psCommand.CommandText) { QueryName = queryName, QueryDescription = queryDescription };

        // Iterate over the parameters and add them to the string
        for (int i = 0; i < psCommand.Parameters.Count; i++)
        {
            CommandParameter param = psCommand.Parameters[i];

            commandParameters[i] = param.Name;
            commandParameterValues[i] = param.Value.ToString()!;
        }

        newQuery.PSCommandParameters = commandParameters;
        newQuery.PSCommandParameterValues = commandParameterValues;

        Queries.Add(newQuery);
        SaveQueriesToJson();

        try
        {
            string serializedJsonQueries = JsonSerializer.Serialize(Queries, _options);
            if (CustomQueryFileLocation == "")
            {
                File.WriteAllText("CustomQueries.json", serializedJsonQueries);
            }
            else
            {
                File.WriteAllText(CustomQueryFileLocation, serializedJsonQueries);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
    /// <summary>
    /// This method Loads the string from the saved file "CustomQueries.dat" then gives it to the Queries List
    /// </summary>
    public void LoadData()
    {
        try
        {
            // Opens file and reads it then adds the json to the Queries List
            string json;
            if (CustomQueryFileLocation == "")
            {
                if (File.Exists("CustomQueries.json"))
                {
                    json = File.ReadAllText("CustomQueries.json");
                }
                else
                {
                    return;
                }
            }
            else
            {
                json = File.ReadAllText(CustomQueryFileLocation);
            }
            Queries = JsonSerializer.Deserialize<List<Query>>(json, _options)!;

            // Now we want to fill the command variable for each query
            foreach (Query query in Queries)
            {
                Command psCommand = new Command(query.PSCommandName);

                // Check if there are parameters, and if there are add them to the list
                if (query.PSCommandParameters != null)
                {
                    for (int i = 0; i < query.PSCommandParameters.Length; i++)
                    {
                        psCommand.Parameters.Add(query.PSCommandParameters[i], query.PSCommandParameterValues![i]);
                    }
                }

                // set the query command to the command
                query.Command = psCommand;
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
    }
}
