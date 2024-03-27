using System.ComponentModel;
using System.IO;

namespace ActiveDirectoryQuerier.ViewModels;

public sealed class ConsoleViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private string _consoleOutput = string.Empty;
    
    /// <remarks>
    /// IMPORTANT: Do not set this property directly. Use the Append method instead. It's public for data binding
    /// purposes.
    /// </remarks>
    /// TODO: Find a way to make the set of this property private, while still allowing data binding.
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

    public void Append(IEnumerable<string> outputText)
    {
        // TODO: Add a newline character to the end of the outputText.
        ConsoleOutput += string.Join(Environment.NewLine, outputText);
    }

    public void Append(string outputText)
    {
        // TODO: Add a newline character to the end of the outputText.
        ConsoleOutput += outputText;
    }

    public void ExportToTextFile(string filePath = "output.txt")
    {
        File.WriteAllText(filePath, ConsoleOutput);
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
