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

using FellowOakDicom;
using System.IO;
using FellowOakDicom.Imaging;
using FellowOakDicom.Imaging.Render;

namespace MyPACSViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<string, FileNodeItem> _DicomDict;
        private DicomDataset _MainDataset;
        private DicomDataset _MaskDataset;

        private enum DisplayMode { MODE_2D, MODE_3D };
        private DisplayMode _mode;


        public MainWindow()
        {
            InitializeComponent();
            _DicomDict = new();
            _mode = DisplayMode.MODE_2D;
        }

        // return count of files opened successfully
        private async Task<int> GenerateDicomDict(FileInfo[] files)
        {
            if (files.Length == 0)
            {
                throw new FileNotFoundException("No DICOM files found in selected folder! Please retry!");
            }
            _DicomDict.Clear();
            string tmp;
            DicomFile dcmFile;
            DicomDataset dcmDataSet;
            FileNodeItem patientNode, studyNode, seriesNode, imageNode;
            int errorCount = 0;
            foreach (FileInfo file in files)
            {
                try
                {
                    dcmFile = await DicomFile.OpenAsync(file.FullName);
                    dcmDataSet = dcmFile.Dataset;
                    tmp = dcmDataSet.GetString(DicomTag.PatientID);
                    if (_DicomDict.ContainsKey(tmp))
                    {
                        patientNode = _DicomDict[tmp];
                    }
                    else
                    {
                        patientNode = new(dcmDataSet.GetString(DicomTag.PatientName), FileNodeItem.PatientIcon);
                        _DicomDict.Add(tmp, patientNode);
                    }

                    tmp = dcmDataSet.GetString(DicomTag.StudyInstanceUID);
                    if (patientNode.Children.ContainsKey(tmp))
                    {
                        studyNode = patientNode.Children[tmp];
                    }
                    else
                    {
                        studyNode = new(dcmDataSet.GetString(DicomTag.Modality), FileNodeItem.StudyIcon);
                        patientNode.Children.Add(tmp, studyNode);
                    }

                    tmp = dcmDataSet.GetString(DicomTag.SeriesInstanceUID);
                    if (studyNode.Children.ContainsKey(tmp))
                    {
                        seriesNode = studyNode.Children[tmp];
                    }
                    else
                    {
                        seriesNode = new(dcmDataSet.GetString(DicomTag.SeriesDescription), FileNodeItem.SeriesIcon);
                        studyNode.Children.Add(tmp, seriesNode);
                    }

                    tmp = dcmDataSet.GetString(DicomTag.SOPInstanceUID);
                    if (seriesNode.Children.ContainsKey(tmp))
                    {
                        imageNode = seriesNode.Children[tmp];
                    }
                    else
                    {
                        imageNode = new(file.Name, FileNodeItem.InstanceIcon, file.FullName);
                        seriesNode.Children.Add(tmp, imageNode);
                    }
                }
                catch
                {
                    errorCount++;
                }
            }

            fileExplorer.ItemsSource = _DicomDict.Values;
            return files.Length - errorCount;
        }

        private void Display(DicomDataset dataset)
        {
            switch(_mode)
            {
                case DisplayMode.MODE_2D:
                    DicomImage dcmImage = new(dataset);
                    break;
                case DisplayMode.MODE_3D:
                    //TODO
                    break;
            }
            

        }

        void Display2DView(DicomDataset dataset, Constant.DisplayPlane plane, bool displayVR = false)
        {

        }

        // menu click 
        private void OpenFileMenu_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Title = "Open DICOM File";
            openFileDialog.Filter = "DICOM Files|*.dcm";
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.DefaultExt = "dcm";
            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                StatusBarText.Text = "Open File Failed!";
                return;
            }

            FileInfo[] files = { new FileInfo(openFileDialog.FileName) };
            try
            {
                _ = GenerateDicomDict(files);
                StatusBarText.Text = "Open File Successfully!";
            }
            catch (Exception ex)
            {
                StatusBarText.Text = ex.Message;
                return;
            }
            DicomDataset dataset = DicomFile.Open(openFileDialog.FileName).Dataset;
            Display(dataset);
        }

        private async void OpenFolderMenu_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog openFolderDialog = new();
            if (openFolderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                StatusBarText.Text = "Open Folder Failed!";
                return;
            }
            DirectoryInfo dir = new(openFolderDialog.SelectedPath);
            var files = dir.GetFiles("*.dcm", SearchOption.AllDirectories);
            try
            {
                int fileCount = await GenerateDicomDict(files);
                StatusBarText.Text = $"Open {fileCount} Files Successfully! Total: {files.Length} Files";
            }
            catch (Exception ex)
            {
                StatusBarText.Text = ex.Message;
                return;
            }

            DicomDataset dataset = DicomFile.Open(files[0].FullName).Dataset;
            Display(dataset);
        }

        private void QueryRetrieveMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExportMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExitMenu_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.MainWindow.Close();
        }


        // toolBar button click
        private void RevertBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Rotate_verticalBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Rotate_horizontalBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
