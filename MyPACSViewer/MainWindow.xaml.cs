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
using MyPACSViewer.DataItems;
using MyPACSViewer.Components;

namespace MyPACSViewer
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region FIELDS

        private readonly Dictionary<string, FileNodeItem> _DicomDict;
        private DicomDataset _MainDataset;
        private DicomDataset _MaskDataset;
        private bool _UseMask;
        private DisplayMode _DisplayMode;
        private SpatialTransform _Transform;

        #endregion

        #region CONSTRUCTOR
        public MainWindow()
        {
            InitializeComponent();
            _DicomDict = new();
            _UseMask = false;
            _DisplayMode = DisplayMode.MODE_2D;
            ImageManager.SetImplementation(WPFImageManager.Instance);
            LogManager.SetImplementation(null);
        }
        #endregion

        #region INNER_METHOD

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

            FileExplorer.ItemsSource = new List<FileNodeItem>(_DicomDict.Values);
            return files.Length - errorCount;
        }

        private void Display()
        {
            switch (_DisplayMode)
            {
                case DisplayMode.MODE_2D:
                    Render2DView();
                    break;
                case DisplayMode.MODE_3D:
                    Render3DView(Display3DPlane.TRANSVERSE);
                    Render3DView(Display3DPlane.CORONAL);
                    Render3DView(Display3DPlane.SAGITTAL);
                    //Render3DView(Display3DPlane.VOLUME_RENDERING);
                    break;
                default:
                    break;
            }
            ChangeVisibility();
        }

        private void ChangeVisibility()
        {
            switch (_DisplayMode)
            {
                case DisplayMode.MODE_2D:
                    Viewer3D.Visibility = Visibility.Hidden;
                    Viewer2D.Visibility = Visibility.Visible;
                    break;
                case DisplayMode.MODE_3D:
                    Viewer2D.Visibility = Visibility.Hidden;
                    Viewer3D.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private void Render2DView()
        {
            DicomImage image = new(_MainDataset);

            WriteableBitmap bitmap = image.RenderImage().As<WriteableBitmap>();
            Viewer2D.Source = bitmap;
        }

        private void Render3DView(Display3DPlane plane)
        {

        }

        #endregion
        // menu click 

        #region BUTTON_CLICK

        private async void OpenFileMenu_Click(object sender, RoutedEventArgs e)
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
                _ = await GenerateDicomDict(files);
                StatusBarText.Text = "Open File Successfully!";
            }
            catch (Exception ex)
            {
                StatusBarText.Text = ex.Message;
                return;
            }
            _MainDataset = DicomFile.Open(openFileDialog.FileName).Dataset;
            Display();
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

            _MainDataset = DicomFile.Open(files[0].FullName).Dataset;
            Display();
        }

        private void QueryRetrieveMenu_Click(object sender, RoutedEventArgs e)
        {
            QRConfigWindow qrConfigWindow = new();
            qrConfigWindow.ShowDialog();
        }

        private void ExportMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExitMenu_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.MainWindow.Close();
        }


        // toolBar button click

        private void FlipHorizontalBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FlipVerticalBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SplitMergeBtn_Click(object sender, RoutedEventArgs e)
        {
            ToolbarButton toolbarButton = (ToolbarButton)sender;
            switch (_DisplayMode)
            {
                case DisplayMode.MODE_2D:
                    _DisplayMode = DisplayMode.MODE_3D;
                    toolbarButton.Source = "/Resources/window-merge.png";
                    toolbarButton.Text = "2D View";
                    break;
                case DisplayMode.MODE_3D:
                    _DisplayMode = DisplayMode.MODE_2D;
                    toolbarButton.Source = "/Resources/window-split.png";
                    toolbarButton.Text = "3D View";
                    break;
                default:
                    break;
            }
            Display();
        }

        private void ResetFocusBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (_DisplayMode)
            {
                case DisplayMode.MODE_2D:
                    Viewer2D.FocusX = Viewer2D.ActualWidth / 2;
                    Viewer2D.FocusY = Viewer2D.ActualHeight / 2;
                    break;
                case DisplayMode.MODE_3D:
                    break;
                default:
                    break;
            }
        }

        private void RevertBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion



        private void FileExplorer_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FileExplorer.SelectedItem != null)
            {
                FileNodeItem fileNodeItem = FileExplorer.SelectedItem as FileNodeItem;
                if (fileNodeItem.Path == null) return;
                _MainDataset = DicomFile.Open(fileNodeItem.Path).Dataset;
                Display();
            }
        }

        
    }
}
