using System.Windows;
using ActiveDirectoryQuerier.MessageBoxService;
using Moq;

namespace ActiveDirectoryQuerier.Tests;

public class MainWindowViewModelTests
{
    [Fact]
    public void EditQueryFromQueryStackPanel_NullParameter_ShowsMessageBox()
    {
        // Arrange
        var mockMessageBoxService = new Mock<IMessageBoxService>();
        var viewModel = new MainWindowViewModel { MessageBoxService = mockMessageBoxService.Object };

        // Act
        viewModel.EditQueryFromQueryStackPanel(null);

        // Assert
        mockMessageBoxService.Verify(
            x => x.Show(It.IsAny<string>(), "Error", MessageBoxButton.OK, MessageBoxImage.Error),
            Times.Once);
    }
}
