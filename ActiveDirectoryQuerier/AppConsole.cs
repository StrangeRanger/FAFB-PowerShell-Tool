using System.ComponentModel;
using System.IO;

namespace ActiveDirectoryQuerier;

// Class that contains app console
public sealed class AppConsole : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private string _consoleOutput = string.Empty;

    /// <summary></summary>
    /// <important>
    /// Do not set this property directly. Use the Append method instead. It's public for data binding purposes.
    /// </important>
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

    // Clears console output
    public void Clear()
    {
        ConsoleOutput = string.Empty;
    }

    // Appends multiple things to console output
    public void Append(List<string> outputText)
    {
        ConsoleOutput += string.Join(Environment.NewLine, outputText);
    }

    // Appends console output (standard)
    public void Append(string outputText)
    {
        ConsoleOutput += outputText;
    }

    // Exports console output to text file
    public void ExportToText(string filePath = "output.txt")
    {
        File.WriteAllText(filePath, ConsoleOutput);
    }

    // Runs when a property is changed
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
