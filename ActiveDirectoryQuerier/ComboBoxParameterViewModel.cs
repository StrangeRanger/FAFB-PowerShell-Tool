using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ActiveDirectoryQuerier;

/// <summary>
/// ViewModel for a ComboBox that displays possible parameters for a PowerShell command.
/// It is used when adding a new parameter slot to a command in the UI.
/// </summary>
public sealed class ComboBoxParameterViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private ObservableCollection<string> _possibleParameterList = new();
    private string _selectedParameter = string.Empty;

    /// <summary>
    /// Initializes a new instance of the ComboBoxParameterViewModel class.
    /// </summary>
    /// <param name="possibleParameterList">Initial list of possible parameters for the ComboBox.</param>
    public ComboBoxParameterViewModel(ObservableCollection<string> possibleParameterList)
    {
        PossibleParameterList = possibleParameterList ?? throw new ArgumentNullException(nameof(possibleParameterList));
    }

    /// <summary>
    /// Sets a unique selected value for each combo box.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the list of possible parameters that can be selected for a command.
    /// </summary>
    public ObservableCollection<string> PossibleParameterList
    {
        get => _possibleParameterList;
        set {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (_possibleParameterList != value)
            {
                _possibleParameterList = value;
                OnPropertyChanged(nameof(PossibleParameterList));
            }
        }
    }

    /// <summary>
    /// Invokes the PropertyChanged event for the specified property name.
    /// </summary>
    /// <param name="propertyName">Name of the property that has changed.</param>
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
