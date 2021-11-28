using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using MyPACSViewer.Command;
using MyPACSViewer.Model;

namespace MyPACSViewer.ViewModel
{
    class ToolbarOpenFileViewModel : ToolbarViewModel
    {

        public string DicomRootPath { get; set; }
        public ToolbarOpenFileViewModel()
        {
            Source = Properties.Resources.openFileIcon;
            Text = Properties.Resources.openFileStr;
        }

        private void GetDicomFilePath()
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Title = "Open DICOM File";
            openFileDialog.Filter = "DICOM Files|*.dcm";
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.DefaultExt = "dcm";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                DicomRootPath = openFileDialog.FileName;
            }
        }

        public ICommand ClickedCommand => new CommandBase(GetDicomFilePath);
    }
}
