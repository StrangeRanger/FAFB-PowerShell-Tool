using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace FAFB_PowerShell_Tool
{
    public class QueryBuilder
    {
        public static List<string> CommandList()
        {
            List<string> list = new List<string>();

            try
            {
                string srcFilePath = 'commands.txt';

                StreamReader sr = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read));

                // Read file.
                while ((line = sr.ReadLine()) != null)
                {
                    // Initialization.
                    CountryObj obj = new CountryObj();
                    string[] info = line.Split(':');

                    // Setting.
                    obj.CountryCode = info[0].ToString();
                    obj.CountryName = info[1].ToString();

                    // Adding.
                    lst.Add(obj);
                }

                // Closing.
                sr.Dispose();
                sr.Close();
            }
        }
        catch (Exception ex)
        { }

        return list;
    }

}
}
