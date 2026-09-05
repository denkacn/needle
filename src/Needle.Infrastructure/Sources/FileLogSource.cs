namespace Needle.Infrastructure.Sources;

using Needle.Core.Sources;

public sealed class FileLogSource : ILogSource
{
    private const int FingerprintByteCount = 4096;

    public FileLogSource(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Path = System.IO.Path.GetFullPath(path);
        DisplayName = System.IO.Path.GetFileName(Path);
    }

    public string Path { get; }

    public string DisplayName { get; }

    public FileSourceIdentity GetIdentity(int? fingerprintByteCount = null)
    {
        var info = new FileInfo(Path);
        var (prefixLength, prefixHash) = GetPrefixFingerprint(fingerprintByteCount ?? FingerprintByteCount);
        return new FileSourceIdentity(info.FullName, info.CreationTimeUtc, prefixLength, prefixHash);
    }

    public DateTime GetLastWriteTimeUtc()
    {
        return new FileInfo(Path).LastWriteTimeUtc;
    }

    public ValueTask<long> GetLengthAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(new FileInfo(Path).Length);
    }

    public ValueTask<Stream> OpenReadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Stream stream = new FileStream(
            Path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            bufferSize: 128 * 1024,
            FileOptions.SequentialScan | FileOptions.Asynchronous);

        return ValueTask.FromResult(stream);
    }

    private (int Length, ulong Hash) GetPrefixFingerprint(int fingerprintByteCount)
    {
        fingerprintByteCount = Math.Clamp(fingerprintByteCount, 0, FingerprintByteCount);
        Span<byte> buffer = stackalloc byte[FingerprintByteCount];
        using var stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        var bytesRead = fingerprintByteCount == 0 ? 0 : stream.Read(buffer[..fingerprintByteCount]);
        var hash = 14695981039346656037UL;
        for (var i = 0; i < bytesRead; i++)
        {
            hash ^= buffer[i];
            hash *= 1099511628211UL;
        }

        return (bytesRead, hash);
    }
}

public readonly record struct FileSourceIdentity(
    string FullPath,
    DateTime CreationTimeUtc,
    int PrefixLength,
    ulong PrefixHash);
