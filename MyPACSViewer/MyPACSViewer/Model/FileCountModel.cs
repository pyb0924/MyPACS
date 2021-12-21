using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPACSViewer.Model
{
    class FileCountModel
    {
        public int PatientCount { get; set; }
        public int StudyCount { get; set; }
        public int SeriesCount { get; set; }
        public int InstanceCount { get; set; }
    }
}
