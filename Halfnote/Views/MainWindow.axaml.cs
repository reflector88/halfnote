using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Messaging;
using DialogHostAvalonia;
using Halfnote.Services;
using Halfnote.ViewModels;

namespace Halfnote.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _mainViewModel;
    public string DialogType = "Add";
    private readonly FileService _fs;
    private float _lastSidebarWidth = 140;
    private bool _isControlPressed = false;

    public MainWindow() { }

    public MainWindow(FileService fileService)
    {
        InitializeComponent();
        MenuBar.ParentWindow = this;
        TitleBar.ParentWindow = this;
        Sidebar.ParentWindow = this;
        MDPreview.ParentWindow = this;
        Editor.ParentWindow = this;

        _fs = fileService;
        DataContext = new MainViewModel(_fs);
        _mainViewModel = (MainViewModel)DataContext;

        WeakReferenceMessenger.Default.Register<ClearUndoStackMessage>(
            this,
            (r, m) =>
            {
                Editor.ClearUndoStack();
            }
        );
    }

    public void EditorView()
    {
        MDPreview.previewer.IsVisible = false;
        SetColumnWidths(1f, 0, 0);
        EditorSplitter.IsEnabled = false;
    }

    public void SplitView()
    {
        SetColumnWidths(0.5f, 2, 0.5f);
        MDPreview.previewer.IsVisible = true;

        EditorSplitter.IsEnabled = true;
    }

    public void PreviewView()
    {
        SetColumnWidths(0, 0, 1f);
        MDPreview.previewer.IsVisible = true;

        EditorSplitter.IsEnabled = false;
    }

    void SetColumnWidths(float editorWidth, int splitterWidth, float previewWidth)
    {
        EditorWindow.ColumnDefinitions[0].Width = new GridLength(editorWidth, GridUnitType.Star);
        EditorWindow.ColumnDefinitions[1].Width = new GridLength(splitterWidth);
        EditorWindow.ColumnDefinitions[2].Width = new GridLength(previewWidth, GridUnitType.Star);
    }

    public void SidebarView()
    {
        var sidebarWidth = MiddleWindow.ColumnDefinitions[0].Width.Value;

        if (sidebarWidth == 0)
        {
            MiddleWindow.ColumnDefinitions[0].Width = new GridLength(_lastSidebarWidth);
            SidebarSplitter.IsEnabled = true;
        }
        else
        {
            _lastSidebarWidth = (float)sidebarWidth;
            MiddleWindow.ColumnDefinitions[0].Width = new GridLength(0);
            SidebarSplitter.IsEnabled = false;
        }
    }

    private void NotebookDialog_OnDialogClosing(object? sender, DialogClosingEventArgs e)
    {
        if (e.Parameter == null)
            return;

        switch (DialogType)
        {
            case "Add":
                _mainViewModel.AddNotebook((string)e.Parameter ?? string.Empty);
                break;

            case "Rename":
                _mainViewModel.RenameNotebook((string)e.Parameter ?? string.Empty);
                break;
        }
    }

    public async void OpenFolderPicker()
    {
        var folder = await StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions { Title = "Choose file directory", AllowMultiple = false }
        );

        if (folder.Count >= 1)
        {
            string folderPath = folder[0].Path.LocalPath;
            _fs.SetRootPath(folderPath);
        }
    }

    public async void SaveFilePicker()
    {
        var mdFileType = new FilePickerFileType("Markdown Files")
        {
            Patterns = new[] { "*.md" },
            AppleUniformTypeIdentifiers = new[] { "net.daringfireball.markdown" },
            MimeTypes = new[] { "text/markdown" }
        };

        var file = await StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                SuggestedFileName = TitleBar.TextBox1.Text,
                FileTypeChoices = [FilePickerFileTypes.TextPlain, mdFileType]
            }
        );
        if (file != null)
        {
            _fs.Export(file.Path.LocalPath, Editor.editor.Document.Text);
        }
    }

    public void ToggleTheme()
    {
        if (Application.Current is App app)
        {
            string theme = app.GetTheme();

            if (theme == "Light")
            {
                app.LoadTheme("Dark");
                _fs.AppSettings.Theme = "Dark";
            }
            else
            {
                app.LoadTheme("Light");
                _fs.AppSettings.Theme = "Light";
            }
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isControlPressed = true;
        }

        if (e.Key == Key.B && _isControlPressed)
        {
            SidebarView();
        }

        if (e.Key == Key.T && _isControlPressed)
        {
            ToggleTheme();
        }
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isControlPressed = false;
        }
    }

    public void SavePage()
    {
        _mainViewModel.SavePage();
    }
}
