using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace FAFB_PowerShell_Tool;

/// <summary>
/// ...
/// </summary>
public class ComboBoxParameterViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// This is the event handler for the INotifyPropertyChanged interface.
    /// </summary>
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

    // Constructor
    public ComboBoxParameterViewModel(ObservableCollection<string> possibleParameterList)
    {
        PossibleParameterList = possibleParameterList;
    }

    /// <summary>
    /// This is the method that is called when a property is changed.
    /// </summary>
    /// <param name="propertyName">...</param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

