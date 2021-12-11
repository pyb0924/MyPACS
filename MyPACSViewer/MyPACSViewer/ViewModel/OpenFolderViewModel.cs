using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

namespace MyPACSViewer.ViewModel
{
    class OpenFolderViewModel : ToolbarViewModel
    {
        public OpenFolderViewModel()
        {
            Source = Properties.Resources.openFolderIcon;
            Text = Properties.Resources.openFolderStr;
        }

        public ICommand OpenFolderCommand => new RelayCommand(() =>
        {
            FolderBrowserDialog openFolderDialog = new();
            if (openFolderDialog.ShowDialog() != DialogResult.OK)
            {
                Messenger.Default.Send("Open Folder Failed!", Properties.Resources.messageKey_status);
                return;
            }
            Messenger.Default.Send(openFolderDialog.SelectedPath, Properties.Resources.messageKey_folder);
        });
    }
}
