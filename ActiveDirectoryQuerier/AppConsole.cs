using System.ComponentModel;

namespace ActiveDirectoryQuerier;

/// <summary>
/// Represents the console output.
/// </summary>
public sealed class AppConsole : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private string _consoleOutput = string.Empty;

    /// <summary>
    /// Gets or sets the console output.
    /// </summary>
    /// <important>
    /// Do not set this property directly. Use the <see cref="Append"/> method instead. It's public for data binding
    /// purposes.
    /// </important>
    public string ConsoleOutput
    {
        get => _consoleOutput;
    [EditorBrowsable(EditorBrowsableState.Never)]
        set {
            if (_consoleOutput != value)
            {
                _consoleOutput = value;
                OnPropertyChanged(nameof(ConsoleOutput));
            }
        }
    }

    /// <summary>
    /// Clears the console output.
    /// </summary>
    public void ClearConsole()
    {
        ConsoleOutput = string.Empty;
    }

    /// <summary>
    /// Appends the output text to the console output.
    /// </summary>
    /// <param name="outputText">The output text to append.</param>
    public void Append(List<string> outputText)
    {
        ConsoleOutput += string.Join(Environment.NewLine, outputText);
    }

    /// <summary>
    /// Appends the output text to the console output.
    /// </summary>
    /// <param name="outputText">The output text to append.</param>
    public void Append(string outputText)
    {
        ConsoleOutput += outputText;
    }

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
