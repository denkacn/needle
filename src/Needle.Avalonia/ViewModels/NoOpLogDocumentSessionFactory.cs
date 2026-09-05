namespace Needle.Avalonia.ViewModels;

using Needle.Application.Documents;

internal sealed class NoOpLogDocumentSessionFactory : ILogDocumentSessionFactory
{
    public Task<LogDocumentSessionOpenResult> OpenAsync(
        string path,
        Action<LogDocumentSessionOpenProgress>? progress,
        CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("No log document session factory is configured.");
    }
}
