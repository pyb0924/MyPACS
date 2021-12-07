using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MyPACSViewer.Model;
using GalaSoft.MvvmLight;

namespace MyPACSViewer.ViewModel
{
    internal abstract class ToolbarViewModel : ViewModelBase
    {
        private ToolbarModel _toolbarModel;

        public ToolbarModel ToolbarModel
        {
            get => _toolbarModel;
            set => _toolbarModel = value;
        }

        public ToolbarViewModel()
        {
            _toolbarModel = new();
        }
        public string Source
        {
            get => ToolbarModel.Source;
            set
            {
                if (ToolbarModel.Source != value)
                {
                    ToolbarModel.Source = value;
                    RaisePropertyChanged(() => Source);
                }
            }
        }

        public string Text
        {
            get => ToolbarModel.Text;
            set
            {
                if (ToolbarModel.Text != value)
                {
                    ToolbarModel.Text = value;
                    RaisePropertyChanged(() => Text);
                }
            }
        }
    }
}
