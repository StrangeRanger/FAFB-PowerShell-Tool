namespace FAFB_PowerShell_Tool.PowerShell;

public interface ICommand
{
    string CommandName { get; }
    string[]? Parameters { get; set; }
    string CommandString { get; }
}