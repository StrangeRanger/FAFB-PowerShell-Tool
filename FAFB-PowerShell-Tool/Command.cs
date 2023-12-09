using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAFB_PowerShell_Tool
{
    internal class Command
    {
        string commandName;
        string[] parameters;
        int parameterCount;

        public Command(string commandName, string[] parameters, int parameterCount)
        {
            this.commandName = commandName;
            this.parameters = parameters;
            this.parameterCount = parameterCount;
        }
    }



  



}
