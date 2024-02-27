using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using ActiveDirectoryQuerier.PowerShell;

namespace ActiveDirectoryQuerier;

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
    public Command OutputToCsv(Command commandString)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// This would be to have the query output to a message box
    /// </summary>
    public InternalCommand OutputToMessageBox(InternalCommand commandSting)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// This will be to save the query to a text file
    /// </summary>
    public void SaveToTxt(ReturnValues results)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// This is used for saving the file to a PS1 powershell file
    /// </summary>
    public void SaveToPS(ReturnValues results)
    {
        throw new NotImplementedException();
    }
}
