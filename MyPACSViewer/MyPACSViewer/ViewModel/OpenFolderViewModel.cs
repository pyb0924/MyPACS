using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;

namespace MyPACSViewer.ViewModel
{
    class OpenFolderViewModel : ToolbarViewModel
    {
        public OpenFolderViewModel()
        {
            Source = Properties.Resources.openFolderIcon;
            Text = Properties.Resources.openFolderStr;
        }
        public string DicomRootPath { get; set; }
        private void GetDicomFolderPath()
        {

        }


        public ICommand OpenFolderCommand => new RelayCommand(() =>
        {
            FolderBrowserDialog openFolderDialog = new();
            if (openFolderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                // StatusBarText.Text = "Open Folder Failed!";
                return;
            }
            DicomRootPath = openFolderDialog.SelectedPath;
        });
    }
}
