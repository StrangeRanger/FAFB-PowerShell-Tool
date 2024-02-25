using System.Management.Automation.Runspaces;

namespace FAFB_PowerShell_Tool.PowerShell;

public interface ICommandParameters
{
    void LoadCommandParameters(Command? commandObject);
}
