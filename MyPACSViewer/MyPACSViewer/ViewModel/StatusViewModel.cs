using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight;

namespace MyPACSViewer.ViewModel
{
    class StatusViewModel : ViewModelBase
    {
        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                RaisePropertyChanged(() => Text);
            }
        }

        public StatusViewModel()
        {
            Messenger.Default.Register<string>(this, Properties.Resources.messageKey_status, msg =>
            {
                Text = msg;
            });
        }
    }
}
