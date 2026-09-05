namespace Needle.Avalonia.ViewModels;

using CommunityToolkit.Mvvm.Input;

public sealed class ExcludeRuleViewModel : ViewModelBase
{
    private readonly Action<ExcludeRuleViewModel> _remove;

    public ExcludeRuleViewModel(string pattern, Action<ExcludeRuleViewModel> remove)
    {
        Pattern = pattern;
        _remove = remove;
        RemoveCommand = new RelayCommand(Remove);
    }

    public string Pattern { get; }

    public IRelayCommand RemoveCommand { get; }

    private void Remove()
    {
        _remove(this);
    }
}
