using System.Diagnostics;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FAFB_PowerShell_Tool.PowerShell
{
    /// <summary>
    /// This class is used to save a json file named "CustomQueries.dat" inside of
    /// \FAFB-PowerShell-Tool\FAFB-PowerShell-Tool\bin\Debug\net6.0-windows This
    /// </summary>
    /// <param name="Queries"></param>
    internal class CustomQueries
    {
        public class query
        {
            /// <summary>
            /// Used for serializing the Command Name
            /// </summary>
            public string commandName { get; set; }

            /// <summary>
            /// Used for Serializing the Commands parameters
            /// </summary>
            public string[] commandParameters { get; set; }
            
            /// <summary>
            /// Used for Serializing the Commands parameters
            /// </summary>
            public string[] commandParametersValues { get; set; }

            /// <summary>
            /// Used for the name of the custom Query
            /// </summary>
            public string queryName { get; set; }

            /// <summary>
            /// Used for the Custom Queries Description
            /// </summary>
            public string queryDescription { get; set; }

            public query(string cN, string[] commandParams)
            {
                this.commandName = cN;
                this.commandParameters = commandParams;
            }
            public query(string cN)
            {
                this.commandName = cN;
            }
            // Empty Constructor
            public query()
            { }
        }

        public List<query> Queries = new List<query>();

        /// <summary>
        /// This a variable for feeding options to the Json serializer
        /// </summary>
        private static readonly JsonSerializerOptions _options = new() {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.Preserve,
        };
        /// <summary>
        /// Emtpy Contructor
        /// </summary>
        public CustomQueries()
        { }
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
        /// </summary>
        public void SerializeCommand(Command cmnd, string queryName, string queryDescription)
        {
            Trace.WriteLine(cmnd.Parameters.Count);


            string[] commandParameters = new string[cmnd.Parameters.Count];
            string[] commandParameterValues = new string[cmnd.Parameters.Count];
            query newQuery = new query(cmnd.CommandText);

            //Set name and description
            newQuery.queryName = queryName;
            newQuery.queryDescription = queryDescription;


            // Iterate over the parameters and add them to the string
            
            for (int i = 0; i < cmnd.Parameters.Count; i++ )
            {
                CommandParameter param = cmnd.Parameters[i];

                Trace.WriteLine(param.Name + " Value: " );

                commandParameters[i] = param.Name;

            }
            newQuery.commandParameters = commandParameters;
            

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
        public void LoadData()
        {
            try
            {
                string json = File.ReadAllText("CustomQueries.dat");
                Queries = JsonSerializer.Deserialize<List<query>>(json, _options);

                Trace.WriteLine(Queries[0].commandName);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }
}
