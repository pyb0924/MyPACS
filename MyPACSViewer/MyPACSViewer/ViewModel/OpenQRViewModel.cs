using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

namespace MyPACSViewer.ViewModel
{
    class OpenQRViewModel : ToolbarViewModel
    {
        private QRWindow _qrConfigWindow;
        public OpenQRViewModel()
        {
            Source = Properties.Resources.netIcon;
            Text = Properties.Resources.QRStr;
            _qrConfigWindow = new();
            Messenger.Default.Register<string>(this, Properties.Resources.messageKey_close, msg =>
            {
                _qrConfigWindow.DialogResult = false;
            });
        }
        public ICommand OpenQRWindowCommand => new RelayCommand(() =>
        {
            _qrConfigWindow = new();
            _qrConfigWindow.ShowDialog();
        });
    }
}
