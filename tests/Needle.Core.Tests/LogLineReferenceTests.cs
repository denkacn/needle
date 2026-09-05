namespace Needle.Core.Tests;

using Needle.Core.Lines;

public sealed class LogLineReferenceTests
{
    [Fact]
    public void StoresLineLocationWithoutLogText()
    {
        var reference = new LogLineReference(Offset: 1024, Length: 80, LineNumber: 42);

        Assert.Equal(1024, reference.Offset);
        Assert.Equal(80, reference.Length);
        Assert.Equal(42, reference.LineNumber);
    }
}
