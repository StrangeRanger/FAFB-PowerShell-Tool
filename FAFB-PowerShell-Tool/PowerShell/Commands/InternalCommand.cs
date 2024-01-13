namespace FAFB_PowerShell_Tool.PowerShell.Commands;

/// <summary>
/// Commands that are not intended to be used by the user/GUI, but rather by the program itself.
/// </summary>
/// <remarks>
/// The separation of commands into 'InternalCommand' and 'GuiCommand' is because the internal commands don't need to
/// know the possible parameters for the command, whereas the GUI commands do.
/// </remarks>
public class InternalCommand : ICommand
{
    public string CommandName { get; }
    public string[]? Parameters { get; set; }
    public string CommandString => Parameters is null ? CommandName : $"{CommandName} {string.Join(" ", Parameters)}";

    /// <summary>
    /// Commands that are not intended to be used by the user, but rather by the program itself.
    /// </summary>
    /// <param name="commandName">The selected command.</param>
    /// <param name="parameters">Selected parameters for 'commandName'.</param>
    /// <exception cref="ArgumentException">Thrown when 'commandName' is null, contains whitespace, or is blank</exception>
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