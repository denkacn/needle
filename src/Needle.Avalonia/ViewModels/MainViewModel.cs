using System.Collections.ObjectModel;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.Input;
using Needle.Avalonia.Services;
using Needle.Application.Documents;
using Needle.Application.Preferences;
using Needle.Application.Workspace;
using Needle.Core.Bookmarks;
using Needle.Core.Search;
namespace Needle.Avalonia.ViewModels;
public class MainViewModel : ViewModelBase
{
    private const int WheelLineDelta = 3;
    private const int FilterResultLimit = 200_000;
    private const int WorkspaceSaveDelayMilliseconds = 350;
    private readonly ILogFilePicker _filePicker;
    private readonly IAppInfoService _appInfo;
    private readonly ActiveFileWorkflow _activeFile;
    private readonly BookmarkSelectionWorkflow _bookmarks = new();
    private readonly BookmarkWorkflow _bookmarkWorkflow;
    private readonly LogTabFactory _tabFactory;
    private readonly TriggerRulesViewModel _emptyTriggers = new();
    private readonly TriggerWorkflow _triggerWorkflow;
    private readonly HighlightRulesViewModel _highlightRules = new();
    private readonly LogTimelineViewModel _timeline = new();
    private readonly TimelineWorkflow _timelineWorkflow;
    private readonly MainHighlightCoordinator _highlights;
    private readonly LogViewportWorkflow _viewport = new();
    private readonly LogTailWorkflow _tail = new();
    private readonly SearchFilterWorkflow _searchFilter = new(FilterResultLimit);
    private readonly WorkspaceRestoreWorkflow _workspaceRestore = new();
    private readonly LogTabStateWorkflow _tabState = new();
    private readonly MainPersistenceWorkflow _persistence;
    private readonly SettingsTransferWorkflow _settingsTransfer;
    private readonly MainCommandSet _commands;
    private readonly UiTaskRunner _tasks;
    private readonly LogDocumentWorkspaceController _documents;
    private readonly CancellationSlot _openCancellation = new();
    private readonly CancellationSlot _viewportCancellation = new();
    private readonly CancellationSlot _tailCancellation = new();
    private bool _isRestoringWorkspace;
    private bool _isSwitchingTabs;
    public MainViewModel()
        : this(new NoOpLogFilePicker(), workspaceStore: null)
    {
    }
    public MainViewModel(ILogFilePicker filePicker)
        : this(filePicker, workspaceStore: null)
    {
    }
    public MainViewModel(ILogFilePicker filePicker, IWorkspaceStore? workspaceStore)
        : this(filePicker, workspaceStore, preferencesStore: null, appInfo: null)
    {
    }
    public MainViewModel(
        ILogFilePicker filePicker,
        IWorkspaceStore? workspaceStore,
        IUserPreferencesStore? preferencesStore,
        IAppInfoService? appInfo,
        ILogDocumentSessionFactory? sessionFactory = null,
        IExternalFileLauncher? externalFileLauncher = null,
        IAppUpdateService? updateService = null)
    {
        _filePicker = filePicker ?? throw new ArgumentNullException(nameof(filePicker));
        _appInfo = appInfo ?? new AssemblyAppInfoService();
        Updates = new AppUpdateViewModel(_appInfo, updateService ?? new NoOpAppUpdateService());
        _activeFile = new ActiveFileWorkflow(new ExternalFileActionCoordinator(externalFileLauncher ?? new ExternalFileLauncher()));
        _tabFactory = new LogTabFactory(sessionFactory ?? new NoOpLogDocumentSessionFactory());
        _tasks = new UiTaskRunner(message => Document.Status = message);
        _documents = new LogDocumentWorkspaceController(_tabState);
        _timelineWorkflow = new TimelineWorkflow(
            _timeline,
            Document,
            Lines,
            Bookmarks,
            SearchFilter,
            () => ActiveTriggers,
            ClampFirstVisibleLine);
        _bookmarkWorkflow = new BookmarkWorkflow(
            _bookmarks,
            Document,
            SearchFilter,
            FilteredLineNumbers,
            ClampFirstVisibleLine,
            () => _tasks.Run(LoadViewportFromScrollAsync, "Viewport refresh failed"),
            QueueWorkspaceSave,
            OnPropertyChanged,
            CreateBookmarkCommandRefreshContext,
            () => _timelineWorkflow.Refresh());
        _triggerWorkflow = new TriggerWorkflow(new TriggerWorkflowContext(
            Document,
            SearchFilter,
            () => ActiveTriggers,
            () => _documents.ActiveDocument.Tab,
            () => !_isRestoringWorkspace && !_isSwitchingTabs && Document.IsDocumentOpen,
            ClampFirstVisibleLine,
            status => Document.Status = status,
            OnPropertyChanged,
            () => _timelineWorkflow.Refresh(),
            NotifyNavigationCanExecuteChanged,
            () => _tasks.Run(LoadViewportFromScrollAsync, "Trigger refresh failed"),
            QueueWorkspaceSave));
        _highlights = new MainHighlightCoordinator(
            _highlightRules,
            _tasks,
            () => _documents.ActiveDocument.Tab,
            () => Document.IsDocumentOpen,
            LoadViewportFromScrollAsync,
            ClearTimelineHighlights,
            QueuePreferencesSave,
            QueueWorkspaceSave);
        _documents.SetTabLifecycleHandlers(AttachTab, DetachTab);
        var saveDelay = TimeSpan.FromMilliseconds(WorkspaceSaveDelayMilliseconds);
        _persistence = new MainPersistenceWorkflow(
            workspaceStore,
            preferencesStore,
            _tasks,
            DisplaySettings,
            OpenRecentFileAsync,
            CreatePreferencesSnapshot,
            CreateWorkspaceState,
            () => !_isRestoringWorkspace && !_isSwitchingTabs,
            () => !_isRestoringWorkspace && !_isSwitchingTabs,
            saveDelay);
        _settingsTransfer = new SettingsTransferWorkflow(
            CreatePreferencesSnapshot,
            ApplyPreferences,
            SaveSettingsNowAsync,
            () => Document.IsDocumentOpen,
            LoadViewportFromScrollAsync,
            status => Document.Status = status);
        DisplaySettings.SettingsChanged += OnDisplaySettingsChanged;
        _commands = new MainCommandFactory().Create(CreateCommandContext());

        Document.SelectedTabChanged += OnSelectedTabChanged;
        Document.ScrollLineChanged += OnScrollLineChanged;
        Document.SelectedLineNumberChanged += OnSelectedLineNumberChanged;
        Document.StructuredModeChanged += OnIsStructuredModeChanged;
        SearchFilter.SearchTextChanged += OnSearchTextChanged;
        SearchFilter.SearchCaseSensitiveChanged += OnIsSearchCaseSensitiveChanged;
        SearchFilter.SearchRegexChanged += OnIsSearchRegexChanged;
        SearchFilter.FilterTextChanged += OnFilterTextChanged;
        ExcludeRules.RulesChanged += OnExcludeRulesChanged;
    }
    public string AppVersion => _appInfo.DisplayVersion;
    public AppUpdateViewModel Updates { get; }
    public ObservableCollection<LogLineViewModel> Lines { get; } = [];
    public ObservableCollection<LogTabViewModel> Tabs => _documents.Tabs;
    public DocumentStateViewModel Document { get; } = new();
    public HighlightRulesViewModel Highlights => _highlightRules;
    public LogTimelineViewModel Timeline => _timeline;
    public TriggerRulesViewModel ActiveTriggers => _documents.ActiveDocument.Tab?.Triggers ?? _emptyTriggers;
    public DisplaySettingsViewModel DisplaySettings { get; } = new();
    public SearchFilterViewModel SearchFilter { get; } = new();
    public ExcludeRulesViewModel ExcludeRules { get; } = new();
    public ObservableCollection<LogSearchMatch> SearchMatches => SearchFilter.SearchMatches;
    public ObservableCollection<long> FilteredLineNumbers => SearchFilter.FilteredLineNumbers;
    public ObservableCollection<LogBookmark> Bookmarks => _bookmarks.Bookmarks;
    public bool HasBookmarks => _bookmarks.HasBookmarks;
    public string SelectedBookmarkText => _bookmarks.SelectedBookmarkDisplay;
    public ObservableCollection<RecentFileViewModel> RecentFiles => _persistence.RecentFiles;
    public IReadOnlyList<string> RecentFilePaths => _persistence.RecentFilePaths;
    public bool HasRecentFiles => _persistence.HasRecentFiles;
    public IAsyncRelayCommand OpenFileCommand => _commands.OpenFileCommand;
    public IRelayCommand ShowInExplorerCommand => _commands.ShowInExplorerCommand;
    public IRelayCommand OpenInEditorCommand => _commands.OpenInEditorCommand;
    public IAsyncRelayCommand ClearLogCommand => _commands.ClearLogCommand;
    public IRelayCommand PageUpCommand => _commands.PageUpCommand;
    public IRelayCommand PageDownCommand => _commands.PageDownCommand;
    public IRelayCommand GoToTopCommand => _commands.GoToTopCommand;
    public IRelayCommand GoToEndCommand => _commands.GoToEndCommand;
    public IRelayCommand ToggleFollowTailCommand => _commands.ToggleFollowTailCommand;
    public IAsyncRelayCommand SearchNextCommand => _commands.SearchNextCommand;
    public IRelayCommand SearchPreviousCommand => _commands.SearchPreviousCommand;
    public IAsyncRelayCommand ApplyFilterCommand => _commands.ApplyFilterCommand;
    public IRelayCommand ClearFilterCommand => _commands.ClearFilterCommand;
    public IRelayCommand ToggleBookmarkCommand => _commands.ToggleBookmarkCommand;
    public IRelayCommand NextBookmarkCommand => _commands.NextBookmarkCommand;
    public IRelayCommand PreviousBookmarkCommand => _commands.PreviousBookmarkCommand;
    public IRelayCommand RemoveSelectedBookmarkCommand => _commands.RemoveSelectedBookmarkCommand;
    public IRelayCommand ClearBookmarksCommand => _commands.ClearBookmarksCommand;
    public IRelayCommand PreviousTriggerCommand => _commands.PreviousTriggerCommand;
    public IRelayCommand NextTriggerCommand => _commands.NextTriggerCommand;
    public IRelayCommand RemoveSelectedTriggerCommand => _commands.RemoveSelectedTriggerCommand;
    public IRelayCommand ClearTriggersCommand => _commands.ClearTriggersCommand;
    public IRelayCommand<LogTabViewModel> SelectTabCommand => _commands.SelectTabCommand;
    public IRelayCommand<LogTabViewModel> CloseTabCommand => _commands.CloseTabCommand;
    public IRelayCommand CloseSelectedTabCommand => _commands.CloseSelectedTabCommand;
    private MainCommandContext CreateCommandContext()
    {
        return new MainCommandContext(
            OpenFileAsync,
            ShowActiveFileInExplorer,
            OpenActiveFileInEditor,
            ClearActiveLogAsync,
            PageUp,
            PageDown,
            GoToTop,
            GoToEnd,
            ToggleFollowTail,
            SearchNextAsync,
            SearchPrevious,
            ApplyFilterAsync,
            ClearFilter,
            ToggleBookmark,
            GoToNextBookmark,
            GoToPreviousBookmark,
            RemoveSelectedBookmark,
            ClearBookmarks,
            GoToPreviousTrigger,
            GoToNextTrigger,
            RemoveSelectedTrigger,
            ClearTriggers,
            SelectTab,
            CloseTab,
            CloseSelectedTab,
            CanOpenActiveFileExternally,
            CanNavigate,
            () => Document.IsDocumentOpen,
            CanSearch,
            () => SearchFilter.SearchMatches.Count > 0,
            () => Document.IsDocumentOpen && !Document.IsBusy && (!string.IsNullOrWhiteSpace(SearchFilter.FilterText) || ExcludeRules.HasRules),
            () => SearchFilter.IsFilterActive || !string.IsNullOrWhiteSpace(SearchFilter.FilterText),
            () => HasBookmarks,
            () => ActiveTriggers.HasHits,
            () => Document.SelectedTab is not null);
    }
    private async Task OpenFileAsync()
    {
        var path = await _filePicker.PickLogFileAsync();
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }
        await OpenFilePathAsync(path);
    }
    public async Task OpenFilePathAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        path = System.IO.Path.GetFullPath(path);
        var existingTab = _documents.FindTab(path);
        if (existingTab is not null)
        {
            Document.SelectedTab = existingTab;
            TrackRecentFile(path);
            return;
        }
        SaveActiveTabState();
        CancelDocumentOperations(includeOpen: true, includeViewport: false);
        var cancellationToken = _openCancellation.Reset().Token;
        ResetVisibleDocumentState();
        Document.IsBusy = true;
        Document.ActiveTabTitle = System.IO.Path.GetFileName(path);
        Document.Status = LogStatusFormatter.Opening(Document.ActiveTabTitle);
        try
        {
            var openResult = await _tabFactory.OpenAsync(
                path,
                progress => Document.Status = LogStatusFormatter.Indexing(progress),
                cancellationToken);
            ActivateOpenedTab(path, openResult.Tab);
            RefreshMaximumScrollLine();
            Document.IsDocumentOpen = true;
            NotifyNavigationCanExecuteChanged();
            await LoadViewportAsync(0, cancellationToken);
            StartTailLoop();
            TrackRecentFile(path);
            SaveActiveTabState();
            QueueWorkspaceSave();
        }
        catch (OperationCanceledException)
        {
            Document.Status = LogStatusFormatter.OpenCancelled;
        }
        catch (Exception ex)
        {
            Lines.Clear();
            Document.HasLines = false;
            Document.IsDocumentOpen = false;
            _documents.Clear();
            Document.Status = LogStatusFormatter.OpenFailed(ex);
        }
        finally
        {
            Document.IsBusy = false;
            NotifyNavigationCanExecuteChanged();
        }
    }
    private void OnScrollLineChanged(double value)
    {
        if (!Document.IsDocumentOpen)
        {
            return;
        }
        UpdateAutoFollowTail(value);
        _tasks.Run(LoadViewportFromScrollAsync, "Viewport refresh failed");
        QueueWorkspaceSave();
    }
    private void OnSelectedTabChanged(LogTabViewModel? value)
    {
        if (_isSwitchingTabs || value is null || _documents.ActiveDocument.IsActive(value))
        {
            return;
        }
        SwitchToTab(value);
    }
    private void OnSelectedLineNumberChanged(long value)
    {
        RefreshSelectedLineText();
        _tasks.Run(LoadViewportFromScrollAsync, "Viewport refresh failed");
        QueueWorkspaceSave();
    }
    private void CancelDocumentOperations(bool includeOpen, bool includeViewport)
    {
        if (includeOpen)
        {
            _openCancellation.CancelAndDispose();
        }
        _tailCancellation.CancelAndDispose();
        _searchFilter.Cancel();
        if (includeViewport)
        {
            _viewportCancellation.CancelAndDispose();
        }
    }
    private void ActivateOpenedTab(string path, LogTabViewModel tab)
    {
        _timelineWorkflow.Reset();
        _documents.ActivateOpenedTab(path, tab);
        _isSwitchingTabs = true;
        Document.SelectedTab = tab;
        _isSwitchingTabs = false;
    }
    private void SelectRestoredActiveTab(string? activeDocumentPath)
    {
        var activeTab = _documents.SelectRestoredActiveTab(activeDocumentPath);
        if (activeTab is not null)
        {
            Document.SelectedTab = activeTab;
        }
    }
    public async Task RestoreWorkspaceAsync()
    {
        await _workspaceRestore.RestoreAsync(new WorkspaceRestoreContext(
            Document,
            SearchFilter,
            ExcludeRules,
            RestorePreferencesAsync,
            _persistence.LoadWorkspaceAsync,
            OpenFilePathAsync,
            () => _documents.ActiveDocument.HasTab,
            () => _isRestoringWorkspace = true,
            () => _isRestoringWorkspace = false,
            RestoreLegacyPreferences,
            _highlights.RestoreGlobal,
            (configured, rules) => _highlights.RestoreActiveTab(_documents.ActiveDocument.Tab, configured, rules),
            RestoreActiveTabTriggers,
            RefreshTailDisplayState,
            RestoreBookmarks,
            ApplyFilterAsync,
            ClampFirstVisibleLine,
            LoadViewportFromScrollAsync,
            SaveActiveTabState,
            SelectRestoredActiveTab,
            QueueWorkspaceSave));
    }
    public async Task RestoreStartupPreferencesAsync()
    {
        await RestorePreferencesAsync();
    }
    public Task CheckForUpdatesAsync()
    {
        return Updates.CheckForUpdatesAsync();
    }
    private async Task LoadViewportFromScrollAsync()
    {
        var cancellationToken = _viewportCancellation.Reset().Token;
        try
        {
            await LoadViewportAsync(ToLineNumber(Document.ScrollLine), cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
    }
    private async Task LoadViewportAsync(long firstLine, CancellationToken cancellationToken)
    {
        await _viewport.LoadAsync(
            _documents.ActiveDocument.Session,
            Document,
            SearchFilter,
            _highlights.GetActiveRules(_documents.ActiveDocument.Tab),
            Lines,
            _bookmarks.Contains,
            line => _documents.ActiveDocument.Tab?.Triggers.ContainsHit(line) == true,
            firstLine,
            cancellationToken);
        RefreshSelectedLineText();
        _timelineWorkflow.Refresh();
    }
    private void PageUp()
    {
        MoveViewportBy(-Document.ViewportLineCount);
    }
    private void PageDown()
    {
        MoveViewportBy(Document.ViewportLineCount);
    }
    private void GoToTop()
    {
        Document.ScrollLine = 0;
    }
    private void GoToEnd()
    {
        Document.ScrollLine = Document.MaximumScrollLine;
    }
    public void MoveViewportByWheelDelta(double wheelDeltaY)
    {
        if (Math.Abs(wheelDeltaY) < 0.001)
        {
            return;
        }
        var direction = wheelDeltaY > 0 ? -1 : 1;
        MoveViewportBy(direction * WheelLineDelta);
    }
    public void MoveViewportBy(long delta)
    {
        if (!CanNavigate() || delta == 0)
        {
            return;
        }
        Document.ScrollLine = ClampFirstVisibleLine(ToLineNumber(Document.ScrollLine) + delta);
    }
    public void SetViewportLineCount(double viewportHeight, double lineHeight)
    {
        if (_viewport.SetViewportLineCount(viewportHeight, lineHeight, _documents.ActiveDocument.Session, Document, SearchFilter))
        {
            _tasks.Run(LoadViewportFromScrollAsync, "Viewport refresh failed");
        }
    }
    private bool CanNavigate()
    {
        return Document.IsDocumentOpen && !Document.IsBusy;
    }
    private void NotifyNavigationCanExecuteChanged()
    {
        _commands.NotifyDocumentStateCanExecuteChanged();
    }
    private void OnSearchTextChanged(string value)
    {
        _searchFilter.OnSearchTextChanged(
            SearchFilter,
            NotifySearchFilterStateChanged,
            NotifySearchCommandsCanExecuteChanged,
            QueueWorkspaceSave);
    }
    private void OnIsSearchCaseSensitiveChanged(bool value)
    {
        _searchFilter.OnSearchOptionChanged(
            SearchFilter,
            NotifySearchFilterStateChanged,
            NotifySearchCommandsCanExecuteChanged,
            QueueWorkspaceSave);
    }
    private void OnIsSearchRegexChanged(bool value)
    {
        _searchFilter.OnSearchOptionChanged(
            SearchFilter,
            NotifySearchFilterStateChanged,
            NotifySearchCommandsCanExecuteChanged,
            QueueWorkspaceSave);
    }
    private void OnFilterTextChanged(string value)
    {
        _searchFilter.OnFilterTextChanged(
            SearchFilter,
            value,
            ClearFilterCommand.NotifyCanExecuteChanged,
            QueueWorkspaceSave);
    }
    private void OnIsStructuredModeChanged(bool value)
    {
        if (!_isSwitchingTabs)
        {
            _tasks.Run(LoadViewportFromScrollAsync, "Viewport refresh failed");
        }
        QueueWorkspaceSave();
    }
    private long ClampFirstVisibleLine(long lineNumber)
    {
        return _viewport.ClampFirstVisibleLine(lineNumber, Document);
    }
    private void RefreshMaximumScrollLine()
    {
        _viewport.RefreshMaximumScrollLine(_documents.ActiveDocument.Session, Document, SearchFilter);
        Document.TotalLineCount = _documents.ActiveDocument.Session?.LineCount ?? 0;
        _timelineWorkflow.Refresh();
    }
    private void ToggleFollowTail()
    {
        _tail.Toggle(Document, GoToEnd);
        QueueWorkspaceSave();
    }
    private void UpdateAutoFollowTail(double scrollLine)
    {
        _tail.UpdateAutoFollowTail(Document, DisplaySettings, scrollLine);
    }
    private void RefreshTailDisplayState()
    {
        _tail.RefreshDisplayState(Document);
    }
    private void StartTailLoop()
    {
        var cancellationToken = _tailCancellation.Reset().Token;
        _tasks.Run(() => RunTailLoopAsync(cancellationToken), "Live tail failed");
    }
    private void RefreshSelectedLineText()
    {
        _bookmarks.RefreshSelection(Document, Lines);
    }
    private async Task RunTailLoopAsync(CancellationToken cancellationToken)
    {
        await _tail.RunAsync(
            _documents.ActiveDocument.Session,
            () => _documents.ActiveDocument.Session,
            LoadViewportFromScrollAsync,
            RefreshMaximumScrollLine,
            ClampFirstVisibleLine,
            ProcessTailUpdateAsync,
            Document,
            cancellationToken);
    }
    private static long ToLineNumber(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return 0;
        }
        return Math.Max(0, (long)Math.Round(value));
    }
    private void ShowActiveFileInExplorer()
    {
        _activeFile.ShowInFileManager(CreateActiveFileActionContext());
    }
    private void OpenActiveFileInEditor()
    {
        _activeFile.OpenInDefaultEditor(CreateActiveFileActionContext());
    }

    private ActiveFileActionContext CreateActiveFileActionContext()
    {
        return new ActiveFileActionContext(
            Document.IsDocumentOpen,
            _documents.ActiveDocument.Path,
            status => Document.Status = status,
            NotifyNavigationCanExecuteChanged);
    }

    private Task ClearActiveLogAsync()
    {
        return _activeFile.ClearLogAsync(new ActiveLogClearContext(
            _documents.ActiveDocument.Path,
            Document,
            SearchFilter,
            RefreshActiveSessionAfterTruncateAsync,
            RefreshMaximumScrollLine,
            LoadViewportFromScrollAsync,
            RefreshBookmarkCommands,
            NotifyNavigationCanExecuteChanged));
    }

    private async Task RefreshActiveSessionAfterTruncateAsync()
    {
        if (_documents.ActiveDocument.Session is not { } session)
        {
            return;
        }

        var cancellationToken = _viewportCancellation.Reset().Token;
        var tailUpdate = await session.TailService.PollAsync(cancellationToken);
        session.FileSize = tailUpdate.CurrentLength;
        await ProcessTailUpdateAsync(session, new LogTailUpdate(
            tailUpdate.WasReset,
            tailUpdate.HasNewLines,
            tailUpdate.AddedLineCount,
            tailUpdate.ResetReason), cancellationToken);
    }
    private bool CanOpenActiveFileExternally()
    {
        return _activeFile.CanUse(Document.IsDocumentOpen, _documents.ActiveDocument.Path);
    }
    private async Task SearchNextAsync()
    {
        await _searchFilter.SearchNextAsync(
            _documents.ActiveDocument.Session,
            Document,
            SearchFilter,
            ClampFirstVisibleLine,
            NotifySearchFilterStateChanged,
            SearchPreviousCommand.NotifyCanExecuteChanged);
    }
    private void SearchPrevious()
    {
        _searchFilter.SearchPrevious(Document, SearchFilter, ClampFirstVisibleLine);
    }
    private bool CanSearch()
    {
        return _searchFilter.CanSearch(Document, SearchFilter);
    }
    private async Task ApplyFilterAsync()
    {
        await _searchFilter.ApplyFilterAsync(
            _documents.ActiveDocument.Session,
            Document,
            SearchFilter,
            ExcludeRules,
            LoadViewportAsync,
            RefreshMaximumScrollLine,
            NotifySearchFilterStateChanged,
            ClearFilterCommand.NotifyCanExecuteChanged,
            QueueWorkspaceSave,
            _viewportCancellation.Reset().Token);
    }
    private void ClearFilter()
    {
        _tasks.Run(ClearFilterAsync, "Clear filter failed");
    }
    private async Task ClearFilterAsync()
    {
        await _searchFilter.ClearFilterAsync(
            _documents.ActiveDocument.Session,
            Document,
            SearchFilter,
            ExcludeRules,
            LoadViewportAsync,
            RefreshMaximumScrollLine,
            NotifySearchFilterStateChanged,
            ClearFilterCommand.NotifyCanExecuteChanged,
            QueueWorkspaceSave,
            _viewportCancellation.Reset().Token);
    }
    private void NotifySearchFilterStateChanged()
    {
        OnPropertyChanged(nameof(SearchFilter.SearchStatus));
        OnPropertyChanged(nameof(SearchFilter.FilterStatus));
        OnPropertyChanged(nameof(SearchFilter.IsFilterActive));
        OnPropertyChanged(nameof(ExcludeRules.Status));
        _timelineWorkflow.Refresh();
    }
    private void OnExcludeRulesChanged()
    {
        ClearFilterCommand.NotifyCanExecuteChanged();
        if (_isRestoringWorkspace || _isSwitchingTabs || !Document.IsDocumentOpen)
        {
            QueueWorkspaceSave();
            return;
        }

        _tasks.Run(ApplyFilterAsync, "Exclude rules failed");
    }
    private void NotifySearchCommandsCanExecuteChanged()
    {
        SearchNextCommand.NotifyCanExecuteChanged();
        SearchPreviousCommand.NotifyCanExecuteChanged();
    }
    private void SelectTab(LogTabViewModel? tab)
    {
        if (tab is not null)
        {
            Document.SelectedTab = tab;
        }
    }
    private void CloseSelectedTab()
    {
        CloseTab(Document.SelectedTab);
    }
    private void CloseTab(LogTabViewModel? tab)
    {
        if (tab is null)
        {
            return;
        }
        var result = _documents.Close(tab);
        if (!result.WasActive)
        {
            QueueWorkspaceSave();
            return;
        }
        CancelDocumentOperations(includeOpen: false, includeViewport: true);
        if (result.NextActiveTab is not null)
        {
            Document.SelectedTab = result.NextActiveTab;
            return;
        }
        ClearActiveDocumentState();
        QueueWorkspaceSave();
    }
    public async Task CopySelectedLineAsync(IClipboard? clipboard)
    {
        var text = _bookmarks.GetTextToCopy(Document);
        if (clipboard is null || text is null)
        {
            return;
        }
        await clipboard.SetTextAsync(text);
        Document.Status = string.IsNullOrEmpty(Document.SelectedText)
            ? LogStatusFormatter.CopiedLine(Document.SelectedLineNumber)
            : LogStatusFormatter.CopiedSelection(Document.SelectedLineNumber);
    }
    private void SwitchToTab(LogTabViewModel tab)
    {
        SaveActiveTabState();
        _isSwitchingTabs = true;
        CancelDocumentOperations(includeOpen: false, includeViewport: true);
        try
        {
            _documents.SwitchTo(tab);
            _timelineWorkflow.Reset();
            Document.ActiveTabTitle = tab.Title;
            Document.IsDocumentOpen = true;
            Document.IsBusy = false;
            RestoreTabState(tab);
            OnPropertyChanged(nameof(ActiveTriggers));
            RefreshMaximumScrollLine();
            Document.ScrollLine = ClampFirstVisibleLine(ToLineNumber(tab.ScrollLine));
            NotifyNavigationCanExecuteChanged();
            _tasks.Run(LoadViewportFromScrollAsync, "Viewport refresh failed");
            StartTailLoop();
        }
        finally
        {
            _isSwitchingTabs = false;
        }
        QueueWorkspaceSave();
    }
    private void SaveActiveTabState()
    {
        _tabState.SaveActiveTabState(
            _documents.ActiveDocument.Tab,
            _documents.ActiveDocument.Session,
            _documents.ActiveDocument.Path,
            Document,
            SearchFilter,
            ExcludeRules,
            Bookmarks,
            _bookmarks.BookmarkStatus,
            _bookmarks.SelectedLineText);
    }
    private void RestoreTabState(LogTabViewModel tab)
    {
        _tabState.RestoreTabState(tab, Document, SearchFilter, ExcludeRules, _bookmarks, RefreshTailDisplayState);
    }
    private void ClearActiveDocumentState()
    {
        _documents.Clear();
        OnPropertyChanged(nameof(ActiveTriggers));
        _timelineWorkflow.Reset();
        Document.SelectedTab = null;
        Document.ActiveTabTitle = LogStatusFormatter.DropALogFile;
        ResetVisibleDocumentState();
        Document.IsBusy = false;
        Document.Status = LogStatusFormatter.NoLogFileOpened;
        NotifyNavigationCanExecuteChanged();
    }
    private void ResetVisibleDocumentState()
    {
        _tabState.ResetVisibleDocumentState(Document, Lines, SearchFilter, ExcludeRules, _bookmarks);
        RefreshTailDisplayState();
    }
    private void ToggleBookmark()
    {
        _bookmarkWorkflow.Toggle();
    }
    private void GoToNextBookmark()
    {
        _bookmarkWorkflow.GoToNext();
    }
    private void GoToPreviousBookmark()
    {
        _bookmarkWorkflow.GoToPrevious();
    }

    private void RemoveSelectedBookmark()
    {
        _bookmarkWorkflow.RemoveSelected();
    }

    private void ClearBookmarks()
    {
        _bookmarkWorkflow.Clear();
    }

    private void GoToPreviousTrigger()
    {
        _triggerWorkflow.GoToPrevious();
    }

    private void GoToNextTrigger()
    {
        _triggerWorkflow.GoToNext();
    }

    private void RemoveSelectedTrigger()
    {
        _triggerWorkflow.RemoveSelected();
    }

    private void ClearTriggers()
    {
        _triggerWorkflow.Clear();
    }

    private void OnTriggerNavigationRequested(long lineNumber)
    {
        _triggerWorkflow.NavigateToHit(lineNumber);
    }

    private void OnTriggersStateChanged()
    {
        _triggerWorkflow.RefreshState();
    }

    private Task ProcessTailUpdateAsync(
        LogDocumentSession session,
        LogTailUpdate update,
        CancellationToken cancellationToken)
    {
        return _triggerWorkflow.ScanTailUpdateAsync(session, update, cancellationToken);
    }

    private void RestoreActiveTabTriggers(IEnumerable<string> patterns)
    {
        _documents.ActiveDocument.Tab?.Triggers.RestoreRules(patterns);
    }
    private void RestoreBookmarks(IEnumerable<long> lineNumbers)
    {
        _bookmarkWorkflow.Restore(lineNumbers);
    }
    private void RefreshBookmarkCommands()
    {
        _bookmarkWorkflow.RefreshCommands();
    }

    private BookmarkCommandRefreshContext CreateBookmarkCommandRefreshContext()
    {
        return new BookmarkCommandRefreshContext(
            NextBookmarkCommand,
            PreviousBookmarkCommand,
            RemoveSelectedBookmarkCommand,
            ClearBookmarksCommand);
    }

    public void JumpToTimelineLine(long lineNumber)
    {
        _timelineWorkflow.JumpToLine(lineNumber);
    }

    private void ClearTimelineHighlights()
    {
        _timelineWorkflow.ClearCapturedHighlights();
    }

    private void AttachTab(LogTabViewModel tab)
    {
        _highlights.Attach(tab);
        tab.Triggers.NavigationRequested += OnTriggerNavigationRequested;
        tab.Triggers.StateChanged += OnTriggersStateChanged;
    }

    private void DetachTab(LogTabViewModel tab)
    {
        _highlights.Detach(tab);
        tab.Triggers.NavigationRequested -= OnTriggerNavigationRequested;
        tab.Triggers.StateChanged -= OnTriggersStateChanged;
    }
    public void SaveMainWindowPlacement(double width, double height, double x, double y)
    {
        _persistence.SaveMainWindowPlacement(width, height, x, y);
    }
    public void SaveSettingsWindowSize(double width, double height)
    {
        _persistence.SaveSettingsWindowSize(width, height);
    }
    public void QueueSettingsSave()
    {
        QueuePreferencesSave();
    }
    public Task SaveSettingsNowAsync()
    {
        return _persistence.SaveSettingsNowAsync();
    }

    public Task ExportSettingsAsync(string path)
    {
        return _settingsTransfer.ExportAsync(path);
    }

    public Task ImportSettingsAsync(string path)
    {
        return _settingsTransfer.ImportAsync(path);
    }
    private UserPreferences CreatePreferencesSnapshot()
    {
        return UserPreferencesMapper.Create(new UserPreferencesSnapshot(
            DisplaySettings,
            Document.IsStructuredMode,
            RecentFilePaths,
            [.. _highlightRules.ToWorkspaceRules()]));
    }
    private Task<bool> RestorePreferencesAsync()
        => _persistence.RestorePreferencesAsync(ApplyPreferences);
    private void ApplyPreferences(UserPreferences preferences)
    {
        DisplaySettings.Apply(preferences);
        Document.IsStructuredMode = preferences.IsStructuredMode;
        _highlights.RestoreGlobal(preferences.HighlightRulesConfigured, preferences.HighlightRules);
        RestoreRecentFiles(preferences.RecentFiles);
    }
    private void RestoreLegacyPreferences(WorkspaceState state)
    {
        DisplaySettings.ApplyLegacy(state);
        _highlights.RestoreGlobal(state);
    }
    private void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        QueuePreferencesSave();
    }
    private void QueuePreferencesSave()
    {
        _persistence.QueuePreferencesSave();
    }
    private Task OpenRecentFileAsync(string path)
    {
        if (!File.Exists(path))
        {
            RemoveRecentFile(path);
            Document.Status = LogStatusFormatter.RecentFileNotFound(path);
            return Task.CompletedTask;
        }
        return OpenFilePathAsync(path);
    }
    private void TrackRecentFile(string path)
    {
        _persistence.TrackRecentFile(path);
        OnPropertyChanged(nameof(HasRecentFiles));
    }
    private void RemoveRecentFile(string path, bool queueSave = true)
    {
        if (!_persistence.RemoveRecentFile(path, queueSave))
        {
            return;
        }
        OnPropertyChanged(nameof(HasRecentFiles));
    }
    private void RestoreRecentFiles(IEnumerable<string> paths)
    {
        _persistence.RestoreRecentFiles(paths);
        OnPropertyChanged(nameof(HasRecentFiles));
    }
    private void QueueWorkspaceSave()
    {
        _persistence.QueueWorkspaceSave();
    }
    private WorkspaceState CreateWorkspaceState()
    {
        SaveActiveTabState();
        return WorkspaceStateMapper.Create(
            _documents.ActiveDocument.Tab?.Path,
            Tabs);
    }
}


