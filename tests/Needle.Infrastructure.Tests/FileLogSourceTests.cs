namespace Needle.Infrastructure.Tests;

using Needle.Infrastructure.Sources;

public sealed class FileLogSourceTests
{
    [Fact]
    public async Task OpensFileWithSharedReadWriteAccess()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.log");
        await File.WriteAllTextAsync(path, "one\ntwo\n");

        try
        {
            var source = new FileLogSource(path);

            await using var stream = await source.OpenReadAsync();

            Assert.True(stream.CanRead);
            Assert.Equal(8, await source.GetLengthAsync());
        }
        finally
        {
            File.Delete(path);
        }
    }
}
