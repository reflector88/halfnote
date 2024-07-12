using System.Threading.Tasks;

namespace Halfnote.Services;

public interface IFileService
{
    public AppSettings AppSettings { get; set; }
    public Task<string> LoadFile(string filePath);
    public Task<string> LoadFile(string notebook, string pageTitle);
    public Task SaveFile(string notebook, string pageTitle, string pageBody);
    public void RenameFile(string notebook, string pageTitle, string newName);
    public void RenameFolder(string notebook, string newName);
    public void DeleteFile(string notebook, string pageTitle, int pageIndex);
    public void DeleteFolder(string notebook);
    public Task NewFolder(string name);
    public void EmptyTrash();
    public void ReindexFollowing(string notebook, int pageIndex, string type);
    public void Reorder(string notebook, string pageTitle, int sourceIndex, int destIndex);
    public void Export(string filePath, string pageBody);
    public Task SaveSettings();
    public string[] GetNotebooks();
    public string[] GetPagesWithoutIndices(string pageTitle);
}
