using System.ComponentModel;
using System.IO;

namespace ActiveDirectoryQuerier;

public sealed class AppConsole : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private string _consoleOutput = string.Empty;

    /// <summary></summary>
    /// <important>
    /// Do not set this property directly. Use the Append method instead. It's public for data binding purposes.
    /// </important>
    /// TODO: Find a way to make the set of this property private.
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

    public void Clear()
    {
        ConsoleOutput = string.Empty;
    }

    public void Append(List<string> outputText)
    {
        ConsoleOutput += string.Join(Environment.NewLine, outputText);
    }

    public void Append(string outputText)
    {
        ConsoleOutput += outputText;
    }

    public void ExportToText(string filePath = "output.txt")
    {
        File.WriteAllText(filePath, ConsoleOutput);
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
