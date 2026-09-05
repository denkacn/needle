namespace Needle.Avalonia.ViewModels;

using System.Collections.ObjectModel;

public sealed class AddLogFontViewModel : ViewModelBase
{
    private readonly IReadOnlyList<string> _systemFontNames;
    private string _searchText = string.Empty;
    private string? _selectedFontName;

    public AddLogFontViewModel(IReadOnlyList<string> systemFontNames, IEnumerable<string> existingFontNames)
    {
        var existingFonts = existingFontNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        _systemFontNames = systemFontNames
            .Where(font => !existingFonts.Contains(font))
            .ToArray();

        RefreshFilteredFonts();
    }

    public ObservableCollection<string> FilteredFontNames { get; } = [];

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                RefreshFilteredFonts();
            }
        }
    }

    public string? SelectedFontName
    {
        get => _selectedFontName;
        set
        {
            if (SetProperty(ref _selectedFontName, value))
            {
                OnPropertyChanged(nameof(CanAccept));
            }
        }
    }

    public bool CanAccept => !string.IsNullOrWhiteSpace(SelectedFontName);

    private void RefreshFilteredFonts()
    {
        var search = SearchText.Trim();
        var selectedFont = SelectedFontName;
        var matchingFonts = string.IsNullOrWhiteSpace(search)
            ? _systemFontNames
            : _systemFontNames.Where(font => font.Contains(search, StringComparison.CurrentCultureIgnoreCase));

        FilteredFontNames.Clear();
        foreach (var fontName in matchingFonts.Take(250))
        {
            FilteredFontNames.Add(fontName);
        }

        if (selectedFont is not null && !FilteredFontNames.Contains(selectedFont))
        {
            SelectedFontName = null;
        }
    }
}
