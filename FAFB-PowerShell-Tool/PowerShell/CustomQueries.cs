using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FAFB_PowerShell_Tool.PowerShell
{
    [Serializable()]
    internal class CustomQueries : ISerializable
    {
        public static List<Button> CustomQueryButtons = new List<Button>();
        
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("CustomQueryButtons", CustomQueryButtons);
        }

        public CustomQueries(SerializationInfo info, StreamingContext context) {
            CustomQueryButtons = (List<Button>)info.GetValue("CustomQueryButtons", typeof(List<Button>));
        }

        public static void SerializeMethod()
        {

            Stream stream = File.Open("CustomQueries.dat", FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(stream, CustomQueryButtons);
            stream.Close();

            //return JsonSerializer.Serialize(CustomQueryButtons, _options);
        }

        public static void LoadData() {

            Stream stream = File.Open("CustomQueries.dat", FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            CustomQueryButtons = (List<Button>)bf.Deserialize(stream);
            stream.Close();
        }
    }
}
