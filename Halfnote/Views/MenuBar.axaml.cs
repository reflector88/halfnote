using Avalonia.Controls;
using Avalonia.Interactivity;
using DialogHostAvalonia;

namespace Halfnote.Views
{
    public partial class MenuBar : UserControl
    {
        public MainWindow ParentWindow { get; set; }

        public MenuBar()
        {
            InitializeComponent();
        }

        private void EditorViewHandler(object sender, RoutedEventArgs e)
        {
            ParentWindow?.EditorView();
        }

        private void SplitViewHandler(object sender, RoutedEventArgs e)
        {
            ParentWindow?.SplitView();
        }

        private void PreviewViewHandler(object sender, RoutedEventArgs e)
        {
            ParentWindow?.PreviewView();
        }

        private void SidebarViewHandler(object sender, RoutedEventArgs e)
        {
            ParentWindow?.SidebarView();
        }

        private void MenuBarViewHandler(object sender, RoutedEventArgs e)
        {
            ParentWindow?.MenuBarView();
        }

        private void TitleBarViewHandler(object sender, RoutedEventArgs e)
        {
            ParentWindow?.TitleBarView();
        }

        private void StatusBarViewHandler(object sender, RoutedEventArgs e)
        {
            ParentWindow?.StatusBarView();
        }

        private void ToggleThemeHandler(object sender, RoutedEventArgs e)
        {
            ParentWindow?.ToggleTheme();
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

        private async void OpenPreferencesDialog(object? sender, RoutedEventArgs e)
        {
            if (ParentWindow.MainDialogHost.IsOpen)
                return;

            ParentWindow.DialogType = "Preferences";

            await DialogHost.Show(Resources["PreferencesDialog"]!, "MainDialogHost");
        }

        private void SetDirectory(object sender, RoutedEventArgs e)
        {
            ParentWindow.OpenFolderPicker();
        }

        private void Export(object sender, RoutedEventArgs e)
        {
            ParentWindow.SaveFilePicker();
        }

        private void Import(object sender, RoutedEventArgs e)
        {
            ParentWindow.OpenFilePicker();
        }
    }
}
