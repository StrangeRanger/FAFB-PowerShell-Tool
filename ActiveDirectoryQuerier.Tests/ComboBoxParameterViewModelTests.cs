using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;
using ActiveDirectoryQuerier.ActiveDirectory;
using ActiveDirectoryQuerier.ViewModels;
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable ConvertConstructorToMemberInitializers

namespace ActiveDirectoryQuerier.Tests;

public class ComboBoxParameterViewModelTests
{
    private readonly Command _command;
    private readonly ADCommandParameters _adCommandParameters;

    public ComboBoxParameterViewModelTests()
    {
        // Arrange
        _command = new Command("Get-ADUser");
        _adCommandParameters = new ADCommandParameters();
    }

    [Fact]
    public async Task ComboBoxParameterViewModel_WhenConstructed_PossibleParametersIsNotNullOrEmpty()
    {
        // Arrange
        ComboBoxParameterViewModel comboBoxParameterViewModel;

        // Act
        await _adCommandParameters.LoadAvailableParametersAsync(_command);
        comboBoxParameterViewModel = new ComboBoxParameterViewModel(_adCommandParameters.AvailableParameters);

        // Assert
        Assert.NotNull(comboBoxParameterViewModel.AvailableParameters);
        Assert.NotEmpty(comboBoxParameterViewModel.AvailableParameters);
    }

    [Fact]
    public async Task ComboBoxParameterViewModel_WhenConstructed_PossibleParametersContainsExpectedParameters()
    {
        // Arrange
        ComboBoxParameterViewModel comboBoxParameterViewModel;

        // Act
        await _adCommandParameters.LoadAvailableParametersAsync(_command);
        comboBoxParameterViewModel = new ComboBoxParameterViewModel(_adCommandParameters.AvailableParameters);

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
        ComboBoxParameterViewModel comboBoxParameterViewModel;

        // Act
        await _adCommandParameters.LoadAvailableParametersAsync(_command);
        comboBoxParameterViewModel = new ComboBoxParameterViewModel(_adCommandParameters.AvailableParameters);
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
