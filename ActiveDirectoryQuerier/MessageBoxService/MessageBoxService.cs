using System.Windows;

namespace ActiveDirectoryQuerier.MessageBoxService;

public class MessageBoxService : IMessageBoxService
{
    public void Show(string message, string caption, MessageBoxButton button, MessageBoxImage icon)
    {
        MessageBox.Show(message, caption, button, icon);
    }

    public MessageBoxResult Show(string message, string caption, MessageBoxButton button, MessageBoxImage icon,
                                 MessageBoxResult result)
    {
        return MessageBox.Show(message, caption, button, icon, result);
    }
}
