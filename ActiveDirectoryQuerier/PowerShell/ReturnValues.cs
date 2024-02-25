namespace ActiveDirectoryQuerier.PowerShell;

/// <summary>
/// Encapsulates the return values from a PowerShell command, separated into standard output (StdOut) and standard
/// error (StdErr).
/// </summary>
public record ReturnValues
{
    /// <summary>
    /// Gets a value indicating whether the command execution resulted in any errors.
    /// </summary>
    /// <value>
    /// True if there are entries in the standard error list; otherwise, false.
    /// </value>
    public bool HadErrors => StdErr.Count > 0;

    /// <summary>
    /// Gets the standard output (StdOut) of the PowerShell command execution.
    /// </summary>
    /// <value>
    /// A list of strings representing the standard output from the command.
    /// </value>
    public List<string> StdOut { get; } = new();

    /// <summary>
    /// Gets the standard error (StdErr) of the PowerShell command execution.
    /// </summary>
    /// <value>
    /// A list of strings representing the standard error output from the command.
    /// </value>
    public List<string> StdErr { get; } = new();
}