using System.ComponentModel;

namespace ActiveDirectoryQuerier.ViewModels;

public sealed class TextBoxViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private string _selectedParameterValue = string.Empty;

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

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
