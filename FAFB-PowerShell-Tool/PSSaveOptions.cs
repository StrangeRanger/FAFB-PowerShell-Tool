using FAFB_PowerShell_Tool.PowerShell;
using System.Management.Automation.Internal;

namespace FAFB_PowerShell_Tool;

/// <summary>
/// This class *might* be used for housing the save options
/// TODO: Delete this class if it is not used...
/// </summary>
public class PSSaveOptions
{
    /// <summary>
    /// This would be to output the query to a csv, will need to eventually adapt to a guicommand as well most likely
    /// </summary>
    /// <param name="commandString"> This is the internal command that we want to output to a csv </param>
    /*public Command OutputToCSV(Command commandString)
    {
        //Checks to see if the parameters are null,
        if (commandString.Parameters == null || commandString.Parameters.Count == 0)
        {
            //if they are null then make a new array with the output option, currently to relative path to SavedOutput
    folder commandString.Parameters.Add("| Export-CSV ..\\..\\..\\SavedOutput\\output.csv");
        }
        else
        {
            // If it is not null we will clone the parameters and add the output parameter to the new string and then
    set the array string[] temp = commandString.Parameters; Array.Resize(ref temp, temp.Length + 1); temp[temp.Length -
    1] = "| Export-CSV ..\\..\\..\\SavedOutput\\output.csv";
            //setting
            commandString.Parameters = temp;
        }
        //return the commandString with the updated parameters
        return commandString;
    }*/

    /// <summary>
    /// This would be to have the query output to a message box
    /// </summary>
    public InternalCommand OutputToMessageBox(InternalCommand commandSting)
    {
        return null;
    }

    /// <summary>
    /// This will be to save the query to a text file
    /// </summary>
    public void SaveToTxt(ReturnValues results)
    { }

    /// <summary>
    /// This is used for saving the file to a PS1 powershell file
    /// </summary>
    public void SaveToPS(ReturnValues results)
    { }
}
