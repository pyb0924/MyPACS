using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FellowOakDicom;

namespace MyPACSViewer.Model
{
    class QRListItem
    {
        public bool IsSelected { get; set; }
        public string PatientName { get; set; }
        public string PatientID { get; set; }
        public string StudyInstanceUID { get; set; }
        public string Modality { get; set; }
        public string BodyPartExamined { get; set; }
        public string SeriesDescription { get; set; }
        public string SeriesInstanceUID { get; set; }

        public QRListItem(DicomDataset dataset, bool isSelected = true)
        {
            IsSelected = isSelected;
            PatientName = dataset.GetSingleValueOrDefault(DicomTag.PatientName, string.Empty);
            PatientID = dataset.GetSingleValueOrDefault(DicomTag.PatientID, string.Empty);
            StudyInstanceUID = dataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty);
            Modality = dataset.GetSingleValueOrDefault(DicomTag.Modality, string.Empty);
            BodyPartExamined = dataset.GetSingleValueOrDefault(DicomTag.BodyPartExamined, string.Empty);
            SeriesDescription = dataset.GetSingleValueOrDefault(DicomTag.SeriesDescription, string.Empty);
            SeriesInstanceUID = dataset.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, string.Empty);
        }
    }
}
