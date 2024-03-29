using ActiveDirectoryQuerier.ViewModels;

namespace ActiveDirectoryQuerier.Tests;

public class TextBoxViewModelTests
{
    [Fact]
    public void TextBoxViewModel_WhenConstructed_SelectedParameterValueIsNotNullOrEmpty()
    {
        // Arrange & Act
        TextBoxViewModel textBoxViewModel = new() { SelectedParameterValue = "*" };

        // Assert
        Assert.NotNull(textBoxViewModel.SelectedParameterValue);
        Assert.NotEmpty(textBoxViewModel.SelectedParameterValue);
    }

    [Fact]
    public void TextBoxViewModel_WhenSetToNull_NoExceptionIsThrown()
    {
        // Arrange & Act
        TextBoxViewModel textBoxViewModel = new() { SelectedParameterValue = null! };

        // Assert
        Assert.Null(textBoxViewModel.SelectedParameterValue);
    }
}
