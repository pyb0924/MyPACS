using System.Collections.Generic;


namespace MyPACSViewer.Model
{
    internal class FileNodeModel
    {
        public string DisplayName { get; set; }
        public string Icon { get; set; }
        public string Path { get; set; }
        public Dictionary<string, FileNodeModel> Children { get; set; } = new();
        public bool IsExpanded { get; set; }
        public bool IsSelected { get; set; }

        public int Index { get; set; }

        public FileNodeModel(string name, string icon, string path = null, int index = -1)
        {
            DisplayName = name;
            Icon = icon;
            Path = path;
            Index = index;
        }

    }
}
