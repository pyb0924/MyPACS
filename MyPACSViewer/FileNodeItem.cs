using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using FellowOakDicom;

namespace MyPACSViewer
{
    internal class FileNodeItem
    {
        public string DisplayName { get; set; }
        public string Icon { get; set; }
        public string Path { get; set; }
        public static readonly string PatientIcon = "/Resources/patient.png";
        public static readonly string StudyIcon = "/Resources/study.png";
        public static readonly string SeriesIcon = "/Resources/series.png";
        public static readonly string InstanceIcon = "/Resources/instance.png";

        public Dictionary<string, FileNodeItem> Children { get; set; }

        public FileNodeItem()
        {
            Children = new();
        }

        public FileNodeItem(string name, string icon, string path = null)
        {
            DisplayName = name;
            Icon = icon;
            Path = path;
            Children = new();
        }
    }
}
