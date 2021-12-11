using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;

namespace MyPACSViewer.ViewModel
{
    class ExitViewModel : ToolbarViewModel
    {
        public ExitViewModel()
        {
            Source = Properties.Resources.exitIcon;
            Text = Properties.Resources.exitStr;
        }

        public ICommand ExitCommmand => new RelayCommand(() => Application.Current.Shutdown());
    }
}
