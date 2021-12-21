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
    class DetectViewModel : ToolbarViewModel
    {
        private bool _isOverlayMode;
        public DetectViewModel()
        {
            Source = Properties.Resources.detectIcon;
            Text = Properties.Resources.detectStr;
            _isOverlayMode = false;
            Messenger.Default.Register<bool>(this, Properties.Resources.messageKey_overlayModeChanged, mode =>
            {
                _isOverlayMode = mode;
                SyncViewWithMode();
            });
        }

        private void SyncViewWithMode()
        {
            if (_isOverlayMode)
            {
                Source = Properties.Resources.lungIcon;
                Text = Properties.Resources.showOriginalStr;
            }
            else
            {
                Source = Properties.Resources.detectIcon;
                Text = Properties.Resources.detectStr;
            }
        }

        public ICommand ToggleOverlayCommand => new RelayCommand(() =>
        {
            Messenger.Default.Send(!_isOverlayMode, Properties.Resources.messageKey_detect);
        });
    }
}
