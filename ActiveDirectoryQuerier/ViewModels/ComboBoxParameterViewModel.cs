using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ActiveDirectoryQuerier.ViewModels;

/// <summary>
/// ViewModel for a ComboBox that displays possible parameters for a PowerShell command.
/// It is used when adding a new parameter slot to a command in the GUI.
/// </summary>
public sealed class ComboBoxParameterViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private ObservableCollection<string> _availableParameters = new();
    private string _selectedParameter = string.Empty;

    public ComboBoxParameterViewModel(ObservableCollection<string> possibleParameters)
    {
        AvailableParameters = possibleParameters ?? throw new ArgumentNullException(nameof(possibleParameters));
    }

    public string SelectedParameter
    {
        get => _selectedParameter;
        set {
            if (_selectedParameter != value)
            {
                _selectedParameter = value;
                OnPropertyChanged(nameof(SelectedParameter));
            }
        }
    }

    public ObservableCollection<string> AvailableParameters
    {
        get => _availableParameters;
        set {
            if (_availableParameters != value)
            {
                _availableParameters = value;
                OnPropertyChanged(nameof(AvailableParameters));
            }
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
