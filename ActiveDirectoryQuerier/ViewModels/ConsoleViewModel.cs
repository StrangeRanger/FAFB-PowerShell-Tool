using System.ComponentModel;
using System.IO;

namespace ActiveDirectoryQuerier.ViewModels;

public sealed class ConsoleViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private string _consoleOutput = string.Empty;

    // Setting this as private to prevent external modification.
    private string ConsoleOutput
    {
        get => _consoleOutput;
        set {
            if (_consoleOutput != value)
            {
                _consoleOutput = value;
                OnPropertyChanged(nameof(GetConsoleOutput));
            }
        }
    }

    // Using a getter-only property to ensure that one-way data binding to the console output is still possible, without
    // exposing the setter of the ConsoleOutput property.
    public string GetConsoleOutput => ConsoleOutput;

    public void Clear()
    {
        ConsoleOutput = string.Empty;
    }

    public void Append(IEnumerable<string> outputText)
    {
        ConsoleOutput += string.Join(Environment.NewLine, outputText);
    }

    public void Append(string outputText)
    {
        ConsoleOutput += outputText + Environment.NewLine;
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
