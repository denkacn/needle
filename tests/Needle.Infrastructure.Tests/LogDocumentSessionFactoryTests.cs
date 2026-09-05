namespace Needle.Infrastructure.Tests;

using System.Text;
using Needle.Infrastructure.Documents;

public sealed class LogDocumentSessionFactoryTests
{
    [Fact]
    public async Task OpenAsyncCreatesReadableSession()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.log");
        await File.WriteAllTextAsync(path, "one\ntwo\n", Encoding.UTF8);

        try
        {
            var factory = new LogDocumentSessionFactory();
            var result = await factory.OpenAsync(path, progress: null, CancellationToken.None);

            Assert.Equal(System.IO.Path.GetFileName(path), result.DisplayName);
            Assert.Equal(2, result.Session.LineCount);

            var lines = await result.Session.Reader.GetLinesAsync(result.Session.Document, 0, 2);

            Assert.Equal(["one", "two"], lines.Select(line => line.Text));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
