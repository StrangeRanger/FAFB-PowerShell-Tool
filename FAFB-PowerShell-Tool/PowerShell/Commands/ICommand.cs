namespace FAFB_PowerShell_Tool.PowerShell.Commands;

/// <summary>
/// Interface for commands.
/// </summary>
/// <remarks>
/// This interface is used to ensure that when executing a PowerShell command via 'PowerShellExecutor', the command type
/// passed to the method has to be of type 'ICommand'.
/// </remarks>
public interface ICommand
{
    string CommandName { get; }
    //want to change this into a list 
    string[]? Parameters { get; set; }
    string CommandString { get; }
}