using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Needle.Avalonia.ViewModels;

namespace Needle.Avalonia.Views;

public partial class LogView : UserControl
{
    public LogView()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
        LinesViewport.SizeChanged += OnLinesViewportSizeChanged;
        LinesViewport.PointerPressed += OnLinesViewportPointerPressed;
        Timeline.JumpRequested += OnTimelineJumpRequested;
        LogText.PropertyChanged += OnLogTextPropertyChanged;
        PointerWheelChanged += OnPointerWheelChanged;
        KeyDown += OnKeyDown;
        AttachedToVisualTree += (_, _) => Focus();
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateViewportLineCount();
    }

    private void OnLinesViewportSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateViewportLineCount();
    }

    private void OnLogTextPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Controls.LogTextView.LineHeightProperty)
        {
            UpdateViewportLineCount();
        }
    }

    private void UpdateViewportLineCount()
    {
        var height = LinesViewport.Bounds.Height;
        if (height <= 0)
        {
            height = Bounds.Height;
        }

        ViewModel?.SetViewportLineCount(height, LogText.LineHeight);
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.MoveViewportByWheelDelta(e.Delta.Y);
        e.Handled = true;
    }

    private void OnLinesViewportPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        var point = e.GetCurrentPoint(LogText);
        var isLeftButton = point.Properties.IsLeftButtonPressed;
        var selected = LogText.SelectAt(point.Position, isLeftButton, isLeftButton ? e.Pointer : null);
        if (selected && isLeftButton)
        {
            e.Handled = true;
        }
    }

    private void OnTimelineJumpRequested(object? sender, Controls.LogTimelineJumpRequestedEventArgs e)
    {
        ViewModel?.JumpToTimelineLine(e.LineNumber);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Up:
                ViewModel.MoveViewportBy(-1);
                e.Handled = true;
                break;
            case Key.Down:
                ViewModel.MoveViewportBy(1);
                e.Handled = true;
                break;
            case Key.PageUp:
                ViewModel.PageUpCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.PageDown:
                ViewModel.PageDownCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Home:
                ViewModel.GoToTopCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.End:
                ViewModel.GoToEndCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.C when e.KeyModifiers.HasFlag(KeyModifiers.Control):
                AsyncEventRunner.Run(() => ViewModel.CopySelectedLineAsync(TopLevel.GetTopLevel(this)?.Clipboard));
                e.Handled = true;
                break;
        }
    }

    private MainViewModel? ViewModel => DataContext as MainViewModel;
}
