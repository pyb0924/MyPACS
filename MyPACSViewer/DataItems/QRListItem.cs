using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPACSViewer.DataItems
{
    class QRListItem
    {
        public bool IsSelected { get; set; }
        public string StudyUID { get; set; }
        public string SeriesUID { get; set; }

        public QRListItem(bool isSelected, string studyUID, string seriesUID)
        {
            IsSelected = isSelected;
            StudyUID = studyUID;
            SeriesUID = seriesUID;
        }
    }
}
