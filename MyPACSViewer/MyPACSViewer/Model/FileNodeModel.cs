using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using FellowOakDicom;

namespace MyPACSViewer.Model
{
    internal class FileNodeModel
    {
        public string DisplayName { get; set; }
        public string Icon { get; set; }
        public string Path { get; set; }
        public static readonly string PatientIcon = "/Resources/patient.png";
        public static readonly string StudyIcon = "/Resources/study.png";
        public static readonly string SeriesIcon = "/Resources/series.png";
        public static readonly string InstanceIcon = "/Resources/instance.png";

        public ObservableCollection<FileNodeModel> Children { get; set; } = new();
        public bool IsExpanded { get; set; }
        public bool IsSelected { get; set; }

        public FileNodeModel(string name, string icon, string path = null)
        {
            DisplayName = name;
            Icon = icon;
            Path = path;
        }


    }
}
