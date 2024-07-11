using System;
using System.Collections.Generic;

namespace Halfnote.Services
{
    public class AppSettings
    {
        public string Directory { get; set; } = "C:/Users/" + Environment.UserName + "/Documents";
        public string Theme { get; set; } = "Light";
        public int LastNotebookIndex { get; set; } = 0;
        public Dictionary<string, int> LastPageIndices { get; set; } =
            new Dictionary<string, int>() { { "Notebook1", 0 } };

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
}
