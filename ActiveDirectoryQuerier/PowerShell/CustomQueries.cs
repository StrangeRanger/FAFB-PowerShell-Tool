using System.Diagnostics;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ActiveDirectoryQuerier.PowerShell;

/// <summary>
/// This class is used to save a json file named "CustomQueries.dat" inside of
/// \ActiveDirectoryQuerier\ActiveDirectoryQuerier\bin\Debug\net6.0-windows This
/// </summary>
internal class CustomQueries
{
    public class Query
    {
        /// <summary>
        /// Command that should help with Binding the command to the query for the buttons sake
        /// </summary>
        [JsonIgnore]
        public Command? Command { get; set; }

        /// <summary>
        /// Used for serializing the Command Name
        /// </summary>
        public string CommandName { get; set; }

        /// <summary>
        /// Used for Serializing the Commands parameters
        /// </summary>
        public string[] CommandParameters { get; set; }

        /// <summary>
        /// Used for Serializing the Commands parameters
        /// </summary>
        public string[] CommandParametersValues { get; set; }

        /// <summary>
        /// Used for the name of the custom Query
        /// </summary>
        public string QueryName { get; set; }

        /// <summary>
        /// Used for the Custom Queries Description
        /// </summary>
        public string QueryDescription { get; set; }

        // Constructor for the query class
        /*public Query(string cN, string[] commandParams)
        {
            this.CommandName = cN;
            this.CommandParameters = commandParams;
        }*/

        /// <summary>
        /// Constructor for the query class
        /// TODO: Fix any and all warnings about possible null values.
        /// </summary>
        /// <param name="cN"></param>
        public Query(string cN)
        {
            CommandName = cN;
        }

        /// <summary>
        /// Empty constructor for the query class
        /// TODO: Fix any and all warnings about possible null values.
        /// </summary>
        public Query()
        { }
    }

    public List<Query> Queries = new();

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
    public void SerializeMethod()
    {
        try
        {
            string json = JsonSerializer.Serialize(Queries, _options);
            File.WriteAllText("CustomQueries.dat", json);
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    /// <summary>
    /// This method is for serializing a command type, so it converts the type to a query and then serializes it
    /// TODO: Fix any and all warnings about possible null values.
    /// </summary>
    public void SerializeCommand(Command? cmnd, string queryName, string queryDescription)
    {
        Trace.WriteLine(cmnd.Parameters.Count);

        string[] commandParameters = new string[cmnd.Parameters.Count];
        string[] commandParameterValues = new string[cmnd.Parameters.Count];
        Query newQuery = new Query(cmnd.CommandText);

        // Set name and description
        newQuery.QueryName = queryName;
        newQuery.QueryDescription = queryDescription;

        // Iterate over the parameters and add them to the string

        for (int i = 0; i < cmnd.Parameters.Count; i++)
        {
            CommandParameter param = cmnd.Parameters[i];

            Trace.WriteLine(param.Name + " Value: ");

            commandParameters[i] = param.Name;
            commandParameterValues[i] = param.Value.ToString();
        }
        newQuery.CommandParameters = commandParameters;
        newQuery.CommandParametersValues = commandParameterValues;

        Queries.Add(newQuery);

        CustomQueries customQueries = new CustomQueries();
        customQueries.SerializeMethod();
        try
        {
            string json = JsonSerializer.Serialize(Queries, _options);
            Trace.WriteLine(json);
            File.WriteAllText("CustomQueries.dat", json);
        }
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
            foreach (Query q in Queries)
            {
                Command? command = new Command(q.CommandName);

                // Check if there are parameters, and if there are add them to the list
                if (q.CommandParameters != null)
                {
                    for (int i = 0; i < q.CommandParameters.Length; i++)
                    {
                        command.Parameters.Add(q.CommandParameters[i], q.CommandParametersValues[i]);
                        // the values are working then we will use this
                        // command.Parameters.Add(q.CommandParameters[i]);
                    }
                }

                // set the query command to the command
                q.Command = command;
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }
}
