using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dicom;

namespace MyPACSViewer.Model
{
    class DicomIndexModel
    {
        public DicomFileMetaInformation DicomMetaInfo { get; set; }
        public bool IsValid { get; set; }

        public DicomIndexModel()
        {
            IsValid = false;
        }

        public DicomIndexModel(string filePath)
        {
            try
            {
                DicomFile dcmFile =DicomFile.Open(filePath);
                DicomMetaInfo = dcmFile.FileMetaInfo;
                IsValid = true;
            }
            catch
            {
                IsValid = false;
            }
        }
    }
}
