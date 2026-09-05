namespace Needle.Core.Documents;

using Needle.Core.Sources;

public sealed class LogDocument
{
    public LogDocument(string id, string displayName, ILogSource source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        Id = id;
        DisplayName = displayName;
        Source = source ?? throw new ArgumentNullException(nameof(source));
    }

    public string Id { get; }

    public string DisplayName { get; }

    public ILogSource Source { get; }
}
