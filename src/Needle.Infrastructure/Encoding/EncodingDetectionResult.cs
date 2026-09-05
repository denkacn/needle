namespace Needle.Infrastructure.Encoding;

using System.Text;

public sealed record EncodingDetectionResult(
    Encoding Encoding,
    int PreambleLength,
    string DisplayName);
