using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;


namespace FAFB_PowerShell_Tool.PowerShell
{
    /// <summary>
    /// This class is used to save a json file named "CustomQueries.dat" inside of \FAFB-PowerShell-Tool\FAFB-PowerShell-Tool\bin\Debug\net6.0-windows
    /// This 
    /// </summary>
    /// <param name="Queries"></param>
    internal class CustomQueries
    {
        public List<string> Queries = new List<string>();
        /// <summary>
        /// This a variable for feeding options to the Json serializer
        /// </summary>
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.Preserve,
        };
        /// <summary>
        /// Emtpy Contructor
        /// </summary>
        public CustomQueries() { }
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
            catch(Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
        /// <summary>
        /// This method Loads the string from the saved file "CustomQueries.dat" then gives it to the Queries List
        /// </summary>
        public void LoadData() {
            try
            {
                string json = File.ReadAllText("CustomQueries.dat");
                Queries = JsonSerializer.Deserialize<List<string>>(json, _options);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);    
            }
        }
    }
}
