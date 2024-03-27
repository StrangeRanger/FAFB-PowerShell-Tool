using System.Windows;

namespace ActiveDirectoryQuerier.MessageBoxService;

public class MessageBoxService : IMessageBoxService
{
    public void Show(string message, string caption, MessageBoxButton button, MessageBoxImage icon)
    {
        MessageBox.Show(message, caption, button, icon);
    }
}