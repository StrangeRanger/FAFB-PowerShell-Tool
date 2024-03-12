using ActiveDirectoryQuerier.PowerShell;

namespace ActiveDirectoryQuerier.Tests;

// ReSharper disable once InconsistentNaming
public class PSOutputTest
{
    [Fact]
    public void HadErrors_StdErrHasEntries_ReturnsTrue()
    {
        // Arrange
        PSOutput psOutput = new();
        psOutput.StdErr.Add("Error");

        // Act
        bool result = psOutput.HadErrors;

        // Assert
        Assert.True(result);
        Assert.NotEmpty(psOutput.StdErr);
        Assert.Empty(psOutput.StdOut);
    }

    [Fact]
    public void NoErrors_StdOutHasEntries_ReturnsFalse()
    {
        // Arrange
        PSOutput psOutput = new();
        psOutput.StdOut.Add("Output");

        // Act
        var result = psOutput.HadErrors;

        // Assert
        Assert.False(result);
        Assert.Empty(psOutput.StdErr);
        Assert.NotEmpty(psOutput.StdOut);
    }
}
