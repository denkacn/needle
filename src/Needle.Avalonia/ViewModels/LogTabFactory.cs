namespace Needle.Avalonia.ViewModels;

using Needle.Application.Documents;

internal sealed class LogTabFactory
{
    private readonly ILogDocumentSessionFactory _sessionFactory;

    public LogTabFactory(ILogDocumentSessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    public async Task<LogTabOpenResult> OpenAsync(
        string path,
        Action<LogTabOpenProgress>? progress,
        CancellationToken cancellationToken)
    {
        var result = await _sessionFactory.OpenAsync(
            path,
            openProgress => progress?.Invoke(new LogTabOpenProgress(
                openProgress.DisplayName,
                openProgress.FileSize,
                openProgress.EncodingDisplayName)),
            cancellationToken);

        var tab = new LogTabViewModel(path, result.Session);
        return new LogTabOpenResult(tab, result.DisplayName, result.FileSize, result.EncodingDisplayName);
    }
}

internal sealed record LogTabOpenResult(
    LogTabViewModel Tab,
    string DisplayName,
    long FileSize,
    string EncodingDisplayName);

internal sealed record LogTabOpenProgress(
    string DisplayName,
    long FileSize,
    string EncodingDisplayName);
