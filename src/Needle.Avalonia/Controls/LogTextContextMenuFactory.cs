namespace Needle.Avalonia.Controls;

using global::Avalonia.Controls;
using Needle.Avalonia.ViewModels;

internal static class LogTextContextMenuFactory
{
    public static ContextMenu Create(LogTextView owner)
    {
        var copyItem = new MenuItem { Header = "Copy" };
        copyItem.Click += async (_, e) =>
        {
            await owner.CopySelectionAsync(TopLevel.GetTopLevel(owner)?.Clipboard);
            e.Handled = true;
        };

        var bookmarkItem = new MenuItem { Header = "Toggle Bookmark" };
        bookmarkItem.Click += (_, e) =>
        {
            if (owner.DataContext is MainViewModel viewModel
                && viewModel.ToggleBookmarkCommand.CanExecute(null))
            {
                viewModel.ToggleBookmarkCommand.Execute(null);
            }

            e.Handled = true;
        };

        var openInEditorItem = new MenuItem { Header = "Open in Editor" };
        openInEditorItem.Click += (_, e) =>
        {
            if (owner.DataContext is MainViewModel viewModel
                && viewModel.OpenInEditorCommand.CanExecute(null))
            {
                viewModel.OpenInEditorCommand.Execute(null);
            }

            e.Handled = true;
        };

        var showInExplorerItem = new MenuItem { Header = "Show in Explorer" };
        showInExplorerItem.Click += (_, e) =>
        {
            if (owner.DataContext is MainViewModel viewModel
                && viewModel.ShowInExplorerCommand.CanExecute(null))
            {
                viewModel.ShowInExplorerCommand.Execute(null);
            }

            e.Handled = true;
        };

        return new ContextMenu
        {
            ItemsSource = new Control[]
            {
                copyItem,
                new Separator(),
                bookmarkItem,
                new Separator(),
                openInEditorItem,
                showInExplorerItem
            }
        };
    }
}
