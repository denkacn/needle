namespace Needle.Application.Documents;

public interface ILogDocumentSessionFactory
{
    Task<LogDocumentSessionOpenResult> OpenAsync(
        string path,
        Action<LogDocumentSessionOpenProgress>? progress,
        CancellationToken cancellationToken);
}

public sealed record LogDocumentSessionOpenResult(
    LogDocumentSession Session,
    string DisplayName,
    long FileSize,
    string EncodingDisplayName);

public sealed record LogDocumentSessionOpenProgress(
    string DisplayName,
    long FileSize,
    string EncodingDisplayName);
