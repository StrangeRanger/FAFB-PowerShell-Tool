using System.Management.Automation.Runspaces;
using ActiveDirectoryQuerier.PowerShell;

namespace ActiveDirectoryQuerier;

public class ActiveDirectoryInfo
{
    private readonly PSExecutor _psExecutor = new();

    public Dictionary<string, Func<Task<PSOutput>>> AvailableOptions { get; } = new();

    public ActiveDirectoryInfo()
    {
        AvailableOptions.Add("Get user on domain", GetADUsers);
        AvailableOptions.Add("Get computers on domain", GetADComputers);
        AvailableOptions.Add("Get IPv4 of each system on domain", GetADIPv4Addresses);
        AvailableOptions.Add("Get IPv6 of each system on domain", GetADIPv6Addresses);
    }

    // ReSharper disable once InconsistentNaming
    private async Task<PSOutput> GetADUsers()
    {
        Command psCommand = new("Get-ADUser");
        psCommand.Parameters.Add("Filter", "*");
        return await _psExecutor.ExecuteAsync(psCommand);
    }

    // ReSharper disable once InconsistentNaming
    private async Task<PSOutput> GetADComputers()
    {
        Command psCommand = new("Get-ADComputer");
        psCommand.Parameters.Add("Filter", "*");
        return await _psExecutor.ExecuteAsync(psCommand);
    }

    // ReSharper disable once InconsistentNaming
    private async Task<PSOutput> GetADIPv4Addresses()
    {
        Command psCommand = new("Get-ADComputer");
        psCommand.Parameters.Add("Filter", "*");
        psCommand.Parameters.Add("Properties", "IPv4Address");
        return await _psExecutor.ExecuteAsync(psCommand);
    }

    // ReSharper disable once InconsistentNaming
    private async Task<PSOutput> GetADIPv6Addresses()
    {
        Command psCommand = new("Get-ADComputer");
        psCommand.Parameters.Add("Filter", "*");
        psCommand.Parameters.Add("Properties", "IPv6Address");
        return await _psExecutor.ExecuteAsync(psCommand);
    }
}
