namespace ActiveDirectoryQuerier.PowerShell;

// ReSharper disable once InconsistentNaming
public record PSOutput
{
    public bool HadErrors => StdErr.Count > 0;
    public List<string> StdOut { get; } = new();
    public List<string> StdErr { get; } = new();
}
