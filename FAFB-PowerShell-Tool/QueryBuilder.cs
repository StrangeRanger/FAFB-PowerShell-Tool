using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace FAFB_PowerShell_Tool
{
    public class QueryBuilder
    {
        
        public static ObservableCollection<string> CommandList() { 

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
}
