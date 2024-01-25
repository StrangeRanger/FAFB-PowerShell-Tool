using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FAFB_PowerShell_Tool;

/// <summary>
/// This class implements a custom ComboBox that is used to display the possible parameters for a command. This is
/// specifically used when the user adds a new parameter slot to a command.
/// </summary>
public sealed class ComboBoxParameterViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private ObservableCollection<string> _possibleParameterList;
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
    /// Class constructor.
    /// </summary>
    /// <todo>Ensure that _possibleParameterList is not null.</todo>
    /// <param name="possibleParameterList">List of possible parameters for the corresponding command.</param>
    public ComboBoxParameterViewModel(ObservableCollection<string> possibleParameterList)
    {
        PossibleParameterList = possibleParameterList;
    }

    /// <summary>
    /// This is the method that is called when a property is changed.
    /// </summary>
    /// <param name="propertyName">Property that changed.</param>
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

