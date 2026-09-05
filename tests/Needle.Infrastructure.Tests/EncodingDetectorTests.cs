namespace Needle.Infrastructure.Tests;

using System.Text;
using Needle.Infrastructure.Encoding;
using Needle.Infrastructure.Sources;

public sealed class EncodingDetectorTests
{
    [Fact]
    public async Task DetectsUtf8Bom()
    {
        var result = await DetectAsync([0xEF, 0xBB, 0xBF, (byte)'h', (byte)'i']);

        Assert.Equal(Encoding.UTF8.CodePage, result.Encoding.CodePage);
        Assert.Equal(3, result.PreambleLength);
        Assert.Equal("UTF-8 BOM", result.DisplayName);
    }

    [Fact]
    public async Task DetectsUtf16LittleEndianBom()
    {
        var result = await DetectAsync([0xFF, 0xFE, (byte)'h', 0x00]);

        Assert.Equal(Encoding.Unicode.CodePage, result.Encoding.CodePage);
        Assert.Equal(2, result.PreambleLength);
        Assert.Equal("UTF-16 LE", result.DisplayName);
    }

    [Fact]
    public async Task DetectsUtf16BigEndianBom()
    {
        var result = await DetectAsync([0xFE, 0xFF, 0x00, (byte)'h']);

        Assert.Equal(Encoding.BigEndianUnicode.CodePage, result.Encoding.CodePage);
        Assert.Equal(2, result.PreambleLength);
        Assert.Equal("UTF-16 BE", result.DisplayName);
    }

    [Fact]
    public async Task DefaultsAsciiForPlainAsciiBytes()
    {
        var result = await DetectAsync(Encoding.ASCII.GetBytes("plain log"));

        Assert.Equal(Encoding.ASCII.CodePage, result.Encoding.CodePage);
        Assert.Equal(0, result.PreambleLength);
        Assert.Equal("ASCII", result.DisplayName);
    }

    [Fact]
    public async Task DetectsUtf8WithoutBomWhenCyrillicBytesAreValidUtf8()
    {
        var result = await DetectAsync(Encoding.UTF8.GetBytes("лог: Привет"));

        Assert.Equal(Encoding.UTF8.CodePage, result.Encoding.CodePage);
        Assert.Equal(0, result.PreambleLength);
        Assert.Equal("UTF-8", result.DisplayName);
    }

    [Fact]
    public async Task DetectsWindows1251WhenCyrillicBytesAreNotValidUtf8()
    {
        var result = await DetectAsync([0xEB, 0xEE, 0xE3, 0x3A, 0x20, 0xCF, 0xF0, 0xE8, 0xE2, 0xE5, 0xF2]);

        Assert.Equal(1251, result.Encoding.CodePage);
        Assert.Equal(0, result.PreambleLength);
        Assert.Equal("Windows-1251", result.DisplayName);
    }

    private static async Task<EncodingDetectionResult> DetectAsync(byte[] bytes)
    {
        var path = await WriteTempBytesAsync(bytes);

        try
        {
            return await new EncodingDetector().DetectAsync(new FileLogSource(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static async Task<string> WriteTempBytesAsync(byte[] bytes)
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.log");
        await File.WriteAllBytesAsync(path, bytes);
        return path;
    }
}
