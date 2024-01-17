namespace FAFB_PowerShell_Tool.PowerShell;

/// <summary>
/// Encapsulates the return values from a PowerShell command, separated into standard output and standard error.
/// </summary>
public record ReturnValues
{
    public bool HadErrors => StdErr.Count > 0;
    public List<string> StdOut { get; } = new();
    public List<string> StdErr { get; } = new();
}
