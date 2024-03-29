using ActiveDirectoryQuerier.PowerShell;
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertConstructorToMemberInitializers

namespace ActiveDirectoryQuerier.Tests;

public class PSOutputTest
{
    private readonly PSOutput _psOutput;

    public PSOutputTest()
    {
        // Arrange
        _psOutput = new PSOutput();
    }

    [Fact]
    public void HadErrors_StdErrHasEntries_ReturnsTrue()
    {
        // Arrange
        _psOutput.StdErr.Add("Error");

        // Act
        bool result = _psOutput.HadErrors;

        // Assert
        Assert.True(result);
        Assert.NotEmpty(_psOutput.StdErr);
        Assert.Empty(_psOutput.StdOut);
    }

    [Fact]
    public void NoErrors_StdOutHasEntries_ReturnsFalse()
    {
        // Arrange
        _psOutput.StdOut.Add("Output");

        // Act
        var result = _psOutput.HadErrors;

        // Assert
        Assert.False(result);
        Assert.Empty(_psOutput.StdErr);
        Assert.NotEmpty(_psOutput.StdOut);
    }
}
