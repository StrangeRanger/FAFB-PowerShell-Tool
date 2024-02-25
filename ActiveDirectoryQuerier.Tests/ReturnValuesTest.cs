using ActiveDirectoryQuerier.PowerShell;

namespace ActiveDirectoryQuerier.Tests;

public class ReturnValuesTest
{
    [Fact]
    public void HadErrors_StdErrHasEntries_ReturnsTrue()
    {
        // Arrange
        var returnValues = new ReturnValues();
        returnValues.StdErr.Add("Error");

        // Act
        var result = returnValues.HadErrors;

        // Assert
        Assert.True(result);
        Assert.NotEmpty(returnValues.StdErr);
        Assert.Empty(returnValues.StdOut);
    }

    [Fact]
    public void NoErrors_StdOutHasEntries_ReturnsFalse()
    {
        // Arrange
        var returnValues = new ReturnValues();
        returnValues.StdOut.Add("Output");

        // Act
        var result = returnValues.HadErrors;

        // Assert
        Assert.False(result);
        Assert.Empty(returnValues.StdErr);
        Assert.NotEmpty(returnValues.StdOut);
    }
}
