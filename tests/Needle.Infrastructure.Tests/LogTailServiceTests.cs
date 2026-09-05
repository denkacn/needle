namespace Needle.Infrastructure.Tests;

using System.Text;
using Needle.Core.Indexing;
using Needle.Infrastructure.Encoding;
using Needle.Infrastructure.Indexing;
using Needle.Infrastructure.Sources;
using Needle.Infrastructure.Tailing;

public sealed class LogTailServiceTests
{
    [Fact]
    public async Task PollAddsOnlyAppendedLines()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.log");
        await File.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes("one\n"));

        try
        {
            var source = new FileLogSource(path);
            var detection = await new EncodingDetector().DetectAsync(source);
            var builder = new ChunkedLineIndexBuilder(chunkSize: 4, encodingDetection: detection);
            var index = (InMemoryLineIndex)await builder.BuildAsync(source);
            var initialLength = await source.GetLengthAsync();
            var tail = new LogTailService(source, index, builder, initialLength);

            await using (var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete))
            {
                await stream.WriteAsync(Encoding.UTF8.GetBytes("two\nthree\n"));
            }

            var update = await tail.PollAsync();

            Assert.True(update.HasNewLines);
            Assert.Equal(2, update.AddedLineCount);
            Assert.Equal(3, index.LineCount);
            Assert.True(index.TryGetLine(1, out var second));
            Assert.True(index.TryGetLine(2, out var third));
            Assert.Equal((4, 3, 1), (second.Offset, second.Length, second.LineNumber));
            Assert.Equal((8, 5, 2), (third.Offset, third.Length, third.LineNumber));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task PollReportsNoChangeWhenLengthIsSame()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.log");
        await File.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes("one\n"));

        try
        {
            var source = new FileLogSource(path);
            var detection = await new EncodingDetector().DetectAsync(source);
            var builder = new ChunkedLineIndexBuilder(chunkSize: 4, encodingDetection: detection);
            var index = (InMemoryLineIndex)await builder.BuildAsync(source);
            var initialLength = await source.GetLengthAsync();
            var tail = new LogTailService(source, index, builder, initialLength);

            var update = await tail.PollAsync();

            Assert.False(update.HasNewLines);
            Assert.False(update.Truncated);
            Assert.Equal(1, index.LineCount);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task PollRebuildsIndexAfterSameLengthRewrite()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.log");
        await File.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes("one\n"));

        try
        {
            var source = new FileLogSource(path);
            var detection = await new EncodingDetector().DetectAsync(source);
            var builder = new ChunkedLineIndexBuilder(chunkSize: 4, encodingDetection: detection);
            var index = (InMemoryLineIndex)await builder.BuildAsync(source);
            var initialLength = await source.GetLengthAsync();
            var tail = new LogTailService(source, index, builder, initialLength);

            await Task.Delay(20);
            await File.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes("two\n"));

            var update = await tail.PollAsync();

            Assert.True(update.WasReset);
            Assert.Equal("rewrite", update.ResetReason);
            Assert.Equal(1, index.LineCount);
            Assert.True(index.TryGetLine(0, out var line));
            Assert.Equal((0, 3, 0), (line.Offset, line.Length, line.LineNumber));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task PollRebuildsIndexAfterTruncation()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.log");
        await File.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes("one\ntwo\nthree\n"));

        try
        {
            var source = new FileLogSource(path);
            var detection = await new EncodingDetector().DetectAsync(source);
            var builder = new ChunkedLineIndexBuilder(chunkSize: 4, encodingDetection: detection);
            var index = (InMemoryLineIndex)await builder.BuildAsync(source);
            var initialLength = await source.GetLengthAsync();
            var tail = new LogTailService(source, index, builder, initialLength);

            await File.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes("new\n"));

            var update = await tail.PollAsync();

            Assert.True(update.WasReset);
            Assert.True(update.Truncated);
            Assert.Equal("truncation", update.ResetReason);
            Assert.Equal(1, index.LineCount);
            Assert.True(index.TryGetLine(0, out var line));
            Assert.Equal((0, 3, 0), (line.Offset, line.Length, line.LineNumber));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task PollRebuildsIndexAfterFileRotation()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.log");
        var rotatedPath = path + ".1";
        await File.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes("one\n"));

        try
        {
            var source = new FileLogSource(path);
            var detection = await new EncodingDetector().DetectAsync(source);
            var builder = new ChunkedLineIndexBuilder(chunkSize: 4, encodingDetection: detection);
            var index = (InMemoryLineIndex)await builder.BuildAsync(source);
            var initialLength = await source.GetLengthAsync();
            var tail = new LogTailService(source, index, builder, initialLength);

            File.Move(path, rotatedPath);
            await Task.Delay(20);
            await File.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes("fresh\nfile\n"));

            var update = await tail.PollAsync();

            Assert.True(update.WasReset);
            Assert.Equal("rotation", update.ResetReason);
            Assert.Equal(2, index.LineCount);
            Assert.True(index.TryGetLine(0, out var first));
            Assert.True(index.TryGetLine(1, out var second));
            Assert.Equal((0, 5, 0), (first.Offset, first.Length, first.LineNumber));
            Assert.Equal((6, 4, 1), (second.Offset, second.Length, second.LineNumber));
        }
        finally
        {
            File.Delete(path);
            File.Delete(rotatedPath);
        }
    }
}
