namespace Needle.Infrastructure.Persistence;

using System.Text.Json;

internal sealed class JsonFileStore<T>
    where T : class
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _path;

    public JsonFileStore(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = path;
    }

    public async ValueTask<T?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_path))
        {
            return null;
        }

        try
        {
            await using var stream = File.OpenRead(_path);
            return await JsonSerializer.DeserializeAsync<T>(stream, SerializerOptions, cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    public async ValueTask SaveAsync(T value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);

        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_path);
        await JsonSerializer.SerializeAsync(stream, value, SerializerOptions, cancellationToken);
    }
}
