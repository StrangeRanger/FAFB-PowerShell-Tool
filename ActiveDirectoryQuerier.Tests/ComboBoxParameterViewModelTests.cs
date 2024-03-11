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
        await adCommandParameters.LoadParametersAsync(command);
        comboBoxParameterViewModel = new(adCommandParameters.AvailableParameters);
        
        // Assert
        Assert.NotNull(comboBoxParameterViewModel.PossibleParameters);
        Assert.NotEmpty(comboBoxParameterViewModel.PossibleParameters);
    }
    
    [Fact]
    public async Task ComboBoxParameterViewModel_WhenConstructed_PossibleParametersContainsExpectedParameters()
    {
        // Arrange
        Command command = new("Get-ADUser");
        ADCommandParameters adCommandParameters = new();
        ComboBoxParameterViewModel comboBoxParameterViewModel;
        
        // Act
        await adCommandParameters.LoadParametersAsync(command);
        comboBoxParameterViewModel = new(adCommandParameters.AvailableParameters);
        
        // Assert
        Assert.Contains(comboBoxParameterViewModel.PossibleParameters, param => param == "-Filter");
        Assert.Contains(comboBoxParameterViewModel.PossibleParameters, param => param == "-Identity");
        Assert.Contains(comboBoxParameterViewModel.PossibleParameters, param => param == "-LDAPFilter");
        Assert.Contains(comboBoxParameterViewModel.PossibleParameters, param => param == "-SearchBase");
        Assert.Contains(comboBoxParameterViewModel.PossibleParameters, param => param == "-SearchScope");
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
        await adCommandParameters.LoadParametersAsync(command);
        comboBoxParameterViewModel = new(adCommandParameters.AvailableParameters);
        selectedParameter = comboBoxParameterViewModel.PossibleParameters[0];
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