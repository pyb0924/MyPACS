using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;

namespace MyPACSViewer.ViewModel
{
    class OpenQRViewModel : ToolbarViewModel
    {
        public OpenQRViewModel()
        {
            Source = Properties.Resources.netIcon;
            Text = Properties.Resources.QRStr;
        }
        private void OpenQRWindow()
        {
            QRConfigWindow qrConfigWindow = new();
            qrConfigWindow.ShowDialog();
        }
        public ICommand OpenQRWindowCommand => new RelayCommand(OpenQRWindow);
    }
}
