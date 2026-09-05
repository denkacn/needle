using Needle.Application.Documents;

namespace Needle.Avalonia.ViewModels;

internal sealed class ActiveLogDocument
{
    public string? Path { get; private set; }
    public LogDocumentSession? Session { get; private set; }
    public LogTabViewModel? Tab { get; private set; }

    public bool HasTab => Tab is not null;

    public void Activate(string path, LogTabViewModel tab)
    {
        Path = path;
        Tab = tab;
        Session = tab.Session;
    }

    public void SwitchTo(LogTabViewModel tab)
    {
        Path = tab.Path;
        Tab = tab;
        Session = tab.Session;
    }

    public void Clear()
    {
        Path = null;
        Session = null;
        Tab = null;
    }

    public bool IsActive(LogTabViewModel? tab)
    {
        return ReferenceEquals(tab, Tab);
    }
}
