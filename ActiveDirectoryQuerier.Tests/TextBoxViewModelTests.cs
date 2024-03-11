namespace ActiveDirectoryQuerier.Tests;

public class TextBoxViewModelTests
{
    [Fact]
    public void TextBoxViewModel_WhenConstructed_SelectedParameterValueIsNotNullOrEmpty()
    {
        // Arrange
        TextBoxViewModel textBoxViewModel = new() {// Act
                                                   SelectedParameterValue = "*"
        };

        // Assert
        Assert.NotNull(textBoxViewModel.SelectedParameterValue);
        Assert.NotEmpty(textBoxViewModel.SelectedParameterValue);
    }

    [Fact]
    public void TextBoxViewModel_WhenSetToNull_NoExceptionIsThrown()
    {
        // Arrange
        TextBoxViewModel textBoxViewModel = new() {// Act
                                                   SelectedParameterValue = null!
        };

        // Assert
        Assert.Null(textBoxViewModel.SelectedParameterValue);
    }
}
