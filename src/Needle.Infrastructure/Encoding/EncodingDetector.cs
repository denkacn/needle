namespace Needle.Infrastructure.Encoding;

using System.Text;
using Needle.Core.Sources;

public sealed class EncodingDetector
{
    private const int ProbeSize = 4 * 1024;
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    static EncodingDetector()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public async ValueTask<EncodingDetectionResult> DetectAsync(
        ILogSource source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        await using var stream = await source.OpenReadAsync(cancellationToken);
        var buffer = new byte[ProbeSize];
        var read = await stream.ReadAsync(buffer, cancellationToken);
        var probe = buffer.AsSpan(0, read);

        if (StartsWith(probe, [0xEF, 0xBB, 0xBF]))
        {
            return new EncodingDetectionResult(new UTF8Encoding(false), 3, "UTF-8 BOM");
        }

        if (StartsWith(probe, [0xFF, 0xFE]))
        {
            return new EncodingDetectionResult(Encoding.Unicode, 2, "UTF-16 LE");
        }

        if (StartsWith(probe, [0xFE, 0xFF]))
        {
            return new EncodingDetectionResult(Encoding.BigEndianUnicode, 2, "UTF-16 BE");
        }

        if (LooksAscii(probe))
        {
            return new EncodingDetectionResult(Encoding.ASCII, 0, "ASCII");
        }

        if (LooksUtf8(probe))
        {
            return new EncodingDetectionResult(new UTF8Encoding(false), 0, "UTF-8");
        }

        if (LooksWindows1251Cyrillic(probe))
        {
            return new EncodingDetectionResult(Encoding.GetEncoding(1251), 0, "Windows-1251");
        }

        return new EncodingDetectionResult(new UTF8Encoding(false), 0, "UTF-8");
    }

    private static bool StartsWith(ReadOnlySpan<byte> value, ReadOnlySpan<byte> prefix)
    {
        return value.Length >= prefix.Length && value[..prefix.Length].SequenceEqual(prefix);
    }

    private static bool LooksAscii(ReadOnlySpan<byte> value)
    {
        foreach (var b in value)
        {
            if (b > 0x7F)
            {
                return false;
            }
        }

        return true;
    }

    private static bool LooksUtf8(ReadOnlySpan<byte> value)
    {
        try
        {
            _ = StrictUtf8.GetString(value);
            return true;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }

    private static bool LooksWindows1251Cyrillic(ReadOnlySpan<byte> value)
    {
        var cyrillicScore = 0;
        var highBytes = 0;

        foreach (var b in value)
        {
            if (b <= 0x7F)
            {
                continue;
            }

            highBytes++;

            if (b is >= 0xC0 and <= 0xFF or 0xA8 or 0xB8)
            {
                cyrillicScore++;
            }
        }

        return highBytes > 0 && cyrillicScore >= Math.Max(1, highBytes / 2);
    }
}
