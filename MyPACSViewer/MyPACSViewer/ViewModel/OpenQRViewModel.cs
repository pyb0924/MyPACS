using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

namespace MyPACSViewer.ViewModel
{
    class OpenQRViewModel : ToolbarViewModel
    {
        private QRConfigWindow qrConfigWindow;
        public OpenQRViewModel()
        {
            Source = Properties.Resources.netIcon;
            Text = Properties.Resources.QRStr;
            qrConfigWindow = new();
            Messenger.Default.Register<string>(this, Properties.Resources.messageKey_close, msg =>
            {
                qrConfigWindow.DialogResult = false;
                qrConfigWindow = new();
            });
        }
        public ICommand OpenQRWindowCommand => new RelayCommand(() => qrConfigWindow.ShowDialog());
    }
}
