using System.Windows;

namespace FAFB_PowerShell_Tool;

public static class MessageBoxOutput
{
    public enum OutputType
    {
        Error,
        InternalError,
        Information,
        Warning,
        Generic
    }

    public static void Show(string message, OutputType type)
    {
        switch (type)
        {
        case OutputType.Error:
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            break;
        case OutputType.InternalError:
            MessageBox.Show(message, "Internal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            break;
        case OutputType.Information:
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            break;
        case OutputType.Warning:
            MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            break;
        case OutputType.Generic:
            MessageBox.Show(message, "Command Output", MessageBoxButton.OK, MessageBoxImage.None);
            break;
        default:
            throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public static void Show(string message)
    {
        Show(message, OutputType.Generic);
    }
}
