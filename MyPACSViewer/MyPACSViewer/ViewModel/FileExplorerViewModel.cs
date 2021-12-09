using MyPACSViewer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using FellowOakDicom;
using System.IO;

namespace MyPACSViewer.ViewModel
{
    class FileExplorerViewModel : ViewModelBase
    {
        private Dictionary<string, FileNodeModel> _DicomDict;
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


        public FileExplorerViewModel()
        {
            Messenger.Default.Register<string>(this, Properties.Resources.messageKey_file, GenerateFromFile);
            Messenger.Default.Register<string>(this, Properties.Resources.messageKey_folder, GenerateFromFolder);
        }

        private async Task<bool> AddToDicomDict(FileInfo file, bool selected)
        {
            string tmp;
            DicomFile dcmFile;
            DicomDataset dcmDataSet;
            FileNodeModel patientNode, studyNode, seriesNode, imageNode;
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
                    patientNode = new(dcmDataSet.GetString(DicomTag.PatientName), Properties.Resources.patientIcon);
                    patientNode.IsExpanded = selected;
                    _DicomDict.Add(tmp, patientNode);
                }

                tmp = dcmDataSet.GetString(DicomTag.StudyInstanceUID);
                if (patientNode.Children.ContainsKey(tmp))
                {
                    studyNode = patientNode.Children[tmp];
                }
                else
                {
                    studyNode = new(dcmDataSet.GetString(DicomTag.Modality), Properties.Resources.studyIcon);
                    studyNode.IsExpanded = selected;
                    patientNode.Children.Add(tmp, studyNode);
                }

                tmp = dcmDataSet.GetString(DicomTag.SeriesInstanceUID);
                if (studyNode.Children.ContainsKey(tmp))
                {
                    seriesNode = studyNode.Children[tmp];
                }
                else
                {
                    seriesNode = new(dcmDataSet.GetString(DicomTag.SeriesDescription), Properties.Resources.seriesIcon);
                    seriesNode.IsExpanded = selected;
                    studyNode.Children.Add(tmp, seriesNode);
                }

                tmp = dcmDataSet.GetString(DicomTag.SOPInstanceUID);
                if (seriesNode.Children.ContainsKey(tmp))
                {
                    imageNode = seriesNode.Children[tmp];
                }
                else
                {
                    imageNode = new(file.Name, Properties.Resources.imageIcon, file.FullName);
                    imageNode.IsSelected = selected;
                    seriesNode.Children.Add(tmp, imageNode);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private async void GenerateFromFile(string file)
        {
            _DicomDict = new();
            FileInfo fileInfo = new(file);
            if (await AddToDicomDict(fileInfo, true))
            {
                FileTreeDataList = new(_DicomDict.Values);
                Messenger.Default.Send("Open File Successfully!", Properties.Resources.messageKey_status);
            }
            else
            {
                Messenger.Default.Send("Open File Failed!", Properties.Resources.messageKey_status);
            }
        }

        private async void GenerateFromFolder(string folder)
        {
            _DicomDict = new();
            DirectoryInfo dir = new(folder);
            FileInfo[] files = dir.GetFiles("*.dcm", SearchOption.AllDirectories);
            int fileCount = 0;
            bool isFirst = true;
            foreach (var file in files)
            {
                if (await AddToDicomDict(file, isFirst))
                {
                    fileCount++;
                }
                isFirst = false;
            }
            FileTreeDataList = new(_DicomDict.Values);
            Messenger.Default.Send(
                $"Open {fileCount} Files Successfully! Total: {files.Length} Files", Properties.Resources.messageKey_status);
        }
    }
}
