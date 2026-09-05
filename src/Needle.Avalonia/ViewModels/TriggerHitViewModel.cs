namespace Needle.Avalonia.ViewModels;

public sealed record TriggerHitViewModel(
    long LineNumber,
    string Pattern,
    string Text)
{
    public string DisplayText => $"{LineNumber + 1:N0}: {Pattern}";
}
