using ActiveDirectoryQuerier.PowerShell;
// ReSharper disable ConvertConstructorToMemberInitializers

namespace ActiveDirectoryQuerier.Tests;

/// <remarks>
/// Since ActiveDirectoryInfo executes Active Directory commands, and it's not guaranteed that the tests will be
/// executed on an Active Directory domain, the output will vary in location (StdOut or StdErr) and content. Therefore,
/// it's more important to test that something is returned rather than the actual contents of the output.
/// </remarks>
public class ActiveDirectoryInfoTests
{
    private readonly ActiveDirectoryInfo _adInfo = new();

    [Fact]
    public async Task GetADUsers_ReturnsExpectedOutput()
    {
        // Act
        PSOutput result = await _adInfo.AvailableOptions["Get user on domain"]();

        // Assert
        Assert.NotNull(result);

        if (result.HadErrors)
        {
            Assert.True(result.StdErr.Count > 0);
        }
        else
        {
            Assert.True(result.StdOut.Count > 0);
        }
    }

    [Fact]
    public async Task GetADComputers_ReturnsExpectedOutput()
    {
        // Act
        PSOutput result = await _adInfo.AvailableOptions["Get computers on domain"]();

        // Assert
        Assert.NotNull(result);

        if (result.HadErrors)
        {
            Assert.True(result.StdErr.Count > 0);
        }
        else
        {
            Assert.True(result.StdOut.Count > 0);
        }
    }

    [Fact]
    public async Task GetADIPv4Addresses_ReturnsExpectedOutput()
    {
        // Act
        PSOutput result = await _adInfo.AvailableOptions["Get IPv4 of each system on domain"]();

        // Assert
        Assert.NotNull(result);

        if (result.HadErrors)
        {
            Assert.True(result.StdErr.Count > 0);
        }
        else
        {
            Assert.True(result.StdOut.Count > 0);
        }
    }

    [Fact]
    public async Task GetADIPv6Addresses_ReturnsExpectedOutput()
    {
        // Act
        PSOutput result = await _adInfo.AvailableOptions["Get IPv6 of each system on domain"]();

        // Assert
        Assert.NotNull(result);

        if (result.HadErrors)
        {
            Assert.True(result.StdErr.Count > 0);
        }
        else
        {
            Assert.True(result.StdOut.Count > 0);
        }
    }
}
