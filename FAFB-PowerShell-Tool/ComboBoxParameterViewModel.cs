using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FAFB_PowerShell_Tool;

/// <summary>
/// ...
/// </summary>
public class ComboBoxParameterViewModel : INotifyPropertyChanged
{

    public event PropertyChangedEventHandler? PropertyChanged;

    private ObservableCollection<string> _possibleParameterList;
    public ObservableCollection<string> PossibleParameterList
    {
        get => _possibleParameterList;
        set
        {
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
    /// <param name="possibleParameterList">List of possible parameters for the corresponding command.</param>
    public ComboBoxParameterViewModel(ObservableCollection<string> possibleParameterList)
    {
        PossibleParameterList = possibleParameterList;
    }

    /// <summary>
    /// This is the method that is called when a property is changed.
    /// </summary>
    /// <param name="propertyName">Property that changed.</param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

