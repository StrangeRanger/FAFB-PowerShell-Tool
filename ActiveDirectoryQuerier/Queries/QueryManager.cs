using System.IO;
using System.Management.Automation.Runspaces;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace ActiveDirectoryQuerier.Queries;

/// <summary>
/// Manages queries, including saving and loading queries from a file.
/// </summary>
public class QueryManager
{
    private readonly JsonSerializerOptions _jsonSerializationOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.Preserve,
    };

    public List<Query> Queries { get; private set; } = new();
    public string QueryFileSaveLocation { get; set; } = string.Empty;

    public void SaveQueryToFile()
    {
        try
        {
            string serializedJsonQueries = JsonSerializer.Serialize(Queries, _jsonSerializationOptions);
            File.WriteAllText(QueryFileSaveLocation == "" ? "CustomQueries.json" : QueryFileSaveLocation,
                              serializedJsonQueries);
        }
        // TODO: Improve exception handling.
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
    }

    public void ConvertCommandToQueryAndSave(Command psCommand, string queryName, string queryDescription)
    {
        var commandParameters = new string[psCommand.Parameters.Count];
        var commandParameterValues = new string[psCommand.Parameters.Count];
        Query newQuery = new(psCommand.CommandText) { QueryName = queryName, QueryDescription = queryDescription };

        // Loop through the parameters and add them to the new query.
        for (int i = 0; i < psCommand.Parameters.Count; i++)
        {
            CommandParameter parameter = psCommand.Parameters[i];
            commandParameters[i] = parameter.Name;
            commandParameterValues[i] = parameter.Value.ToString()!;
        }

        // Set the new query's parameters.
        newQuery.PSCommandParameters = commandParameters;
        newQuery.PSCommandParameterValues = commandParameterValues;

        // Add the new query to the list of queries.
        Queries.Add(newQuery);
        SaveQueryToFile();

        try
        {
            string serializedJsonQueries = JsonSerializer.Serialize(Queries, _jsonSerializationOptions);
            File.WriteAllText(QueryFileSaveLocation == "" ? "CustomQueries.json" : QueryFileSaveLocation,
                              serializedJsonQueries);
        }
        // TODO: Improve exception handling.
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
    }

    public void LoadQueriesFromFile()
    {
        try
        {
            string fileContents;

            // Opens file and reads it then adds the json to the Queries List
            if (QueryFileSaveLocation == "")
            {
                if (File.Exists("CustomQueries.json"))
                {
                    fileContents = File.ReadAllText("CustomQueries.json");
                }
                else
                {
                    return;
                }
            }
            else
            {
                fileContents = File.ReadAllText(QueryFileSaveLocation);
            }

            Queries = JsonSerializer.Deserialize<List<Query>>(fileContents, _jsonSerializationOptions)!;

            // Now we want to fill the command variable for each query
            foreach (Query query in Queries)
            {
                Command psCommand = new(query.PSCommandName);

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
        // TODO: Improve exception handling.
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
    }
}
