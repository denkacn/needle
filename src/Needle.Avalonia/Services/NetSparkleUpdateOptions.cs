namespace Needle.Avalonia.Services;

internal sealed record NetSparkleUpdateOptions(
    string AppCastUrl,
    string? Ed25519PublicKey = null)
{
    public static NetSparkleUpdateOptions Default { get; } = new(
        AppCastUrl: "https://needle.bypuziki.com/updates/appcast.xml");
}
