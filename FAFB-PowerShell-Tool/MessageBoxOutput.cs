using System.Windows;

namespace FAFB_PowerShell_Tool;

/// <summary>
/// Provides a utility class for displaying MessageBoxes with different types of outputs.
/// </summary>
public static class MessageBoxOutput
{
    /// <summary>
    /// Defines the type of output to display in the MessageBox.
    /// </summary>
    public enum OutputType
    {
        Error,
        Generic
    }

    /// <summary>
    /// Shows a MessageBox with a specified message and output type.
    /// </summary>
    /// <param name="message">The message to be displayed in the MessageBox.</param>
    /// <param name="type">The type of MessageBox to display, defaulted to Generic.</param>
    public static void Show(string message, OutputType type = OutputType.Generic)
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
}
