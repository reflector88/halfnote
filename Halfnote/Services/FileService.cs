using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Halfnote.Services;

public class FileService : IFileService
{
    public AppSettings AppSettings { get; set; }
    private string _appDataFolderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Halfnote"
    );
    private string _rootPath = "C:/Users/" + Environment.UserName + "/Documents";
    private string _notebooksPath;
    private string _trashPath;

    public FileService()
    {
        InitializeRootPath();
    }

    private async Task LoadSettings()
    {
        string settingsPath = Path.Combine(_appDataFolderPath, "appsettings.json");

        if (File.Exists(settingsPath))
        {
            using (var fs = File.OpenRead(settingsPath))
            {
                AppSettings = await JsonSerializer.DeserializeAsync<AppSettings>(fs);
            }
        }
        else
        {
            Directory.CreateDirectory(_appDataFolderPath);
            using (var fs = File.Create(settingsPath))
            {
                AppSettings = new AppSettings();
                await JsonSerializer.SerializeAsync(fs, AppSettings);
            }
        }
    }

    public async Task SaveSettings()
    {
        string settingsPath = Path.Combine(_appDataFolderPath, "appsettings.json");

        if (File.Exists(settingsPath))
        {
            using (
                var fs = new FileStream(
                    settingsPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None
                )
            )
            {
                await JsonSerializer.SerializeAsync(fs, AppSettings);
            }
        }
    }

    public async void InitializeRootPath()
    {
        await LoadSettings();

        _rootPath = AppSettings.Directory + "/Halfnote";
        _notebooksPath = Path.Combine(_rootPath, "Notebooks");
        _trashPath = Path.Combine(_rootPath, "Trash");

        if (!Directory.Exists(_rootPath))
        {
            Directory.CreateDirectory(_rootPath);
            Directory.CreateDirectory(_notebooksPath);
            Directory.CreateDirectory(_trashPath);
            await NewFolder("Notebook1");
        }
    }

    public void SetRootPath(string directory)
    {
        AppSettings.Directory = directory;
        string oldRoot = _rootPath;
        _rootPath = directory + "/Halfnote";
        _notebooksPath = Path.Combine(_rootPath, "Notebooks");
        _trashPath = Path.Combine(_rootPath, "Trash");

        Directory.Move(oldRoot, _rootPath);
    }

    public async Task SaveFile(string notebook, string pageTitle, string pageBody)
    {
        string newPath = Path.Combine(_notebooksPath, notebook, pageTitle + ".txt");

        await File.WriteAllTextAsync(newPath, pageBody);
    }

    public async Task<string> LoadFile(string notebook, string pageTitle)
    {
        try
        {
            string newPath = Path.Combine(_notebooksPath, notebook, pageTitle + ".txt");

            return await File.ReadAllTextAsync(newPath);
        }
        catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException)
        {
            return null;
        }
    }

    public void RenameFile(string notebook, string pageTitle, string newName)
    {
        string oldFile = Path.Combine(_notebooksPath, notebook, pageTitle + ".txt");
        string newFile = Path.Combine(_notebooksPath, notebook, newName + ".txt");

        if (File.Exists(newFile))
        {
            Console.WriteLine("File of that name already exists in current notebook.");
            return;
        }

        if (!File.Exists(oldFile))
        {
            Console.WriteLine("File does not exist.");
            return;
        }

        File.Move(oldFile, newFile);
    }

    public void RenameFolder(string notebook, string newName)
    {
        string oldFolder = Path.Combine(_notebooksPath, notebook);
        string newFolder = Path.Combine(_notebooksPath, newName);

        if (Directory.Exists(newFolder))
        {
            Console.WriteLine("File of that name already exists in current notebook.");
            return;
        }

        if (!Directory.Exists(oldFolder))
        {
            Console.WriteLine("File does not exist.");
            return;
        }

        Directory.Move(oldFolder, newFolder);
    }

    public void DeleteFile(string notebook, string pageTitle, int pageIndex)
    {
        string newPath = Path.Combine(_notebooksPath, notebook, pageTitle + ".txt");

        if (File.Exists(newPath))
        {
            if (!Directory.Exists(_trashPath))
                Directory.CreateDirectory(_trashPath);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string uniqueFileName = pageTitle + "_" + timestamp + ".txt";

            File.Move(newPath, Path.Combine(_trashPath, uniqueFileName));

            ReindexFollowing(notebook, pageIndex, "Delete");
        }
    }

    public void ReindexFollowing(string notebook, int pageIndex, string type)
    {
        string[] pages = Directory.GetFiles(Path.Combine(_notebooksPath, notebook));
        pages = ProcessPages(pages);

        for (int i = pageIndex, k = (type == "Delete") ? 0 : 1; i < pages.Length; i++, k++)
        {
            string nameSansIndex = pages[i].Substring(pages[i].IndexOf('_') + 1);
            string newName = (pageIndex + k) + "_" + nameSansIndex;
            RenameFile(notebook, pages[i], newName);
        }
    }

    public void DeleteFolder(string notebook)
    {
        string newPath = Path.Combine(_notebooksPath, notebook);

        if (Directory.Exists(newPath))
        {
            if (!Directory.Exists(_trashPath))
                Directory.CreateDirectory(_trashPath);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string uniqueFolderName = notebook + "_" + timestamp;

            Directory.Move(newPath, Path.Combine(_trashPath, uniqueFolderName));
        }
    }

    public void EmptyTrash()
    {
        Directory.Delete(_trashPath, true);
        Directory.CreateDirectory(_trashPath);
    }

    public async Task NewFolder(string name)
    {
        string newPath = Path.Combine(_notebooksPath, name);
        Directory.CreateDirectory(newPath);
        newPath = Path.Combine(newPath, "0_Untitled.txt");
        await File.WriteAllTextAsync(newPath, "");
    }

    public void Reorder(string notebook, string pageTitle, int sourceIndex, int destIndex)
    {
        string sourcePath = Path.Combine(_notebooksPath, notebook, pageTitle + ".txt");

        if (File.Exists(sourcePath))
        {
            string[] pages = Directory.GetFiles(Path.Combine(_notebooksPath, notebook));
            pages = ProcessPages(pages);

            string sourceNameSansIndex = pages[sourceIndex]
                .Substring(pages[sourceIndex].IndexOf('_') + 1);
            string newSourceName = (destIndex) + "_" + sourceNameSansIndex;

            string destNameSansIndex = pages[destIndex]
                .Substring(pages[destIndex].IndexOf('_') + 1);
            string newDestName = (sourceIndex) + "_" + destNameSansIndex;

            RenameFile(notebook, pages[sourceIndex], newSourceName);
            RenameFile(notebook, pages[destIndex], newDestName);
        }
    }

    public string[] GetNotebooks()
    {
        string[] notebooks = Directory.GetDirectories(_notebooksPath);

        for (int i = 0; i < notebooks.Length; i++)
        {
            notebooks[i] = Path.GetFileNameWithoutExtension(notebooks[i]);
        }

        return notebooks;
    }

    public string[] GetPagesWithoutIndices(string name)
    {
        string[] pages = Directory.GetFiles(Path.Combine(_notebooksPath, name));

        pages = ProcessPages(pages);

        for (int i = 0; i < pages.Length; i++)
        {
            pages[i] = pages[i].Substring(pages[i].IndexOf('_') + 1);
        }

        return pages;
    }

    public async void Export(string filePath, string pageBody)
    {
        await File.WriteAllTextAsync(filePath, pageBody);
    }

    private static string[] ProcessPages(string[] pages)
    {
        // OS orders 10 before 2 by default
        var orderedPages = pages.OrderBy(file =>
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string numberPart = new string(fileName.TakeWhile(char.IsDigit).ToArray());

            if (int.TryParse(numberPart, out int number))
                return number;

            return int.MaxValue;
        });

        pages = orderedPages.ToArray();

        for (int i = 0; i < pages.Length; i++)
        {
            pages[i] = Path.GetFileNameWithoutExtension(pages[i]);
        }

        return pages;
    }
}
