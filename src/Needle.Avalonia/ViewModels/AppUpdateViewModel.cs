namespace Needle.Avalonia.ViewModels;

using CommunityToolkit.Mvvm.Input;
using Needle.Avalonia.Services;

public sealed class AppUpdateViewModel : ViewModelBase
{
    private readonly IAppInfoService _appInfo;
    private readonly IAppUpdateService _updates;
    private readonly CancellationSlot _checkCancellation = new();
    private readonly CancellationSlot _updateCancellation = new();
    private bool _isChecking;
    private bool _isUpdating;
    private bool _hasAvailableUpdate;
    private int _updateProgress;
    private string? _availableVersion;
    private string _status = string.Empty;

    public AppUpdateViewModel(IAppInfoService appInfo, IAppUpdateService updates)
    {
        _appInfo = appInfo ?? throw new ArgumentNullException(nameof(appInfo));
        _updates = updates ?? throw new ArgumentNullException(nameof(updates));
        ApplyUpdateCommand = new AsyncRelayCommand(ApplyUpdateAsync, () => HasAvailableUpdate && !IsUpdating);
    }

    public string CurrentVersionDisplay => _appInfo.DisplayVersion;

    public string? AvailableVersion
    {
        get => _availableVersion;
        private set
        {
            if (SetProperty(ref _availableVersion, value))
            {
                OnPropertyChanged(nameof(AvailableVersionDisplay));
                OnPropertyChanged(nameof(AvailableUpdateText));
            }
        }
    }

    public string? AvailableVersionDisplay => string.IsNullOrWhiteSpace(AvailableVersion)
        ? null
        : $"v{AvailableVersion}";

    public string AvailableUpdateText => string.IsNullOrWhiteSpace(AvailableVersionDisplay)
        ? string.Empty
        : $"{AvailableVersionDisplay} is available";

    public bool IsChecking
    {
        get => _isChecking;
        private set => SetProperty(ref _isChecking, value);
    }

    public bool IsUpdating
    {
        get => _isUpdating;
        private set
        {
            if (SetProperty(ref _isUpdating, value))
            {
                OnPropertyChanged(nameof(UpdateButtonText));
                OnPropertyChanged(nameof(ShowUpdateIcon));
                ApplyUpdateCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool HasAvailableUpdate
    {
        get => _hasAvailableUpdate;
        private set
        {
            if (SetProperty(ref _hasAvailableUpdate, value))
            {
                ApplyUpdateCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public int UpdateProgress
    {
        get => _updateProgress;
        private set
        {
            if (SetProperty(ref _updateProgress, value))
            {
                OnPropertyChanged(nameof(UpdateButtonText));
            }
        }
    }

    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public string UpdateButtonText => IsUpdating ? $"{UpdateProgress}%" : "Update";

    public bool ShowUpdateIcon => !IsUpdating;

    public IAsyncRelayCommand ApplyUpdateCommand { get; }

    public async Task CheckForUpdatesAsync()
    {
        var cancellationToken = _checkCancellation.Reset().Token;
        IsChecking = true;
        try
        {
            var result = await _updates.CheckForUpdatesAsync(cancellationToken);
            AvailableVersion = result.Version;
            HasAvailableUpdate = result.IsAvailable;
            Status = result.Message ?? string.Empty;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Status = $"Update check failed: {ex.Message}";
            AvailableVersion = null;
            HasAvailableUpdate = false;
        }
        finally
        {
            IsChecking = false;
        }
    }

    private async Task ApplyUpdateAsync()
    {
        if (!HasAvailableUpdate || IsUpdating)
        {
            return;
        }

        var cancellationToken = _updateCancellation.Reset().Token;
        IsUpdating = true;
        UpdateProgress = 0;
        Status = "Downloading update";
        try
        {
            await _updates.DownloadAndApplyUpdateAsync(
                new Progress<int>(progress => UpdateProgress = progress),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Status = "Update cancelled";
        }
        catch (Exception ex)
        {
            Status = $"Update failed: {ex.Message}";
            IsUpdating = false;
        }
    }
}
