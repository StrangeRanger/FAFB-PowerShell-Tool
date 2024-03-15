using ActiveDirectoryQuerier.ViewModels;

namespace ActiveDirectoryQuerier.Tests;

public class RelayCommandTests
{
    [Fact]
    public void CanExecute_WithNullPredicate_ReturnsTrue()
    {
        RelayCommand command = new(
            _ =>
            {});
        Assert.True(command.CanExecute(null));
    }

    [Fact]
    public void CanExecute_WithNonNullPredicate_ReturnsTrue()
    {
        RelayCommand command = new(
            _ =>
            {},
            _ => true);
        Assert.True(command.CanExecute(null));
    }

    [Fact]
    public void CanExecute_WithNonNullPredicate_ReturnsFalse()
    {
        RelayCommand command = new(
            _ =>
            {},
            _ => false);
        Assert.False(command.CanExecute(null));
    }

    [Fact]
    public void Execute_WithNullParameter_Executes()
    {
        bool executed = false;
        RelayCommand command = new(
            _ => executed = true);
        command.Execute(null);
        Assert.True(executed);
    }

    [Fact]
    public void Execute_WithNonNullParameter_Executes()
    {
        bool executed = false;
        RelayCommand command = new(
            _ => executed = true);
        command.Execute("test");
        Assert.True(executed);
    }

    [Fact]
    public void RaiseCanExecuteChanged_WithNullParameter_DoesNotThrow()
    {
        RelayCommand command = new(
            _ =>
            {});
        command.RaiseCanExecuteChanged();
    }

    [Fact]
    public void CanExecuteChanged_WithNullParameter_DoesNotThrow()
    {
        RelayCommand command = new(
            _ =>
            {});
        command.CanExecuteChanged += (_, _) =>
        {};
    }

    [Fact]
    public void CanExecuteChanged_WithNullParameter_DoesNotThrow2()
    {
        RelayCommand command = new(
            _ =>
            {});
        // ReSharper disable once EventUnsubscriptionViaAnonymousDelegate
        command.CanExecuteChanged -= (_, _) =>
        {};
    }
}
