using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Dicom;
using System.IO;
using Dicom.Imaging;
using Dicom.Log;
using Dicom.Imaging.Render;

using MyPACSViewer.Utils;
using MyPACSViewer.Model;
using MyPACSViewer.View;
using MyPACSViewer.ViewModel;


namespace MyPACSViewer
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
   
        public MainWindow()
        {
            InitializeComponent();
        }

        // return count of files opened successfully
        //private async Task<int> GenerateDicomDict(FileInfo[] files)
        //{
        //    if (files.Length == 0)
        //    {
        //        throw new FileNotFoundException("No DICOM files found in selected folder! Please retry!");
        //    }
        //    _DicomDict.Clear();
        //    string tmp;
        //    DicomFile dcmFile;
        //    DicomDataset dcmDataSet;
        //    FileNodeModel patientNode, studyNode, seriesNode, imageNode;
        //    int errorCount = 0;
        //    foreach (FileInfo file in files)
        //    {
        //        try
        //        {
        //            dcmFile = await DicomFile.OpenAsync(file.FullName);
        //            dcmDataSet = dcmFile.Dataset;
        //            tmp = dcmDataSet.GetString(DicomTag.PatientID);
        //            if (_DicomDict.ContainsKey(tmp))
        //            {
        //                patientNode = _DicomDict[tmp];
        //            }
        //            else
        //            {
        //                patientNode = new(dcmDataSet.GetString(DicomTag.PatientName), FileNodeModel.PatientIcon);
        //                _DicomDict.Add(tmp, patientNode);
        //            }

        //            tmp = dcmDataSet.GetString(DicomTag.StudyInstanceUID);
        //            if (patientNode.Children.ContainsKey(tmp))
        //            {
        //                studyNode = patientNode.Children[tmp];
        //            }
        //            else
        //            {
        //                studyNode = new(dcmDataSet.GetString(DicomTag.Modality), FileNodeModel.StudyIcon);
        //                patientNode.Children.Add(tmp, studyNode);
        //            }

        //            tmp = dcmDataSet.GetString(DicomTag.SeriesInstanceUID);
        //            if (studyNode.Children.ContainsKey(tmp))
        //            {
        //                seriesNode = studyNode.Children[tmp];
        //            }
        //            else
        //            {
        //                seriesNode = new(dcmDataSet.GetString(DicomTag.SeriesDescription), FileNodeModel.SeriesIcon);
        //                studyNode.Children.Add(tmp, seriesNode);
        //            }

        //            tmp = dcmDataSet.GetString(DicomTag.SOPInstanceUID);
        //            if (seriesNode.Children.ContainsKey(tmp))
        //            {
        //                imageNode = seriesNode.Children[tmp];
        //            }
        //            else
        //            {
        //                imageNode = new(file.Name, FileNodeModel.InstanceIcon, file.FullName);
        //                seriesNode.Children.Add(tmp, imageNode);
        //            }
        //        }
        //        catch
        //        {
        //            errorCount++;
        //        }
        //    }

        //    return files.Length - errorCount;
        //}
    }
}
