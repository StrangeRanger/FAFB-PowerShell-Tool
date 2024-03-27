using System.Windows;

namespace ActiveDirectoryQuerier.MessageBoxService;

public interface IMessageBoxService
{
    void Show(string message, string caption, MessageBoxButton button, MessageBoxImage icon);
}
