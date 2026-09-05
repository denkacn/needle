namespace Needle.Avalonia.Services;

public interface IAppInfoService
{
    string Version { get; }

    string DisplayVersion { get; }
}
