namespace Needle.Avalonia.ViewModels;

public sealed class DocumentStateViewModel : ViewModelBase
{
    public const int DefaultViewportLineCount = 100;

    private string _title = "Needle";
    private string _status = LogStatusFormatter.NoLogFileOpened;
    private string _activeTabTitle = LogStatusFormatter.DropALogFile;
    private LogTabViewModel? _selectedTab;
    private bool _hasLines;
    private bool _isDocumentOpen;
    private bool _isBusy;
    private double _scrollLine;
    private double _maximumScrollLine;
    private double _timelineScrollLine;
    private long _totalLineCount;
    private string _viewportRange = string.Empty;
    private int _viewportLineCount = DefaultViewportLineCount;
    private int _timelineViewportLineCount = DefaultViewportLineCount;
    private bool _isTailPaused;
    private bool _isFollowingTail = true;
    private string _followTailButtonText = "Follow Tail";
    private string _bookmarkCountText = "0";
    private bool _isStructuredMode;
    private long _selectedLineNumber;
    private string _selectedText = string.Empty;

    public event Action<LogTabViewModel?>? SelectedTabChanged;

    public event Action<double>? ScrollLineChanged;

    public event Action<long>? SelectedLineNumberChanged;

    public event Action<bool>? StructuredModeChanged;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string ActiveTabTitle
    {
        get => _activeTabTitle;
        set => SetProperty(ref _activeTabTitle, value);
    }

    public LogTabViewModel? SelectedTab
    {
        get => _selectedTab;
        set
        {
            if (SetProperty(ref _selectedTab, value))
            {
                SelectedTabChanged?.Invoke(value);
            }
        }
    }

    public bool HasLines
    {
        get => _hasLines;
        set => SetProperty(ref _hasLines, value);
    }

    public bool IsDocumentOpen
    {
        get => _isDocumentOpen;
        set => SetProperty(ref _isDocumentOpen, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public double ScrollLine
    {
        get => _scrollLine;
        set
        {
            if (SetProperty(ref _scrollLine, value))
            {
                ScrollLineChanged?.Invoke(value);
            }
        }
    }

    public double MaximumScrollLine
    {
        get => _maximumScrollLine;
        set => SetProperty(ref _maximumScrollLine, value);
    }

    public double TimelineScrollLine
    {
        get => _timelineScrollLine;
        set => SetProperty(ref _timelineScrollLine, value);
    }

    public long TotalLineCount
    {
        get => _totalLineCount;
        set => SetProperty(ref _totalLineCount, value);
    }

    public string ViewportRange
    {
        get => _viewportRange;
        set => SetProperty(ref _viewportRange, value);
    }

    public int ViewportLineCount
    {
        get => _viewportLineCount;
        set => SetProperty(ref _viewportLineCount, value);
    }

    public int TimelineViewportLineCount
    {
        get => _timelineViewportLineCount;
        set => SetProperty(ref _timelineViewportLineCount, Math.Max(1, value));
    }

    public bool IsTailPaused
    {
        get => _isTailPaused;
        set => SetProperty(ref _isTailPaused, value);
    }

    public bool IsFollowingTail
    {
        get => _isFollowingTail;
        set => SetProperty(ref _isFollowingTail, value);
    }

    public string FollowTailButtonText
    {
        get => _followTailButtonText;
        set => SetProperty(ref _followTailButtonText, value);
    }

    public string BookmarkCountText
    {
        get => _bookmarkCountText;
        set => SetProperty(ref _bookmarkCountText, value);
    }

    public bool IsStructuredMode
    {
        get => _isStructuredMode;
        set
        {
            if (SetProperty(ref _isStructuredMode, value))
            {
                StructuredModeChanged?.Invoke(value);
            }
        }
    }

    public long SelectedLineNumber
    {
        get => _selectedLineNumber;
        set
        {
            if (SetProperty(ref _selectedLineNumber, value))
            {
                SelectedLineNumberChanged?.Invoke(value);
            }
        }
    }

    public string SelectedText
    {
        get => _selectedText;
        set => SetProperty(ref _selectedText, value);
    }

    public void ResetVisible()
    {
        HasLines = false;
        IsDocumentOpen = false;
        IsBusy = false;
        ScrollLine = 0;
        MaximumScrollLine = 0;
        TimelineScrollLine = 0;
        TotalLineCount = 0;
        ViewportRange = string.Empty;
        TimelineViewportLineCount = DefaultViewportLineCount;
        SelectedLineNumber = 0;
        SelectedText = string.Empty;
        IsTailPaused = false;
        IsFollowingTail = true;
        FollowTailButtonText = "Follow Tail: On";
        IsStructuredMode = false;
    }
}
