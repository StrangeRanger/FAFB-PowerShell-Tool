using System.Management.Automation.Runspaces;

namespace ActiveDirectoryQuerier.PowerShell;

public interface ICommandParameters
{
    void LoadCommandParameters(Command? commandObject);
}
