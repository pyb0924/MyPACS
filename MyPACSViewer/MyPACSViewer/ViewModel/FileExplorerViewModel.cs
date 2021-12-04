using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using System.Collections.ObjectModel;
using MyPACSViewer.Model;

namespace MyPACSViewer.ViewModel
{
    class FileExplorerViewModel:ViewModelBase
    {
        public ObservableCollection<DicomIndexModel> ChildList { get; set; } = new();

        
    }
}
