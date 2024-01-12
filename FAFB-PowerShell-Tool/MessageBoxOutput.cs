using System.Windows;

namespace FAFB_PowerShell_Tool;

public static class MessageBoxOutput
{
    public enum OutputType
    {
        Error,
        Generic
    }

    public static void Show(string message, OutputType type)
    {
        switch (type)
        {
        case OutputType.Error:
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
