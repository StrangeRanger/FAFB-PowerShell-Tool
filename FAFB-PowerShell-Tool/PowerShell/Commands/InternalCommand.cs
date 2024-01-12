namespace FAFB_PowerShell_Tool.PowerShell.Commands;
/// <summary>
/// Class that implements ICommand and is specifically for Interally used command
/// </summary>
public class InternalCommand : ICommand
{
    public string CommandName { get; }
    public string[]? Parameters { get; set; }
    public string CommandString => Parameters is null ? CommandName : $"{CommandName} {string.Join(" ", Parameters)}";

    /// <summary>
    /// Commands that are not intended to be used by the user, but rather by the program itself.
    /// </summary>
    /// <param name="commandName">...</param>
    /// <param name="parameters">...</param>
    /// <exception cref="ArgumentException">...</exception>
    public InternalCommand(string commandName, string[]? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(commandName))
        {
            //MessageBoxOutput.Show("Command name cannot be null or whitespace.", MessageBoxOutput.OutputType.Error);
            throw new ArgumentException("Command name cannot be null or whitespace.", nameof(commandName));
        }
        
        CommandName = commandName;
        Parameters = parameters;
    }
}