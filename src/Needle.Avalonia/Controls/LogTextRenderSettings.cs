namespace Needle.Avalonia.Controls;

using global::Avalonia.Media;

internal sealed record LogTextRenderSettings(
    double AvailableWidth,
    double AvailableHeight,
    double LineHeight,
    double GutterWidth,
    double TextFontSize,
    FontFamily TextFontFamily,
    IBrush TextBrush,
    IBrush LineNumberBrush,
    IBrush SelectedLineBrush,
    IBrush TextSelectionBrush,
    IBrush BookmarkBackgroundBrush,
    IBrush TriggerBackgroundBrush,
    IBrush? GutterBrush,
    long SelectedLineNumber);
