using System.Windows;
using ActiveDirectoryQuerier.MessageBoxService;
using ActiveDirectoryQuerier.ViewModels;
using Moq;

namespace ActiveDirectoryQuerier.Tests;

public class MainWindowViewModelTests
{
    private readonly MainWindowViewModel _viewModel;
    private readonly Mock<IMessageBoxService> _messageBoxServiceMock;

    public MainWindowViewModelTests()
    {
        // Arrange
        _messageBoxServiceMock = new Mock<IMessageBoxService>();
        _viewModel = new MainWindowViewModel { MessageBoxService = _messageBoxServiceMock.Object };
    }

    [Fact]
    public void ClearConsoleOutput_WhenConsoleIsEmpty_ShowsInformationMessage()
    {
        _viewModel.ClearConsoleOutput(new ConsoleViewModel());

        _messageBoxServiceMock.Verify(
            m => m.Show(It.IsAny<string>(), "Information", MessageBoxButton.OK, MessageBoxImage.Information),
            Times.Once);
    }

    [Fact]
    public void ClearConsoleOutput_WhenConsoleIsNotEmpty_ShowsWarningMessage()
    {
        var consoleViewModel = new ConsoleViewModel();
        consoleViewModel.Append("Some text");

        _viewModel.ClearConsoleOutput(consoleViewModel);

        _messageBoxServiceMock.Verify(m => m.Show(It.IsAny<string>(),
                                                  "Warning",
                                                  MessageBoxButton.YesNo,
                                                  MessageBoxImage.Warning,
                                                  MessageBoxResult.No),
                                      Times.Once);
    }

    [Fact]
    public void ExecuteSelectedQueryInADInfo_SelectedQueryInActiveDirectoryInfoIsNull_ShowWarningMessage()
    {
        // sdf
    }

    [Fact]
    public void EditQueryFromQueryStackPanel_NullParameter_ShowsMessageBox()
    {
        // Act
        _viewModel.EditQueryFromQueryStackPanel(null);

        // Assert
        _messageBoxServiceMock.Verify(
            x => x.Show(It.IsAny<string>(), "Error", MessageBoxButton.OK, MessageBoxImage.Error),
            Times.Once);
    }

    [Fact]
    public void DeleteQueryFromQueryStackPanel_WhenNoQuerySelected_ShowsWarningMessage()
    {
        _viewModel.DeleteQueryFromQueryStackPanel(null);

        _messageBoxServiceMock.Verify(
            m => m.Show(It.IsAny<string>(), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning),
            Times.Once);
    }

    [Fact]
    public void RemoveParameterComboBoxInQueryBuilder_WhenNoParametersToRemove_ShowsWarningMessage()
    {
        _viewModel.RemoveParameterComboBoxInQueryBuilder(null);

        _messageBoxServiceMock.Verify(
            m => m.Show(It.IsAny<string>(), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning),
            Times.Once);
    }

    [Fact]
    public void SaveCurrentQuery_WhenNoCommandSelected_ShowsWarningMessage()
    {
        _viewModel.SaveCurrentQuery(null);

        _messageBoxServiceMock.Verify(
            m => m.Show(It.IsAny<string>(), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning),
            Times.Once);
    }

    [Fact]
    public void AddParameterComboBoxInQueryBuilder_WhenNoCommandSelected_ShowsWarningMessage()
    {
        _viewModel.AddParameterComboBoxInQueryBuilder(null);

        _messageBoxServiceMock.Verify(
            m => m.Show(It.IsAny<string>(), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning),
            Times.Once);
    }

    [Fact]
    public void ClearQueryBuilder_WhenQueryBuilderIsEmpty_ShowsInformationMessage()
    {
        _viewModel.ClearQueryBuilder(null);

        _messageBoxServiceMock.Verify(
            m => m.Show(It.IsAny<string>(), "Information", MessageBoxButton.OK, MessageBoxImage.Information),
            Times.Once);
    }
}
