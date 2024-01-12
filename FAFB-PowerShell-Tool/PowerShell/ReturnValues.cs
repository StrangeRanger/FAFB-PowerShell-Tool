namespace FAFB_PowerShell_Tool.PowerShell;

public record ReturnValues
{
    public bool HadErrors => StdErr.Count > 0;
    public List<string> StdOut { get; } = new();
    public List<string> StdErr { get; } = new();
}
