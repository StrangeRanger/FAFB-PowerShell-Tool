namespace FAFB_PowerShell_Tool.PowerShell;

public class ExecuteReturnValues
{
    public bool HadErrors { get; set; }
    public List<string> StdOut { get; set; } = new();
    public List<string> StdErr { get; set; } = new();
    public List<string> StdWarn { get; set; } = new();
}