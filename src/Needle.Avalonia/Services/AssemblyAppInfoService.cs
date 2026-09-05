namespace Needle.Avalonia.Services;

using System.Reflection;

internal sealed class AssemblyAppInfoService : IAppInfoService
{
    public string Version { get; } = ResolveVersion();

    public string DisplayVersion => $"v{Version}";

    private static string ResolveVersion()
    {
        var assembly = typeof(AssemblyAppInfoService).Assembly;
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            var metadataIndex = informationalVersion.IndexOf('+');
            return metadataIndex > 0
                ? informationalVersion[..metadataIndex]
                : informationalVersion;
        }

        return assembly.GetName().Version?.ToString(fieldCount: 3) ?? "0.0.0";
    }
}
