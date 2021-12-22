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
    class AnnotationViewModel : ToolbarViewModel
    {
        private bool _isAnnotationMode;
        public AnnotationViewModel()
        {
            Source = Properties.Resources.annotationIcon;
            Text = Properties.Resources.annotationStr;
            _isAnnotationMode = false;
            Messenger.Default.Register<bool>(this, Properties.Resources.messageKey_annotationModeChange, mode =>
            {
                _isAnnotationMode = mode;
                SyncViewWithMode();
            });
        }

        private void SyncViewWithMode()
        {
            if (_isAnnotationMode)
            {
                Source = Properties.Resources.lungIcon;
                Text = Properties.Resources.showOriginalStr;
            }
            else
            {
                Source = Properties.Resources.annotationIcon;
                Text = Properties.Resources.annotationStr;
            }
        }

        public ICommand ToggleAnnotationCommand => new RelayCommand(() =>
        {
            Messenger.Default.Send(!_isAnnotationMode, Properties.Resources.messageKey_detect);
        });
    }
}
