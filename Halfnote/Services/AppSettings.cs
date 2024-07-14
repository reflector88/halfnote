using System;
using System.Collections.Generic;

namespace Halfnote.Services;

public class AppSettings
{
    public string Directory { get; set; } = "C:/Users/" + Environment.UserName + "/Documents";
    public string Theme { get; set; } = "Light";
    public int LastNotebookIndex { get; set; } = 0;

    public Dictionary<string, bool> UILayout { get; set; } =
        new Dictionary<string, bool>()
        {
            { "Menu", true },
            { "Sidebar", true },
            { "Titlebar", true },
            { "Status Bar", true }
        };

    public Dictionary<string, int> LastPageIndices { get; set; } =
        new Dictionary<string, int>() { { "Notebook1", 0 } };

    public string EditorFont { get; set; } = "Cascadia Code,Consolas,Menlo,Monospace";
    public int EditorFontSize { get; set; } = 14;
    public int AutosaveInterval { get; set; } = 60;
    public float PreviewDelay { get; set; } = 0.5f;
    public bool Syntax { get; set; } = true;
    public bool Wrap { get; set; } = true;
    public bool LineNumbers { get; set; } = false;

    public AppSettings()
    {
        if (OperatingSystem.IsLinux())
        {
            Directory = "/home/" + Environment.UserName + "/Documents";
        }
        else if (OperatingSystem.IsMacOS())
        {
            Directory = "/Users/" + Environment.UserName + "/Documents";
        }
    }
}
