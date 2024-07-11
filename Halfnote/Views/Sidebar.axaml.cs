using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using DialogHostAvalonia;

namespace Halfnote.Views;

/// <summary>
/// Displays and handles organization of notebooks and notes.
/// </summary>
public partial class Sidebar : UserControl
{
    public MainWindow ParentWindow { get; set; }

    public Sidebar()
    {
        InitializeComponent();
    }

    private async void OpenAddNotebookDialog(object? sender, RoutedEventArgs e)
    {
        if (ParentWindow.MainDialogHost.IsOpen)
            return;

        ParentWindow.DialogType = "Add";

        await DialogHost.Show(Resources["NotebookDialog"]!, "MainDialogHost");
    }

    private async void OpenRenameNotebookDialog(object? sender, RoutedEventArgs e)
    {
        if (ParentWindow.MainDialogHost.IsOpen)
            return;

        ParentWindow.DialogType = "Rename";

        await DialogHost.Show(Resources["NotebookDialog"]!, "MainDialogHost");
    }
}
