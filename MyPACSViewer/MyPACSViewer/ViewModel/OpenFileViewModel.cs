using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

namespace MyPACSViewer.ViewModel
{
    class OpenFileViewModel : ToolbarViewModel
    {
        public OpenFileViewModel()
        {
            Source = Properties.Resources.openFileIcon;
            Text = Properties.Resources.openFileStr;
        }

        public ICommand OpenFileCommand => new RelayCommand(() =>
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Title = Properties.Resources.openFileDialogTitle;
            openFileDialog.Filter = Properties.Resources.openFileDialogFilter;
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.DefaultExt = Properties.Resources.dicomExt;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Messenger.Default.Send(openFileDialog.FileName, Properties.Resources.messageKey_file);
            }
        });
    }
}
