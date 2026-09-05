using CommunityToolkit.Mvvm.ComponentModel;

namespace Needle.Avalonia.ViewModels;

public sealed class TriggerRuleViewModel : ObservableObject
{
    private string _pattern;

    public TriggerRuleViewModel(string pattern, Action<TriggerRuleViewModel> remove)
    {
        _pattern = pattern;
        RemoveCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(() => remove(this));
    }

    public string Pattern
    {
        get => _pattern;
        set => SetProperty(ref _pattern, value);
    }

    public CommunityToolkit.Mvvm.Input.IRelayCommand RemoveCommand { get; }
}
