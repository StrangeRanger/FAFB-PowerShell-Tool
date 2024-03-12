using System.Diagnostics;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ActiveDirectoryQuerier.Queries;

/// <summary>
/// This class is used to save a json file named "CustomQueries.dat" inside of
/// \ActiveDirectoryQuerier\ActiveDirectoryQuerier\bin\Debug\net6.0-windows This
/// </summary>
/// TODO: Remove property descriptions if name is descriptive enough, or provide a detailed description if it is not.
public class CustomQueries
{
    public List<Query> Queries { get; private set; } = new();

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
            File.WriteAllText("CustomQueries.dat", serializedJsonQueries);
        }
        // TODO: Possibly provide more comprehensive error handling.
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    /// <summary>
    /// This method is for serializing a command type, so it converts the type to a query and then serializes it
    /// </summary>
    /// TODO: Fix any and all warnings about possible null values.
    public void SerializeCommand(Command? psCommand, string queryName, string queryDescription)
    {
        string[] commandParameters = new string[psCommand.Parameters.Count];
        string[] commandParameterValues = new string[psCommand.Parameters.Count];
        CustomQueries customQueries = new();
        Query newQuery = new(psCommand.CommandText)
        {
            QueryName = queryName,
            QueryDescription = queryDescription
        };
        
        // TODO: Determine if this line is necessary.
        Trace.WriteLine(psCommand.Parameters.Count);

        // Iterate over the parameters and add them to the string
        for (int i = 0; i < psCommand.Parameters.Count; i++)
        {
            CommandParameter param = psCommand.Parameters[i];

            Trace.WriteLine(param.Name + " Value: ");

            commandParameters[i] = param.Name;
            commandParameterValues[i] = param.Value.ToString();
        }
        
        newQuery.PSCommandParameters = commandParameters;
        newQuery.PSCommandParameterValues = commandParameterValues;

        Queries.Add(newQuery);
        customQueries.SaveQueriesToJson();
        
        try
        {
            string serializedJsonQueries = JsonSerializer.Serialize(Queries, _options);
            Trace.WriteLine(serializedJsonQueries);
            File.WriteAllText("CustomQueries.dat", serializedJsonQueries);
        }
        // TODO: Possibly provide more comprehensive error handling.
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }
    /// <summary>
    /// This method Loads the string from the saved file "CustomQueries.dat" then gives it to the Queries List
    /// </summary>
    /// TODO: Fix any and all warnings about possible null values.
    public void LoadData()
    {
        try
        {
            // Opens file and reads it then adds the json to the Queries List
            string json = File.ReadAllText("CustomQueries.dat");
            Queries = JsonSerializer.Deserialize<List<Query>>(json, _options);

            // Now we want to fill the command variable for each query
            foreach (Query query in Queries)
            {
                Command psCommand = new Command(query.PSCommandName);

                // Check if there are parameters, and if there are add them to the list
                if (query.PSCommandParameters != null)
                {
                    for (int i = 0; i < query.PSCommandParameters.Length; i++)
                    {
                        psCommand.Parameters.Add(query.PSCommandParameters[i], query.PSCommandParameterValues[i]);
                        // the values are working then we will use this
                        // command.Parameters.Add(q.ADCommandParameters[i]);
                    }
                }

                // set the query command to the command
                query.Command = psCommand;
            }
        }
        // TODO: Possibly provide more comprehensive error handling.
        catch (Exception exception)
        {
            Trace.WriteLine(exception);
        }
    }
}
