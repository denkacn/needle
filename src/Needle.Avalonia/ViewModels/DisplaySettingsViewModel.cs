namespace Needle.Avalonia.ViewModels;

using System.Collections.ObjectModel;
using global::Avalonia.Media;
using Needle.Application.Preferences;
using Needle.Application.Workspace;

public sealed class DisplaySettingsViewModel : ViewModelBase
{
    private string _logFontFamilyName;
    private FontFamily _logFontFamily;
    private double _logFontSize = 13;
    private string _logTextColor = "#C2CBD6";
    private string _triggerHighlightColor = "#4A1F24";
    private bool _isAlwaysOnTop;
    private bool _isAutoFollowTailEnabled = true;
    private bool _isMiniMapEnabled = true;
    private double _mainWindowX = double.NaN;
    private double _mainWindowY = double.NaN;
    private double _mainWindowWidth = 1100;
    private double _mainWindowHeight = 720;
    private double _settingsWindowWidth = 620;
    private double _settingsWindowHeight = 420;
    private readonly List<string> _customLogFontFamilyNames = [];

    public event EventHandler? SettingsChanged;

    public event EventHandler? MainWindowPlacementRestored;

    public DisplaySettingsViewModel()
    {
        LogFontFamilyOptions = [.. LogFontOptionsProvider.GetAvailableLogFonts()];
        _logFontFamilyName = LogFontOptionsProvider.GetDefaultLogFont(LogFontFamilyOptions);
        _logFontFamily = new FontFamily(_logFontFamilyName);
    }

    public ObservableCollection<string> LogFontFamilyOptions { get; }

    public IReadOnlyList<string> CustomLogFontFamilyNames => _customLogFontFamilyNames;

    public bool CanRemoveSelectedCustomLogFont => _customLogFontFamilyNames
        .Contains(LogFontFamilyName, StringComparer.OrdinalIgnoreCase);

    public string LogFontFamilyName
    {
        get => _logFontFamilyName;
        set
        {
            var resolvedFontName = LogFontOptionsProvider.ResolveFontName(value, LogFontFamilyOptions);
            if (SetProperty(ref _logFontFamilyName, resolvedFontName))
            {
                LogFontFamily = new FontFamily(resolvedFontName);
                OnPropertyChanged(nameof(CanRemoveSelectedCustomLogFont));
                NotifySettingsChanged();
            }
        }
    }

    public FontFamily LogFontFamily
    {
        get => _logFontFamily;
        private set => SetProperty(ref _logFontFamily, value);
    }

    public double LogFontSize
    {
        get => _logFontSize;
        set
        {
            if (SetProperty(ref _logFontSize, Math.Clamp(value, 8, 32)))
            {
                OnPropertyChanged(nameof(LogLineHeight));
                NotifySettingsChanged();
            }
        }
    }

    public double LogLineHeight => Math.Ceiling(LogFontSize + 9);

    public string LogTextColor
    {
        get => _logTextColor;
        set
        {
            if (SetProperty(ref _logTextColor, DisplaySettingsNormalizer.NormalizeColor(value, "#C2CBD6")))
            {
                NotifySettingsChanged();
            }
        }
    }

    public string TriggerHighlightColor
    {
        get => _triggerHighlightColor;
        set
        {
            if (SetProperty(ref _triggerHighlightColor, DisplaySettingsNormalizer.NormalizeColor(value, "#4A1F24")))
            {
                NotifySettingsChanged();
            }
        }
    }

    public bool IsAlwaysOnTop
    {
        get => _isAlwaysOnTop;
        set
        {
            if (SetProperty(ref _isAlwaysOnTop, value))
            {
                NotifySettingsChanged();
            }
        }
    }

    public bool IsAutoFollowTailEnabled
    {
        get => _isAutoFollowTailEnabled;
        set
        {
            if (SetProperty(ref _isAutoFollowTailEnabled, value))
            {
                NotifySettingsChanged();
            }
        }
    }

    public bool IsMiniMapEnabled
    {
        get => _isMiniMapEnabled;
        set
        {
            if (SetProperty(ref _isMiniMapEnabled, value))
            {
                NotifySettingsChanged();
            }
        }
    }

    public double MainWindowX
    {
        get => _mainWindowX;
        set => SetPositionProperty(ref _mainWindowX, value);
    }

    public double MainWindowY
    {
        get => _mainWindowY;
        set => SetPositionProperty(ref _mainWindowY, value);
    }

    public bool HasMainWindowPosition =>
        DisplaySettingsNormalizer.IsValidPosition(MainWindowX) &&
        DisplaySettingsNormalizer.IsValidPosition(MainWindowY);

    public double MainWindowWidth
    {
        get => _mainWindowWidth;
        set => SetWindowProperty(ref _mainWindowWidth, value, 900);
    }

    public double MainWindowHeight
    {
        get => _mainWindowHeight;
        set => SetWindowProperty(ref _mainWindowHeight, value, 560);
    }

    public double SettingsWindowWidth
    {
        get => _settingsWindowWidth;
        set => SetWindowProperty(ref _settingsWindowWidth, value, 520);
    }

    public double SettingsWindowHeight
    {
        get => _settingsWindowHeight;
        set => SetWindowProperty(ref _settingsWindowHeight, value, 340);
    }

    public void Apply(UserPreferences preferences)
    {
        ReplaceCustomLogFonts(preferences.CustomLogFontFamilyNames);
        LogFontFamilyName = LogFontOptionsProvider.ResolveFontName(preferences.LogFontFamilyName, LogFontFamilyOptions);
        LogFontSize = preferences.LogFontSize;
        if (!string.IsNullOrWhiteSpace(preferences.LogTextColor))
        {
            LogTextColor = preferences.LogTextColor;
        }

        if (!string.IsNullOrWhiteSpace(preferences.TriggerHighlightColor))
        {
            TriggerHighlightColor = preferences.TriggerHighlightColor;
        }

        IsAlwaysOnTop = preferences.IsAlwaysOnTop;
        IsAutoFollowTailEnabled = preferences.IsAutoFollowTailEnabled;
        IsMiniMapEnabled = preferences.IsMiniMapEnabled;
        ApplyMainWindowPosition(preferences.MainWindow);
        ApplyWindowSizes(preferences.MainWindow, preferences.SettingsWindow);
        MainWindowPlacementRestored?.Invoke(this, EventArgs.Empty);
    }

    public bool AddCustomLogFont(string? fontName)
    {
        var resolvedFontName = LogFontOptionsProvider.ResolveSystemFontName(fontName);
        if (string.IsNullOrWhiteSpace(resolvedFontName))
        {
            return false;
        }

        if (!_customLogFontFamilyNames.Contains(resolvedFontName, StringComparer.OrdinalIgnoreCase))
        {
            _customLogFontFamilyNames.Add(resolvedFontName);
            _customLogFontFamilyNames.Sort(StringComparer.CurrentCultureIgnoreCase);
            RebuildLogFontOptions();
        }

        LogFontFamilyName = resolvedFontName;
        NotifySettingsChanged();
        return true;
    }

    public void RemoveSelectedCustomLogFont()
    {
        var removed = _customLogFontFamilyNames.RemoveAll(font =>
            string.Equals(font, LogFontFamilyName, StringComparison.OrdinalIgnoreCase));
        if (removed == 0)
        {
            return;
        }

        RebuildLogFontOptions();
        LogFontFamilyName = LogFontOptionsProvider.GetDefaultLogFont(LogFontFamilyOptions);
        OnPropertyChanged(nameof(CanRemoveSelectedCustomLogFont));
        NotifySettingsChanged();
    }

    public void ApplyLegacy(WorkspaceState state)
    {
        ApplyWindowSizes(state.MainWindow, state.SettingsWindow);
        if (!string.IsNullOrWhiteSpace(state.UiSettings?.LogTextColor))
        {
            LogTextColor = state.UiSettings.LogTextColor;
        }
    }

    public void SaveMainWindowPlacement(double width, double height, double x, double y)
    {
        MainWindowX = x;
        MainWindowY = y;
        MainWindowWidth = width;
        MainWindowHeight = height;
    }

    public void SaveSettingsWindowSize(double width, double height)
    {
        SettingsWindowWidth = width;
        SettingsWindowHeight = height;
    }

    public void ResetLogTextColor(string defaultColor)
    {
        LogTextColor = DisplaySettingsNormalizer.NormalizeColor(defaultColor, "#C2CBD6");
    }

    public void ResetTriggerHighlightColor()
    {
        TriggerHighlightColor = "#4A1F24";
    }

    private void ApplyWindowSizes(WindowPreference? mainWindow, WindowPreference? settingsWindow)
    {
        ApplyWindowSizes(mainWindow?.Width, mainWindow?.Height, settingsWindow?.Width, settingsWindow?.Height);
    }

    private void ApplyMainWindowPosition(WindowPreference? mainWindow)
    {
        var x = mainWindow?.X;
        var y = mainWindow?.Y;

        if (x is double validX && double.IsFinite(validX))
        {
            MainWindowX = validX;
        }

        if (y is double validY && double.IsFinite(validY))
        {
            MainWindowY = validY;
        }
    }

    private void ApplyWindowSizes(WorkspaceWindowState? mainWindow, WorkspaceWindowState? settingsWindow)
    {
        ApplyWindowSizes(mainWindow?.Width, mainWindow?.Height, settingsWindow?.Width, settingsWindow?.Height);
    }

    private void ApplyWindowSizes(double? mainWidth, double? mainHeight, double? settingsWidth, double? settingsHeight)
    {
        if (mainWidth > 0)
        {
            MainWindowWidth = mainWidth.Value;
        }

        if (mainHeight > 0)
        {
            MainWindowHeight = mainHeight.Value;
        }

        if (settingsWidth > 0)
        {
            SettingsWindowWidth = settingsWidth.Value;
        }

        if (settingsHeight > 0)
        {
            SettingsWindowHeight = settingsHeight.Value;
        }
    }

    private void SetWindowProperty(ref double storage, double value, double minimum)
    {
        if (SetProperty(ref storage, DisplaySettingsNormalizer.ClampWindowSize(value, minimum)))
        {
            NotifySettingsChanged();
        }
    }

    private void SetPositionProperty(ref double storage, double value)
    {
        if (SetProperty(ref storage, value))
        {
            OnPropertyChanged(nameof(HasMainWindowPosition));
            NotifySettingsChanged();
        }
    }

    private void NotifySettingsChanged()
    {
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ReplaceCustomLogFonts(IEnumerable<string>? fontNames)
    {
        _customLogFontFamilyNames.Clear();
        _customLogFontFamilyNames.AddRange(LogFontOptionsProvider.NormalizeCustomFonts(fontNames));
        RebuildLogFontOptions();
    }

    private void RebuildLogFontOptions()
    {
        var current = LogFontFamilyName;
        LogFontFamilyOptions.Clear();
        foreach (var fontName in LogFontOptionsProvider.GetAvailableLogFonts(_customLogFontFamilyNames))
        {
            LogFontFamilyOptions.Add(fontName);
        }

        OnPropertyChanged(nameof(CustomLogFontFamilyNames));
        OnPropertyChanged(nameof(CanRemoveSelectedCustomLogFont));
        if (!LogFontFamilyOptions.Contains(current, StringComparer.OrdinalIgnoreCase))
        {
            LogFontFamilyName = LogFontOptionsProvider.GetDefaultLogFont(LogFontFamilyOptions);
        }
    }
}
