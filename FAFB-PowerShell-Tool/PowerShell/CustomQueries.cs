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
        public class query {
            public string commandName { get; set; }
            public string[] commandParams { get; set; }

            public query(string cN, string[] commandParams) {
                this.commandName = cN;
                this.commandParams = commandParams;
            }
            public query(string cN) {
                this.commandName = cN;
            }
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
        public void SerializeCommand(Command cmnd) {
            string[] cparams = new string[cmnd.Parameters.Count];
            query newQuery = new query(cmnd.CommandText);

            // Iterate over the parameters and add them to the string
            int i = 0;
            foreach (var param in cmnd.Parameters)
            {
                //need to adjust this filling out 
                //cparams[i] = param.Name;
                //i++;
            }
            newQuery.commandParams = cparams;
            Queries.Add(newQuery);

            CustomQueries customQueries = new CustomQueries();
            customQueries.SerializeMethod();

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
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }
}
