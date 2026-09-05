namespace Needle.Avalonia.ViewModels;

using System.Collections.ObjectModel;
using Needle.Core.Search;

public sealed class SearchFilterViewModel : ViewModelBase
{
    private string _searchText = string.Empty;
    private bool _isSearchCaseSensitive;
    private bool _isSearchRegex;
    private string _searchStatus = string.Empty;
    private string _filterText = string.Empty;
    private string _filterStatus = string.Empty;
    private bool _isFilterActive;

    public event Action<string>? SearchTextChanged;

    public event Action<bool>? SearchCaseSensitiveChanged;

    public event Action<bool>? SearchRegexChanged;

    public event Action<string>? FilterTextChanged;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                SearchTextChanged?.Invoke(value);
            }
        }
    }

    public bool IsSearchCaseSensitive
    {
        get => _isSearchCaseSensitive;
        set
        {
            if (SetProperty(ref _isSearchCaseSensitive, value))
            {
                SearchCaseSensitiveChanged?.Invoke(value);
            }
        }
    }

    public bool IsSearchRegex
    {
        get => _isSearchRegex;
        set
        {
            if (SetProperty(ref _isSearchRegex, value))
            {
                SearchRegexChanged?.Invoke(value);
            }
        }
    }

    public string SearchStatus
    {
        get => _searchStatus;
        set => SetProperty(ref _searchStatus, value);
    }

    public string FilterText
    {
        get => _filterText;
        set
        {
            if (SetProperty(ref _filterText, value))
            {
                FilterTextChanged?.Invoke(value);
            }
        }
    }

    public string FilterStatus
    {
        get => _filterStatus;
        set => SetProperty(ref _filterStatus, value);
    }

    public bool IsFilterActive
    {
        get => _isFilterActive;
        set => SetProperty(ref _isFilterActive, value);
    }

    public ObservableCollection<LogSearchMatch> SearchMatches { get; } = [];

    public ObservableCollection<long> FilteredLineNumbers { get; } = [];

    public void Clear()
    {
        SearchMatches.Clear();
        FilteredLineNumbers.Clear();
        SearchText = string.Empty;
        SearchStatus = string.Empty;
        FilterText = string.Empty;
        FilterStatus = string.Empty;
        IsFilterActive = false;
    }
}
