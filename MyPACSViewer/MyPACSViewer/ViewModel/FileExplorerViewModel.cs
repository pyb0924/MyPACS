using MyPACSViewer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace MyPACSViewer.ViewModel
{
    class FileExplorerViewModel : ViewModelBase
    {
        private ObservableCollection<FileNodeModel> _fileTreeDataList;
        public ObservableCollection<FileNodeModel> FileTreeDataList
        {
            get => _fileTreeDataList;
            set
            {
                _fileTreeDataList = value;
                RaisePropertyChanged(() => FileTreeDataList);
            }
        }
    }
}
