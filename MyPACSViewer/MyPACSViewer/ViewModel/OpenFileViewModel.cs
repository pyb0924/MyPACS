using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using MyPACSViewer.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace MyPACSViewer.ViewModel
{
    class OpenFileViewModel : ToolbarViewModel
    {
        public string DicomRootPath { get; set; }
        public OpenFileViewModel()
        {
            Source = Properties.Resources.openFileIcon;
            Text = Properties.Resources.openFileStr;
        }

        public ICommand OpenFileCommand => new RelayCommand(() =>
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
        });
    }
}
