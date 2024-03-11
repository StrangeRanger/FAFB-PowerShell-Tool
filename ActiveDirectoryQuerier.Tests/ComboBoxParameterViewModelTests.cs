using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;
using ActiveDirectoryQuerier.PowerShell;

namespace ActiveDirectoryQuerier.Tests;

public class ComboBoxParameterViewModelTests
{
    [Fact]
    public async Task ComboBoxParameterViewModel_WhenConstructed_PossibleParametersIsNotNullOrEmpty()
    {
        // Arrange
        Command command = new("Get-ADUser");
        ADCommandParameters adCommandParameters = new();
        ComboBoxParameterViewModel comboBoxParameterViewModel;

        // Act
        await adCommandParameters.LoadAvailableParametersAsync(command);
        comboBoxParameterViewModel = new(adCommandParameters.AvailableParameters);

        // Assert
        Assert.NotNull(comboBoxParameterViewModel.AvailableParameters);
        Assert.NotEmpty(comboBoxParameterViewModel.AvailableParameters);
    }

    [Fact]
    public async Task ComboBoxParameterViewModel_WhenConstructed_PossibleParametersContainsExpectedParameters()
    {
        // Arrange
        Command command = new("Get-ADUser");
        ADCommandParameters adCommandParameters = new();
        ComboBoxParameterViewModel comboBoxParameterViewModel;

        // Act
        await adCommandParameters.LoadAvailableParametersAsync(command);
        comboBoxParameterViewModel = new(adCommandParameters.AvailableParameters);

        // Assert
        Assert.Contains(comboBoxParameterViewModel.AvailableParameters, param => param == "-Filter");
        Assert.Contains(comboBoxParameterViewModel.AvailableParameters, param => param == "-Identity");
        Assert.Contains(comboBoxParameterViewModel.AvailableParameters, param => param == "-LDAPFilter");
        Assert.Contains(comboBoxParameterViewModel.AvailableParameters, param => param == "-SearchBase");
        Assert.Contains(comboBoxParameterViewModel.AvailableParameters, param => param == "-SearchScope");
    }

    [Fact]
    public async Task ComboBoxParameterViewModel_WhenSelectedParameterChosen_SelectedParameterIsSet()
    {
        // Arrange
        string selectedParameter;
        Command command = new("Get-ADUser");
        ADCommandParameters adCommandParameters = new();
        ComboBoxParameterViewModel comboBoxParameterViewModel;

        // Act
        await adCommandParameters.LoadAvailableParametersAsync(command);
        comboBoxParameterViewModel = new(adCommandParameters.AvailableParameters);
        selectedParameter = comboBoxParameterViewModel.AvailableParameters[0];
        comboBoxParameterViewModel.SelectedParameter = selectedParameter;

        // Assert
        Assert.Equal(selectedParameter, comboBoxParameterViewModel.SelectedParameter);
    }

    [Fact]
    public void ComboBoxParameterViewModel_WhenConstructedWithNullPossibleParameters_ThrowsArgumentNullException()
    {
        // Arrange
        ObservableCollection<string> possibleParameters = null!;

        // Assert
        Assert.Throws<ArgumentNullException>(() => new ComboBoxParameterViewModel(possibleParameters));
    }
}
