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
    class DetectViewModel:ToolbarViewModel
    {
        private bool _isMaskMode;
        public DetectViewModel()
        {
            Source = Properties.Resources.detectIcon;
            Text = Properties.Resources.detectStr;
            _isMaskMode = false;
        }

        public ICommand ToggleMaskCommand => new RelayCommand(() =>
        {
            _isMaskMode = !_isMaskMode;
            if(_isMaskMode)
            {
                Source = Properties.Resources.imageIcon;
                Text = Properties.Resources.showOriginalStr;
            }
            else
            {
                Source = Properties.Resources.detectIcon;
                Text = Properties.Resources.detectStr;
            }
            Messenger.Default.Send(_isMaskMode, Properties.Resources.messageKey_detect);
        });
    }
}
