using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Halfnote.Services;

namespace Halfnote.ViewModels;

//TODO Custom Shortcuts
//dotnet publish -c Release -r win-x64 --self-contained

public partial class MainViewModel : ObservableObject
{
    private readonly IFileService _fs;
    private string _actualPageTitle = "";
    private string _actualNotebookName = "";
    private int _priorNotebookIndex = 0;
    private bool _doNotSaveFlag = false;
    private bool _doNotUpdateFlag = false;
    private DispatcherTimer _autosaveTimer;
    private DispatcherTimer _messageTimer;
    private DispatcherTimer _previewTimer;

    public ObservableCollection<string> Notebooks { get; set; } = [];
    public ObservableCollection<string> Pages { get; set; } = [];

    [ObservableProperty]
    private int _notebookIndex = 0;

    [ObservableProperty]
    private int _pageIndex = 0;

    [ObservableProperty]
    private string _previewText = "*Markdown Preview*";

    [ObservableProperty]
    private TextDocument _currentDocument = new TextDocument("");

    [ObservableProperty]
    private string _statusMessage = "";

    [ObservableProperty]
    private string _titleBarText = "";

    [ObservableProperty]
    private bool _titleBarEnabled = false;

    public MainViewModel() { }

    public MainViewModel(IFileService fileService)
    {
        _fs = fileService;
        PropertyChanged += OnPropertyChanged;
        CurrentDocument.TextChanged += StartPreviewTimer;
        InitializeHighlighting();
        LoadAppPreferences();
        InitializeCollections();
        InitializeTimers();
        LoadPage();
    }

    private void CheckNotebookIntegrity()
    {
        var settingNotebooks = _fs.AppSettings.LastPageIndices.Keys.ToList();
        settingNotebooks.Sort();

        var programNotebooks = Notebooks.ToList();

        if (!Enumerable.SequenceEqual(settingNotebooks, programNotebooks))
        {
            RebuildAppSettings();
        }
    }

    private void CheckPageIntegrity(string notebookName)
    {
        if (_fs.AppSettings.LastPageIndices[notebookName] >= Pages.Count)
        {
            RebuildAppSettings();
        }
    }

    private void InitializeCollections()
    {
        int _lastNotebookIndex = _fs.AppSettings.LastNotebookIndex;
        Notebooks = new ObservableCollection<string>(_fs.GetNotebooks());
        CheckNotebookIntegrity();
        _actualNotebookName = Notebooks[_lastNotebookIndex];

        Pages = new ObservableCollection<string>(_fs.GetPagesWithoutIndices(_actualNotebookName));
        CheckPageIntegrity(Notebooks[_fs.AppSettings.LastNotebookIndex]);

        ChangePageIndex(_fs.AppSettings.LastPageIndices[Notebooks[_lastNotebookIndex]]);
        NotebookIndex = _fs.AppSettings.LastNotebookIndex;

        _actualPageTitle = PageIndex + "_" + Pages[PageIndex];
        TitleBarText = Pages[PageIndex];
    }

    private void InitializeTimers()
    {
        _autosaveTimer = new DispatcherTimer(
            TimeSpan.FromSeconds(OptionAutosaveInterval),
            DispatcherPriority.Background,
            DoAutoSave
        );
        _messageTimer = new DispatcherTimer(
            TimeSpan.FromSeconds(3),
            DispatcherPriority.Background,
            ClearMessage
        );
        _messageTimer.Stop();

        _previewTimer = new DispatcherTimer(
            TimeSpan.FromSeconds(OptionPreviewDelay),
            DispatcherPriority.Background,
            UpdatePreviewText
        );
        _previewTimer.Stop();
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnAppSettingsChanged(e);

        if (e.PropertyName == nameof(PageIndex))
        {
            if (!_doNotSaveFlag)
            {
                SavePage();
            }

            CurrentDocument.Text = "";

            _actualPageTitle = PageIndex + "_" + Pages[PageIndex];
            TitleBarText = Pages[PageIndex];

            StatusMessage = "";
            await LoadPage();
            PreviewText = CurrentDocument.Text;
            WeakReferenceMessenger.Default.Send(new ClearUndoStackMessage());
            _fs.AppSettings.LastPageIndices[Notebooks[NotebookIndex]] = PageIndex;

            _doNotSaveFlag = false;
        }

        if (e.PropertyName == nameof(NotebookIndex))
        {
            if (_doNotUpdateFlag)
            {
                _doNotUpdateFlag = false;
                return;
            }

            if (NotebookIndex == -1)
                NotebookIndex = 0;

            _fs.AppSettings.LastPageIndices[Notebooks[_priorNotebookIndex]] = PageIndex;

            UpdatePageList();
            _actualPageTitle = PageIndex + "_" + Pages[PageIndex];
            TitleBarText = Pages[PageIndex];
            await LoadPage();
            PreviewText = CurrentDocument.Text;

            _fs.AppSettings.LastNotebookIndex = NotebookIndex;
            _priorNotebookIndex = NotebookIndex;
        }
    }

    private void RebuildAppSettings()
    {
        _fs.AppSettings.LastPageIndices = new Dictionary<string, int>();

        foreach (var notebook in Notebooks)
        {
            _fs.AppSettings.LastPageIndices.Add(notebook, 0);
        }
        _fs.SaveSettings();
    }

    private void UpdatePageList()
    {
        _doNotSaveFlag = true;
        PageIndex = 0;

        for (int i = Pages.Count - 1; i > 0; i--)
        {
            _doNotSaveFlag = true;
            Pages.RemoveAt(i);
        }
        if (NotebookIndex >= Notebooks.Count)
            NotebookIndex = 0;
        _actualNotebookName = Notebooks[NotebookIndex];
        string[] fileNames = _fs.GetPagesWithoutIndices(_actualNotebookName);
        Pages[0] = fileNames[0];
        for (int i = 1; i < fileNames.Length; i++)
        {
            _doNotSaveFlag = true;
            Pages.Add(fileNames[i]);
        }
        if (_fs.AppSettings.LastPageIndices.ContainsKey(Notebooks[NotebookIndex]))
        {
            CheckPageIntegrity(_actualNotebookName);
            ChangePageIndex(_fs.AppSettings.LastPageIndices[Notebooks[NotebookIndex]]);
        }
        else
        {
            _fs.AppSettings.LastPageIndices.Add(Notebooks[NotebookIndex], 0);
        }
    }

    private void DoAutoSave(object sender, EventArgs e)
    {
        SavePage();
    }

    private void ChangePageIndex(int index)
    {
        _doNotSaveFlag = true;
        PageIndex = index;
    }

    private void ChangePage(int index, string value)
    {
        _doNotSaveFlag = true;
        Pages[index] = value;
    }

    [RelayCommand]
    public void EmptyTrash()
    {
        _fs.EmptyTrash();
        DisplayMessage("Trash folder was emptied");
    }

    [RelayCommand]
    public void RenamePage()
    {
        if (InvalidateName(TitleBarText))
        {
            TitleBarText = Pages[PageIndex];
            return;
        }
        int currentPageIndex = PageIndex;
        _fs.RenameFile(_actualNotebookName, _actualPageTitle, PageIndex + "_" + TitleBarText);
        ChangePage(PageIndex, TitleBarText);

        ChangePageIndex(currentPageIndex);
    }

    private void StartPreviewTimer(object args, EventArgs e)
    {
        _previewTimer.Stop();
        _previewTimer.Start();
    }

    private void UpdatePreviewText(object sender, EventArgs e)
    {
        _previewTimer.Stop();
        PreviewText = CurrentDocument.Text;
    }

    [RelayCommand]
    public void SavePage()
    {
        _fs.SaveFile(_actualNotebookName, _actualPageTitle, CurrentDocument.Text);
    }

    [RelayCommand]
    public async Task<string> LoadPage()
    {
        CurrentDocument.Text = await _fs.LoadFile(_actualNotebookName, _actualPageTitle);

        return CurrentDocument.Text;
    }

    [RelayCommand]
    public void AddPage()
    {
        SavePage();

        int newPageIndex = PageIndex + 1;
        _fs.ReindexFollowing(_actualNotebookName, newPageIndex, "Insert");

        int untitledCount = 0;

        foreach (string page in Pages)
        {
            if (page.Contains("Untitled"))
            {
                untitledCount++;
            }
        }
        string newName = "Untitled" + (untitledCount != 0 ? untitledCount : "");

        _actualPageTitle = newPageIndex + "_" + newName;

        Pages.Insert(newPageIndex, newName);
        CurrentDocument.Text = "";

        SavePage();
        ChangePageIndex(newPageIndex);

        WeakReferenceMessenger.Default.Send(new AddPageMessage());
    }

    [RelayCommand]
    public async Task DeletePage()
    {
        if (Pages.Count <= 1)
            return;

        string deletedPageTitle = _actualPageTitle;
        int deletedPageIndex = PageIndex;

        if (PageIndex >= Pages.Count - 1)
            ChangePageIndex(deletedPageIndex - 1);

        _fs.DeleteFile(_actualNotebookName, deletedPageTitle, PageIndex);
        _doNotSaveFlag = true;
        Pages.RemoveAt(deletedPageIndex);

        if (PageIndex < Pages.Count - 1)
            ChangePageIndex(deletedPageIndex);

        if (deletedPageIndex == 0)
        {
            _actualPageTitle = PageIndex + "_" + Pages[PageIndex];
            TitleBarText = Pages[PageIndex];
            await LoadPage();
        }
    }

    public async Task Import(string filePath)
    {
        string extension = Path.GetExtension(filePath);

        if (extension != ".txt" && extension != ".md")
        {
            DisplayMessage("Extension of type " + extension + " is not supported.");
            return;
        }

        string fileName = Path.GetFileNameWithoutExtension(filePath);

        AddPage();
        TitleBarText = fileName;
        RenamePage();
        CurrentDocument.Text = await _fs.LoadFile(filePath);
    }

    [RelayCommand]
    public async Task AddNotebook(string name)
    {
        if (Notebooks.Contains(name))
        {
            DisplayMessage("A file of that name already exists.");
            return;
        }

        if (InvalidateName(name))
            return;

        List<string> wordList = Notebooks.ToList();
        wordList.Add(name);
        wordList.Sort();
        int newIndex = wordList.IndexOf(name);

        Notebooks.Insert(newIndex, name);
        await _fs.NewFolder(name);
        NotebookIndex = newIndex;
    }

    [RelayCommand]
    public void DeleteNotebook()
    {
        if (Notebooks.Count <= 1)
            return;
        ChangePageIndex(0);
        int deletedNotebookIndex = NotebookIndex;
        _fs.AppSettings.LastPageIndices.Remove(Notebooks[NotebookIndex]);

        _fs.DeleteFolder(Notebooks[deletedNotebookIndex]);
        Notebooks.RemoveAt(deletedNotebookIndex);
        NotebookIndex = 0;
    }

    [RelayCommand]
    public void RenameNotebook(string newName)
    {
        if (Notebooks.Contains(newName))
        {
            DisplayMessage("A file of that name already exists.");
            return;
        }

        if (InvalidateName(newName))
            return;

        string oldName = _actualNotebookName;
        _fs.RenameFolder(_actualNotebookName, newName);

        int oldIndex = NotebookIndex;
        List<string> wordList = Notebooks.ToList();
        wordList.Remove(oldName);
        wordList.Add(newName);
        wordList.Sort();
        int newIndex = wordList.IndexOf(newName);

        _fs.AppSettings.LastPageIndices.Add(
            newName,
            _fs.AppSettings.LastPageIndices[_actualNotebookName]
        );
        _fs.AppSettings.LastPageIndices.Remove(_actualNotebookName);

        _actualNotebookName = newName;

        _doNotUpdateFlag = true;
        Notebooks.RemoveAt(oldIndex);

        _doNotUpdateFlag = true;
        Notebooks.Insert(newIndex, newName);
        NotebookIndex = newIndex;
    }

    [RelayCommand]
    public void PageUp()
    {
        if (PageIndex == 0)
            return;
        Reorder(-1);
    }

    [RelayCommand]
    public void PageDown()
    {
        if (PageIndex == Pages.Count - 1)
            return;
        Reorder(1);
    }

    private void Reorder(int dir)
    {
        SavePage();
        int sourceIndex = PageIndex;
        int destIndex = sourceIndex + dir;
        _fs.Reorder(_actualNotebookName, _actualPageTitle, sourceIndex, destIndex);
        string sourcePage = Pages[sourceIndex];
        string destPage = Pages[destIndex];

        ChangePage(destIndex, sourcePage);
        ChangePage(sourceIndex, destPage);
        ChangePageIndex(destIndex);
    }

    private bool InvalidateName(string name)
    {
        char[] invalid = [];

        if (OperatingSystem.IsWindows())
        {
            invalid = ['*', '\"', '\\', '/', '<', '>', ':', '|', '?'];
        }
        else if (OperatingSystem.IsLinux())
        {
            invalid = ['/'];
        }
        else if (OperatingSystem.IsMacOS())
        {
            invalid = ['/', ':'];
        }

        bool isInvalid = name.Any(x => invalid.Contains(x));

        if (isInvalid)
            DisplayMessage("Page name contains invalid characters");

        return isInvalid;
    }

    private void DisplayMessage(string message)
    {
        StatusMessage = message;

        _messageTimer.Start();
    }

    private void ClearMessage(object sender, EventArgs e)
    {
        StatusMessage = "";

        _messageTimer.Stop();
    }
}

public class ClearUndoStackMessage;

public class AddPageMessage;

public class DeletePageMessage;
