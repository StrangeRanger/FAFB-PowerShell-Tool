using System.Management.Automation.Runspaces;
using ActiveDirectoryQuerier.ActiveDirectory;
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertConstructorToMemberInitializers

namespace ActiveDirectoryQuerier.Tests;

public class ADCommandParametersTests
{
    private readonly ADCommandParameters _adCommandParameters;

    public ADCommandParametersTests()
    {
        // Arrange
        _adCommandParameters = new ADCommandParameters();
    }

    [Fact]
    public void AvailableParameters_AvailableParametersNotPopulated_NoValidCommandProvided()
    {
        // Assert
        Assert.Contains("No valid command provided", _adCommandParameters.AvailableParameters);
    }

    [Fact]
    public async Task LoadAvailableParametersAsync_PopulatesAvailableParameters_IsNotEmpty()
    {
        // Arrange
        Command command = new("Get-Process");

        // Act
        await _adCommandParameters.LoadAvailableParametersAsync(command);

        // Assert
        Assert.NotEmpty(_adCommandParameters.AvailableParameters);
    }

    [Fact]
    public async Task LoadAvailableParametersAsync_CheckAvailableParameters_ContainsNameAndId()
    {
        // Arrange
        Command command = new("Get-Process");

        // Act
        await _adCommandParameters.LoadAvailableParametersAsync(command);

        // Assert
        Assert.Contains("-Name", _adCommandParameters.AvailableParameters);
        Assert.Contains("-Id", _adCommandParameters.AvailableParameters);
    }
}
