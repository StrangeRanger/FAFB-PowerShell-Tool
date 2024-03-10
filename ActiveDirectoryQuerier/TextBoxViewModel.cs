using System.ComponentModel;

namespace ActiveDirectoryQuerier;

/// <summary>
/// ViewModel for a TextBox that contains the value of a parameter slot.
/// It is used to bind the TextBox to the ViewModel.
/// </summary>
public sealed class TextBoxViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private string _selectedParameterValue = string.Empty;

    /// <summary>
    /// Gets or sets the selected parameter value for the unique ComboBox.
    /// </summary>
    public string SelectedParameterValue
    {
        get => _selectedParameterValue;
        set {
            if (_selectedParameterValue != value)
            {
                _selectedParameterValue = value;
                OnPropertyChanged(nameof(SelectedParameterValue));
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
