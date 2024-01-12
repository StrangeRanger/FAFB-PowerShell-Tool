namespace FAFB_PowerShell_Tool.PowerShell;

public record ReturnValues
{
    public bool HadErrors { get; set; }
    public List<string> StdOut { get; } = new();
    public List<string> StdErr { get; } = new();
}
